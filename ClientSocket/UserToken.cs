using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ClientSocket
{
    /// <summary>
    /// 서버의 연결 관리 및 송수신을 처리
    /// </summary>
    public class UserToken
    {
        public enum State
        {
            Idle = 0,
            Connected = 1,
            Closing = 2,
            Closed = 3
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // Member variables

        private NetworkService m_service;
        private Socket m_socket;

        private object m_syncObject;

        private UserBuffer m_receiveBuffer;

        private MessageResolver m_messageResolver;
        private Queue<Packet> m_sendPackets;
        private List<ArraySegment<byte>> m_sendSegments = new List<ArraySegment<byte>>();

        private IPeer m_peer;

        private long m_lnLastHeartBeatTicks;

        private State m_state;

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constructors

        /// <summary>
		/// 
		/// </summary>
		/// <param name="service">네트워크 통신을 관리하는 객체</param>
		/// <param name="nBufferSize">내부 버퍼의 크기</param>
		/// <param name="receiveBuffer">수신 버퍼</param>
        internal UserToken(NetworkService service, int nBufferSize, UserBuffer receiveBuffer)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            if (nBufferSize <= 0)
                throw new ArgumentOutOfRangeException("nBufferSize");

            if (receiveBuffer == null)
                throw new ArgumentNullException("receiveBuffer");

            m_service = service;
            m_socket = null;

            m_syncObject = new object();

            m_receiveBuffer = receiveBuffer;

            m_messageResolver = new MessageResolver(nBufferSize);
            m_sendPackets = new Queue<Packet>();
            m_sendSegments = new List<ArraySegment<byte>>();

            m_peer = null;

            m_lnLastHeartBeatTicks = 0;

            m_state = State.Idle;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // Member functions

        /// <summary>
		/// 사용자토큰을 수신이 가능한 상태로 전환하는 함수
		/// </summary>
		/// <param name="socket">클라이언트 소켓</param>
        internal void Start(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            m_socket = socket;

            m_lnLastHeartBeatTicks = DateTime.Now.Ticks;

            m_state = State.Connected;
        }

        /// <summary>
        /// 수신 및 송신 함수를 호출 하는 함수
        /// 일정 시간마다 호출 필요
        /// </summary>
        public void Service()
        {
            if (m_state == State.Closed)
                return;

            // 수신
            Receive();

            // 연결상태 확인 요청 전송
            SendHeartBeatREQ();
            // 송신
            StartSend();
        }

        //
        // Peer
        //

        /// <summary>
		/// 사용자 피어를 설정하는 함수
		/// </summary>
		/// <param name="peer">사용자 피어 객체</param>
        public void SetPeer(IPeer peer)
        {
            if (peer == null)
                throw new ArgumentNullException("peer");

            m_peer = peer;
        }

        //
        // HeartBeat
        //

        /// <summary>
		/// 소켓의 마지막 연결상태 체크 시각을 확인하는 함수
		/// </summary>
		/// <param name="lnValidTimeTicks">유효한 시간틱</param>
		/// <returns>유효한 연결상태의 경우 true, 유효시간이 경과 됬을경우 false 반환</returns>
        internal bool CheckHeartBeat(long lnValidTimeTicks)
        {
            lock (m_syncObject)
            {
                return lnValidTimeTicks < m_lnLastHeartBeatTicks;
            }
        }

        /// <summary>
		/// 소켓의 연결상태 체크 시각을 갱신하는 함수
		/// </summary>
        private void UpdateHeartBeat()
        {
            lock (m_syncObject)
            {
                m_lnLastHeartBeatTicks = DateTime.Now.Ticks;
            }
        }

        //
        // Receive
        //

        /// <summary>
		/// 데이터 수신 함수
		/// </summary>
        private void Receive()
        {
            try
            {
                SocketError error;
                // 논블로킹 소켓이므로 수신 데이터와 상관 없이 즉시 반환
                int nReceiveBytes = m_socket.Receive(m_receiveBuffer.buffer, m_receiveBuffer.offset, m_receiveBuffer.count, SocketFlags.None, out error);

                // 수신 데이터가 없을 경우 WouldBlock 소켓 에러 출력
                if (error == SocketError.WouldBlock)
                    return;

                // 수신받은 데이터 크기가 0 보다 크고 소켓에러가 성공일 경우 사용자토큰 수신성공 함수 호출
                if (nReceiveBytes > 0 && error == SocketError.Success)
                    m_messageResolver.OnReceive(m_receiveBuffer.buffer, m_receiveBuffer.offset, nReceiveBytes, OnReceive);
                else
                    throw new Exception(String.Format("error - {0}, nReceiveBytes - {1}", error, nReceiveBytes));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Receive() {0}", ex.Message);
                Close();
            }
        }

        /// <summary>
		/// 패킷변환 완료시 호출되는 함수
		/// 패킷의 타입별로 이후 프로세스 진행
		/// </summary>
		/// <param name="buffer">처리된 데이터가 저장되어있는 버퍼</param>
        private void OnReceive(byte[] buffer)
        {
            Packet packet = Packet.Create(buffer);
            PacketType type = packet.PopPacketType();

            switch (type)
            {
                // 연결상태 확인 응답의 경우
                // 연결상태 체크 시각 갱신
                case PacketType.SYS_HeartBeatACK:
                    {
                        UpdateHeartBeat();
                    }
                    break;

                // 연결 종료 요청 신호의 경우
                // 연결 종료 함수 호출
                case PacketType.SYS_DisconnectSignal:
                    {
                        Disconnect();
                    }
                    break;

                // 상요자 메제시의 경우
                // 등록되어 있는 사용자 피어의 메세지 수신 처리 함수 호출
                case PacketType.USER_Message:
                    {
                        if (m_peer != null)
                            m_peer.OnReceive(packet);
                    }
                    break;
            }
        }

        //
        // Send
        //

        /// <summary>
		/// 연결상태 확인 요청 전송 함수
		/// </summary>
        private void SendHeartBeatREQ()
        {
            Packet packet = PacketMaker.CreatePacket();
            packet.Push((byte)PacketType.SYS_HeartBeatREQ);

            Send(packet);
        }

        /// <summary>
		/// 메세지 전송 대기 함수
		/// </summary>
		/// <param name="packet">전송할 패킷</param>
        public void Send(Packet packet)
        {
            lock (m_syncObject)
            {
                if (m_state != State.Connected)
                    return;

                packet.RecordSize();
                m_sendPackets.Enqueue(packet);
            }
        }

        /// <summary>
		/// 전송대기큐의 패킷을 전송이 가능한 형태로 변환 하여 전송 시작
		/// </summary>
        private void StartSend()
        {
            lock (m_syncObject)
            {
                if (m_state != State.Connected)
                    return;

                // 전송대기큐의 데이터가 없을 경우 리턴
                if (m_sendPackets.Count <= 0)
                    return;

                // 전송중인 메세지가 있을 경우 리턴
                if (m_sendSegments.Count > 0)
                    return;

                while (m_sendPackets.Count > 0)
                {
                    Packet packet = m_sendPackets.Dequeue();

                    m_sendSegments.Add(packet.ToArraySegment());
                }
            }

            ProcessSend();
        }

        /// <summary>
        /// 데이터 전송 함수
        /// </summary>
        private void ProcessSend()
        {
            try
            {
                SocketError error;
                int nSendBytes = m_socket.Send(m_sendSegments, SocketFlags.None, out error);

                // 전송 완료시 전송 완료 함수 호출
                if (nSendBytes > 0 && error == SocketError.Success)
                    OnSend(nSendBytes);
                // 전송 실패시 예외 발생 시켜 소켓 종료 처리
                else
                    throw new Exception(String.Format("error - {0}, nSendBytes - {1}", error, nSendBytes));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ProcessSend {0}", ex.Message);
                Close();
            }
        }

        /// <summary>
		/// 정상적으로 데이터 송신 완료시 호출되는 함수
		/// </summary>
		/// <param name="nSendBytes">송신한 데이터 크기</param>
        private void OnSend(int nSendBytes)
        {
            lock (m_syncObject)
            {
                // 논블로킹 소켓의 경우 전송 시 모든 데이터가 전송되지 않을수 있으므로
                // 전송한 데이터 삭제 처리
                int nTotalCount = m_sendSegments.Sum(seg => seg.Count);

                if (nSendBytes < nTotalCount)
                {
                    int nRemain = nSendBytes;

                    while (nRemain > 0)
                    {
                        ArraySegment<byte> segment = m_sendSegments.First();
                        m_sendSegments.RemoveAt(0);

                        if (segment.Count <= nRemain)
                            nRemain -= segment.Count;
                        else
                        {
                            ArraySegment<byte> newSegment = new ArraySegment<byte>(segment.Array, segment.Offset + nRemain, segment.Count - nRemain);
                            m_sendSegments.Insert(0, newSegment);

                            nRemain = 0;
                        }
                    }
                }
                else
                    m_sendSegments.Clear();

                // 사용자토큰의 상태가 종료 처리중일 경우 종료 처리
                if (m_state == State.Closing)
                    m_socket.Shutdown(SocketShutdown.Send);
            }
        }

        //
        //
        //

        /// <summary>
        /// 연결 종료 함수
        /// </summary>
        public void Disconnect()
        {
            lock (m_syncObject)
            {
                if (m_state != State.Connected)
                    return;

                m_state = State.Closing;

                try
                {
                    m_peer.OnRemoved();
                }
                catch
                {

                }
                finally
                {
                    m_peer = null;
                }

                // 전송중인 메세지가 있을 경우 전송 이후 처리
                if (m_sendSegments.Count > 0)
                    return;

                m_socket.Shutdown(SocketShutdown.Send);
            }
        }

        /// <summary>
        /// 최종 종료 함수
        /// </summary>
        internal void Close()
        {
            lock (m_syncObject)
            {
                if (m_state == State.Closed)
                    return;

                m_state = State.Closed;

                // Disconnect 함수가 이미 호출 되었을 경우 피어가 null
                if (m_peer != null)
                {
                    try
                    {
                        m_peer.OnRemoved();
                    }
                    catch
                    {

                    }
                    finally
                    {
                        m_peer = null;
                    }
                }

                m_socket.Close();
                m_socket = null;

                m_messageResolver.ClearBuffer();
                m_sendPackets.Clear();
                m_sendSegments.Clear();
            }
        }
    }
}
