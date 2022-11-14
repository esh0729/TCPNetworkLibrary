using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace EAPSocket
{
	/// <summary>
	/// 네트워크 통신의 모든 서비스를 관리
	/// </summary>
	public class NetworkService
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nMaxConnections;
		private int m_nBufferSize;
		private int m_nPreAllocBufferCount;

		private Listener? m_listener;

		private BufferManager m_bufferManager;

		private UserTokenPool m_userTokenPool;
		private UserTokenManager m_userTokenManager;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Delegate / Event

		// 클라이언트 연결 완료시 호출되는 대리자
		public delegate void CreateSessionHandler(UserToken token);

		private event CreateSessionHandler? m_onCreatedSession;
		public event CreateSessionHandler onCreatedSession
		{
			add { m_onCreatedSession += value; }
			remove { m_onCreatedSession -= value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public NetworkService(int nMaxConnections, int nBufferSize, int nPreAllocBufferCount)
		{
			if (nMaxConnections <= 0)
				throw new ArgumentOutOfRangeException("nMaxConnections");

			if (nBufferSize <= 0)
				throw new ArgumentOutOfRangeException("nBufferSize");

			if (nPreAllocBufferCount <= 0)
				throw new ArgumentNullException("nPreAllocBufferCount");

			m_nMaxConnections = nMaxConnections;
			m_nBufferSize = nBufferSize;
			m_nPreAllocBufferCount = nPreAllocBufferCount;

			m_listener = null;

			// 최대 접속자수 * 버퍼 크기 * 사전할당버퍼 개수 크기의 모든 송수신 버퍼 배열 생성
			m_bufferManager = new BufferManager(m_nMaxConnections * m_nBufferSize * m_nPreAllocBufferCount, m_nBufferSize);

			// 사용자토큰풀 최대 접속자 수를 가진 풀의 크기로 생성
			m_userTokenPool = new UserTokenPool(m_nMaxConnections);

			// 사용자토큰관리자 생성
			m_userTokenManager = new UserTokenManager();	

			m_onCreatedSession = null;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 초기화 함수
		/// </summary>
		public void Initialize()
		{
			// 패킷생성객체 초기화
			PacketMaker.Initialize(m_nBufferSize);

			// 모든 사용자 버퍼를 처리하는 배열 생성
			m_bufferManager.InitBuffer();

			// 사용자 토큰 생성 및 비동기 송수신 작업을 처리하는 객체 생성 후 토큰에 할당
			UserToken token;
			for (int i = 0; i < m_nMaxConnections; i++)
			{
				SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
				m_bufferManager.SetBuffer(receiveEventArgs);

				// 송신의 경우 BufferList를 사용하기 때문에 별도의 버퍼를 할당받지 않음
				SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();

				token = new UserToken(this, m_nBufferSize, receiveEventArgs, sendEventArgs);

				m_userTokenPool.Push(token);
			}
		}

		/// <summary>
		/// 리스너 생성 및 연결요청대기 시작 함수
		/// </summary>
		/// <param name="sHost">서버 주소</param>
		/// <param name="nPort">서버 포트</param>
		/// <param name="nBacklog">연결요청대기큐 크기</param>
		/// <param name="nHeartBeatCheckInterval">연결상태 체크 간격</param>
		/// <param name="nHeartBeatLifetime">연결상태 생명주기</param>
		public void Listen(string sHost, int nPort, int nBacklog, int nHeartBeatCheckInterval = 1000, int nHeartBeatLifetime = 60000)
		{
			if (nHeartBeatCheckInterval <= 0)
				throw new ArgumentOutOfRangeException("nHeartBeatCheckInterval");

			if (nHeartBeatLifetime <= 0)
				throw new ArgumentOutOfRangeException("nHeartBeatLifetime");

			m_listener = new Listener();
			m_listener.onConnectClient += OnConnectionClient;
			m_listener.Start(sHost, nPort, nBacklog);

			// 접속한 사용자토큰 연결상태 체크 시각 설정
			m_userTokenManager.StartHeartBeat(nHeartBeatCheckInterval, nHeartBeatLifetime);
		}

		/// <summary>
		/// 클라이언트 연결시 효출되는 함수
		/// </summary>
		/// <param name="socket">연결된 클라이언트 소켓</param>
		private void OnConnectionClient(Socket socket)
		{
			UserToken token = m_userTokenPool!.Pop();

			if (m_onCreatedSession != null)
				m_onCreatedSession(token);

			m_userTokenManager.AddUserToken(token);

			token.Start(socket);
		}

		/// <summary>
		/// 사용자토큰 반납 함수
		/// </summary>
		/// <param name="token">반납할 사용자토큰</param>
		internal void ReturnUserToken(UserToken token)
		{
			m_userTokenManager.RemoveUserToken(token);

			m_userTokenPool!.Push(token);
		}

		/// <summary>
		/// TCP 네트워크 서비스 종료 함수
		/// </summary>
		public void Close()
		{
			m_listener!.Close();

			m_userTokenManager.CloseAll();
		}
	}
}
