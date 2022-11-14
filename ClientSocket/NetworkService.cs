using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ClientSocket
{
	/// <summary>
	/// 네트워크 통신의 모든 서비스를 관리
	/// </summary>
	public class NetworkService
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nBufferSize;

		private UserToken m_token;

		private long m_lnHeartBeatLifetimeTicks;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nBufferSize">입출력버퍼의 크기</param>
		/// <param name="nPreAllocBufferCount">사전할당버퍼개수</param>
		public NetworkService(int nBufferSize)
		{
			if (nBufferSize <= 0)
				throw new ArgumentOutOfRangeException("nBufferSize");

			m_nBufferSize = nBufferSize;

			m_token = null;

			m_lnHeartBeatLifetimeTicks = 0;

			onConnected = null;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Delegate / Event

		public delegate void ConnectHandler(UserToken token);

		public event ConnectHandler onConnected;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 초기화 함수
		/// </summary>
		public void Initialize()
		{
			// 패킷생성객체 초기화
			PacketMaker.Initialize(m_nBufferSize);

			// 수신 버퍼 생성 및 설정
			UserBuffer userBuffer = new UserBuffer();
			userBuffer.SetBuffer(new byte[m_nBufferSize], 0, m_nBufferSize);

			// 사용자 토큰 생성
			m_token = new UserToken(this, m_nBufferSize, userBuffer);
		}

		/// <summary>
		/// 연결 시작 함수
		/// </summary>
		/// <param name="sHost">서버 주소</param>
		/// <param name="nPort">서버 포트</param>
		/// <param name="nHeartBeatLifetime">생결상태 생명주기</param>
		public void Connect(string sHost, int nPort, int nHeartBeatLifetime = 60000)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			IPAddress address = null;
			if (sHost == "0.0.0.0")
				address = IPAddress.Any;
			else
				address = IPAddress.Parse(sHost);

			IPEndPoint endPoint = new IPEndPoint(address, nPort);

			socket.Connect(endPoint);
			// 소켓 논블로킹 설정
			socket.Blocking = false;

			OnConnect(socket);

			m_lnHeartBeatLifetimeTicks = nHeartBeatLifetime * TimeSpan.TicksPerMillisecond;
		}

		/// <summary>
		/// 연결시 호출되는 함수
		/// </summary>
		private void OnConnect(Socket socket)
		{
			if (onConnected != null)
				onConnected(m_token);

			m_token.Start(socket);
		}

		/// <summary>
		/// 송신과 수신을 처리하는 함수
		/// 일정 주기마다 호출 필요
		/// </summary>
		public void Service()
		{
			m_token.Service();

			long lnValidTimeTicks = DateTime.Now.Ticks - m_lnHeartBeatLifetimeTicks;
			if (!m_token.CheckHeartBeat(lnValidTimeTicks))
				m_token.Close();
		}
	}
}
