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
	/// 소켓 통신에서 클라이언트와의 연결을 처리
	/// </summary>
	public class Listener
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Socket? m_listener;
		private SocketAsyncEventArgs? m_acceptArgs;
		private AutoResetEvent? m_listenControlEvent;

		private object m_syncObject;

		private bool m_bClosed;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Delegate / Event

		// 클라이언트 연결 완료시 호출될 대리자
		public delegate void ConnectionClientHandler(Socket socket);

		private event ConnectionClientHandler? m_onConnectClient;
		public event ConnectionClientHandler onConnectClient
		{
			add
			{
				lock (m_syncObject)
				{
					m_onConnectClient += value;
				}
			}
			remove
			{
				lock (m_syncObject)
				{
					m_onConnectClient -= value;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		public Listener()
		{
			m_listener = null;
			m_listenControlEvent = null;

			m_syncObject = new object();

			m_bClosed = false;

			m_onConnectClient = null;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 연결요청대기 시작 함수
		/// </summary>
		/// <param name="sHost">서버 주소</param>
		/// <param name="nPort">서버 포트</param>
		/// <param name="nBacklog">연결요청대기큐 크기</param>
		public void Start(string sHost, int nPort, int nBacklog)
		{
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			IPAddress? address = null;
			if (sHost == "0.0.0.0")
				address = IPAddress.Any;
			else
				address = IPAddress.Parse(sHost);

			IPEndPoint endPoint = new IPEndPoint(address, nPort);

			m_listener.Bind(endPoint);
			m_listener.Listen(nBacklog);

			// 비동기로 연결처리하여 연결 완료시 등록된 이벤트 핸들러 호출되도록 설정
			m_acceptArgs = new SocketAsyncEventArgs();
			m_acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

			// 별도의 스레드에서 연결요청 처리
			Thread listenThread = new Thread(new ThreadStart(Listening));
			listenThread.Start();
		}

		/// <summary>
		/// 연결요청 처리 함수
		/// 별도 스레드에서 동작
		/// </summary>
		private void Listening()
		{
			// 연결요철을 한번에 한건씩 처리하기 위한 이벤트 객체
			m_listenControlEvent = new AutoResetEvent(false);

			while (true)
			{
				m_acceptArgs!.AcceptSocket = null;

				bool bPending = true;

				try
				{
					bPending = m_listener!.AcceptAsync(m_acceptArgs);
				}
				catch (Exception ex)
				{
					lock (m_syncObject)
					{
						if (m_bClosed)
							break;
					}

					Console.WriteLine("Listening() error - {0}", ex.Message);
					continue;
				}

				// 연결처리가 동기적으로 완료 되었을 경우 연결 이후 작업 함수 직접 호출
				if (!bPending)
					OnAcceptCompleted(null, m_acceptArgs);

				// 연결 이후 작업이 완료 될때까지 대기
				m_listenControlEvent.WaitOne();

				lock (m_syncObject)
				{
					if (m_bClosed)
						break;
				}
			}
		}
		
		/// <summary>
		/// 연결완료시 호출되는 함수
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
		{
			if (args.SocketError == SocketError.Success)
			{
				Socket clientSocket = args.AcceptSocket!;
				// 비동기 소켓 작업에서 연결된 소켓을 가져온 이후 다시 연결요청대기를 할 수 있도록 이벤트 신호 발생
				m_listenControlEvent!.Set();

				if (m_onConnectClient != null)
					m_onConnectClient(clientSocket);
			}
			else
			{
				Console.WriteLine("OnAcceptCompleted() error - {0}", args.SocketError);
				// 비동기 소켓 작업이 실패 했을 경우 다시 연결요청대기를 할 수 있도록 이벤트 신호 발생
				m_listenControlEvent!.Set();
			}
		}

		/// <summary>
		/// 리스너 종료 함수
		/// </summary>
		public void Close()
		{
			lock (m_syncObject)
			{
				if (m_bClosed)
					return;

				m_bClosed = true;

				m_listener!.Close();
				m_listener = null;

				m_onConnectClient = null;
			}
		}
	}
}
