using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace APMSocket
{
	/// <summary>
	/// 소켓 통신에서 클라이언트와의 연결을 처리
	/// </summary>
	internal class Listener
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Socket? m_listener;
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
				lock (m_syncObject!)
				{
					m_onConnectClient += value;
				}
			}
			remove
			{
				lock (m_syncObject!)
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

			// 별도 스레드에서 연결요청 처리
			Thread listenThread = new Thread(new ThreadStart(Listening));
			listenThread.Start();
		}

		/// <summary>
		/// 연결요청 처리 함수
		/// 별도 스레드에서 동작
		/// </summary>
		private void Listening()
		{
			// 연결요청을 한번에 한건씩 처리하기 위한 이벤트 객체
			m_listenControlEvent = new AutoResetEvent(false);

			while (true)
			{
				try
				{
					m_listener!.BeginAccept(new AsyncCallback(AcceptCallback), null);
				}
				catch (Exception ex)
				{
					lock (m_syncObject!)
					{
						if (m_bClosed)
							break;
					}

					Console.WriteLine("Listening() error - {0}", ex.Message);
					continue;
				}

				// 콜백함수의 연결수락이 끝날때 까지 대기
				m_listenControlEvent.WaitOne();

				lock (m_syncObject!)
				{
					if (m_bClosed)
						break;
				}
			}
		}

		/// <summary>
		/// 비동기 연결 콜백함수
		/// </summary>
		/// <param name="result">비동기작업 결과</param>
		private void AcceptCallback(IAsyncResult result)
		{
			Socket? socket = null;

			try
			{
				socket = m_listener!.EndAccept(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				// 클라이언트 연결작업이 끝나거나 에러 출력 이후 연결요청을 대기할수 있게 이벤트 신호 발생
				m_listenControlEvent!.Set();
			}

			// 에러가 발생하여 소켓이 생성되지 않았을 경우 이후 작업을 처리하지 않음
			if (socket == null)
				return;

			lock (m_syncObject!)
			{
				if (m_onConnectClient != null)
					m_onConnectClient(socket);
			}
		}

		/// <summary>
		/// 리스너 종료 함수
		/// </summary>
		public void Close()
		{
			lock (m_syncObject!)
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
