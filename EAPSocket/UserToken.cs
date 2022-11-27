using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace EAPSocket
{
	/// <summary>
	/// 클라이언트와의 연결 관리 및 송수신을 처리
	/// </summary>
	public class UserToken
	{
		private enum State
		{
			Idle = 0,
			Connected = 1,
			Closing = 2,
			Closed = 3
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private NetworkService m_service;
		private Socket? m_socket;

		private object m_syncObject;

		private SocketAsyncEventArgs m_receiveEventArgs;
		private SocketAsyncEventArgs m_sendEventArgs;

		private MessageResolver m_messageResolver;
		private Queue<Packet> m_sendPackets;
		private List<ArraySegment<byte>> m_sendSegments;

		private IPeer? m_peer;

		private long m_lnLastHeartBeatTicks;

		private State m_state;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="service">네트워크 통신을 관리하는 객체</param>
		/// <param name="nBufferSize">내부 버퍼의 크기</param>
		/// <param name="receiveEventArgs">수신 비동기 소켓 작업</param>
		/// <param name="sendEventArgs">송신 비동기 소켓 작업</param>
		internal UserToken(NetworkService service, int nBufferSize, SocketAsyncEventArgs receiveEventArgs, SocketAsyncEventArgs sendEventArgs)
		{
			if (service == null)
				throw new ArgumentNullException("service");

			if (nBufferSize <= 0)
				throw new ArgumentOutOfRangeException("nBufferSize");

			m_service = service;
			m_socket = null;

			m_syncObject = new object();

			m_receiveEventArgs = receiveEventArgs;
			m_receiveEventArgs.UserToken = this;
			m_receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompleteReceive);

			m_sendEventArgs = sendEventArgs;
			m_sendEventArgs.UserToken = this;
			m_sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompleteSend);

			m_messageResolver = new MessageResolver(nBufferSize);
			m_sendPackets = new Queue<Packet>();
			m_sendSegments = new List<ArraySegment<byte>>();

			m_peer = null;

			m_lnLastHeartBeatTicks = 0;

			m_state = State.Idle;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		internal Socket? socket
		{
			get { return m_socket; }
			set { m_socket = value; }
		}

		public string ipAddress
		{
			get { return ((IPEndPoint)m_socket!.RemoteEndPoint!).Address.ToString(); }
		}

		public int port
		{
			get { return ((IPEndPoint)m_socket!.RemoteEndPoint!).Port; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 사용자토큰을 수신이 가능한 상태로 전환하는 함수
		/// </summary>
		internal void Start()
		{
			if (socket == null)
				throw new ArgumentNullException("socket");

			m_state = State.Connected;
			m_lnLastHeartBeatTicks = DateTime.Now.Ticks;

			BeginReceive(m_receiveEventArgs);
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
		/// <param name="lnValidTimeTicks"></param>
		/// <returns>유효한 연결상태의 경우 true, 유효시간이 경과 됬을경우 false 반환</returns>
		public bool CheckHeartBeat(long lnValidTimeTicks)
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
				if (m_state != State.Connected)
					return;

				m_lnLastHeartBeatTicks = DateTime.Now.Ticks;
			}
		}

		//
		// Receive
		//

		/// <summary>
		/// 데이터 수신대기 시작 함수
		/// </summary>
		/// <param name="e">수신 비동기 소켓 작업</param>
		private void BeginReceive(SocketAsyncEventArgs e)
		{
			bool bPending = false;

			try
			{
				bPending = m_socket!.ReceiveAsync(e);
			}
			catch (Exception ex)
			{
				Console.WriteLine("BeginReceive() error - {0}", ex.Message);
				Close();

				return;
			}

			// 동기적으로 처리됬을 경우 직접 수신 데이터 확인 함수 호출
			if (!bPending)
				CompleteReceive(this, e);
		}

		/// <summary>
		/// 비동기 수신 작업이 완료됬을때 호출되는 함수
		/// </summary>
		/// <param name="sender">이벤트 소스</param>
		/// <param name="e">수신 비동기 소켓 작업</param>
		private void CompleteReceive(object? sender, SocketAsyncEventArgs e)
		{
			// 비동기 소켓 작업의 마지막 수행 작업 체크
			if (e.LastOperation == SocketAsyncOperation.Receive)
				ProcessReceive(e);
			else
				throw new ArgumentNullException("Receive operation error");
		}

		/// <summary>
		/// 수신한 데이터를 확인 하는 함수
		/// </summary>
		/// <param name="e">수신 비동기 소켓 작업</param>
		private void ProcessReceive(SocketAsyncEventArgs e)
		{
			// 수신한 데이터 크기가 0보다 크고 소켓에러가 성공일 경우
			if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
			{
				// 수신한 데이터를 완성된 패킷으로 변환
				m_messageResolver.OnReceive(e.Buffer!, e.Offset, e.BytesTransferred, OnRecive);

				BeginReceive(e);
			}
			// 수신받은 데이터 크기가 0 또는 0보다 작거나 소켓에러가 성공이 아닐 경우 소켓 종료
			else
			{
				Console.WriteLine("ProcessReceive() error - {0}, transferred - {1}", e.SocketError, e.BytesTransferred);
				Close();
			}
		}

		/// <summary>
		/// 패킷변환 완료시 호출되는 함수
		/// 패킷의 타입별로 이후 프로세스 진행
		/// </summary>
		/// <param name="buffer">처리된 데이터가 저장되어있는 버퍼</param>
		private void OnRecive(byte[] buffer)
		{
			Packet packet = Packet.Create(buffer);
			PacketType type = packet.PopPacketType();

			switch (type)
			{
				// 연결상태 요청의 경우
				// 연결상태 체크 시각 갱신 이후
				// 연결상태 확인 응답 메세지 송신
				case PacketType.SYS_HeartBeatREQ:
					{
						UpdateHeartBeat();
						SendHeartBeatACK();
					}
					break;

				// 사용자 메세지의 경우
				// 등록되 있는 사용자 피어의 메세지 수신 처리 함수 호출
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
		/// 연결상태 확인 응답 송신 함수
		/// </summary>
		private void SendHeartBeatACK()
		{
			Packet packet = PacketMaker.CreatePacket();
			packet.Push((byte)PacketType.SYS_HeartBeatACK);

			Send(packet);
		}

		/// <summary>
		/// 연결 종료 요청 송신 함수
		/// </summary>
		private void SendDisconnectSignal()
		{
			Packet packet = PacketMaker.CreatePacket();
			packet.Push((byte)PacketType.SYS_DisconnectSignal);

			Send(packet);
		}

		/// <summary>
		/// 메세지 송신 대기 함수
		/// </summary>
		/// <param name="packet">송신할 패킷</param>
		public void Send(Packet packet)
		{
			lock (m_syncObject)
			{
				if (m_state != State.Connected)
					return;

				packet.RecordSize();
				m_sendPackets.Enqueue(packet);

				// 송신중인 메세지가 있을 경우 리턴
				if (m_sendSegments.Count > 0)
					return;
			}

			// 송신중인 메세지가 없을 경우 송신큐에 대기중인 메세지를 송신준비 시작
			StartSend();
		}

		/// <summary>
		/// 송신대기큐의 패킷을 송신이 가능한 형태로 변환 하여 송신 시작
		/// </summary>
		private void StartSend()
		{
			lock (m_syncObject)
			{
				if (m_state != State.Connected)
					return;

				while (m_sendPackets.Count > 0)
				{
					Packet packet = m_sendPackets.Dequeue();

					m_sendSegments.Add(packet.ToArraySegment());
				}

				m_sendEventArgs.BufferList = m_sendSegments;
			}

			BeginSend();
		}

		/// <summary>
		/// 송신 처리 함수
		/// </summary>
		private void BeginSend()
		{
			bool bPending = false;

			try
			{
				bPending = m_socket!.SendAsync(m_sendEventArgs);
			}
			catch (Exception ex)
			{
				Console.WriteLine("BeginSend() error - {0}", ex.Message);
				Close();

				return;
			}

			// 동기적으로 처리됬을 경우 직접 송신 데이터 확인 함수 호출
			if (!bPending)
				CompleteSend(this, m_sendEventArgs);
		}

		/// <summary>
		/// 비동기 송신 작업이 완료됬을때 호출되는 함수
		/// </summary>
		/// <param name="sender">이벤트 소스</param>
		/// <param name="e">수신 비동기 소켓 작업</param>
		private void CompleteSend(object? sender, SocketAsyncEventArgs e)
		{
			// 비동기 소켓 작업의 마지막 수행 작업 체크
			if (e.LastOperation == SocketAsyncOperation.Send)
				ProcessSend(e);
			else
				throw new ArgumentException("Send operation error");
		}

		/// <summary>
		/// 송신한 데이터를 확인 하는 함수
		/// </summary>
		/// <param name="e">수신 비동기 소켓 작업</param>
		private void ProcessSend(SocketAsyncEventArgs e)
		{
			// 송신 완료시 송신 완료 함수 호출
			if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
				OnSend(e.BytesTransferred);
			// 송신 실패시 예외 발생 시켜 소켓 종료 처리
			else
			{
				Console.WriteLine(String.Format("ProcessSend() error - {0}, nSendBytes - {1}", e.SocketError, e.BytesTransferred));
				Close();
			}
		}

		/// <summary>
		/// 정상적으로 데이터 송신 완료시 호출되는 함수
		/// </summary>
		/// <param name="nSendBytes">송신한 데이터 크기</param>y
		private void OnSend(int nSendBytes)
		{
			lock (m_syncObject)
			{
				int nTotalCount = m_sendSegments.Sum(seg => seg.Count);

				if (nSendBytes < nTotalCount)
				{
					// 정상적인 비동기 송신의 경우 세그먼트 리스트의 모든 데이터를 SendBuffer에 입력 후 완료 이벤트 호출
					// 모든 데이터를 보내지 않았을 경우 종료 처리
					Close();

					return;

					/*
					int nRemain = nSendBytes;

					while (nRemain > 0)
					{
						ArraySegment<byte> segment = m_sendSegments.First();
						m_sendSegments.RemoveAt(0);

						if (segment.Count <= nRemain)
							nRemain -= segment.Count;
						else
						{
							ArraySegment<byte> newSegment = new ArraySegment<byte>(segment.Array!, segment.Offset + nRemain, segment.Count - nRemain);
							m_sendSegments.Insert(0, newSegment);

							nRemain = 0;
						}
					}
					*/
				}
				else
					m_sendSegments.Clear();

				if (m_sendPackets.Count <= 0 && m_sendSegments.Count <= 0)
					return;
			}

			// 송신할 메세지가 있을 경우 StartSend 함수 호출하여 다시 송신 절차 시작
			StartSend();
		}

		//
		//
		//

		/// <summary>
		/// 연결 종료 함수
		/// 바로 연결을 끊는것이 클라이언트측에서 먼저 연결을 종료하도록 메세지 전달
		/// </summary>
		public void Disconnect()
		{
			// 클라이언트에 연결종료 메세지 전달
			SendDisconnectSignal();

			lock (m_syncObject)
			{
				if (m_state != State.Connected)
					return;

				m_state = State.Closing;
			}

			// 토큰에 등록된 피어는 종료처리
			try
			{
				m_peer!.OnDisconnect();
			}
			catch
			{

			}
			finally
			{
				m_peer = null;
			}
		}

		/// <summary>
		/// 최종 종료 함수
		/// 정상적으로 수신데이터 크기가 0이 수신되서 정상 종료되어 호출되거나
		/// 소켓 에러 또는 연결상태 체크에서 생명주기가 경과됬을 경우 강제 종료가 필요하여 호출됨
		/// </summary>
		public void Close()
		{
			lock (m_syncObject)
			{
				if (m_state == State.Closed)
					return;

				m_state = State.Closed;

				m_socket!.Close();
				m_socket = null;

				m_messageResolver.Stop();
				m_sendPackets.Clear();
				m_sendSegments.Clear();
			}

			// Disconnect 함수가 이미 호출 되었을 경우 피어가 null
			if (m_peer != null)
			{
				try
				{
					m_peer.OnDisconnect();
				}
				catch
				{

				}
				finally
				{
					m_peer = null;
				}
			}

			// 네트워크 연결이 종료된 토큰 반납
			m_service.ReturnUserToken(this);
		}
	}
}
