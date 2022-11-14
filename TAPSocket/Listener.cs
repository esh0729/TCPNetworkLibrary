using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TAPSocket
{
	/// <summary>
	/// 소켓 통신에서 클라이언트와의 연결을 처리
	/// </summary>
	public class Listener
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Socket? m_listener;

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
			Task.Run(new Func<Task>(Listening));
		}

		/// <summary>
		/// 연결요청 처리 함수
		/// 별도 스레드에서 동작
		/// </summary>
		/// <returns></returns>
		private async Task Listening()
		{
			while (true)
			{
				Socket socket;

				try
				{
					// await를 이용하여 클라이언트 연결 비동기 대기
					socket = await m_listener!.AcceptAsync();

					// 접속 이후 처리는 별도의 스레드에서 진행
					ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessAccept), socket);
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
			}
		}

		/// <summary>
		/// 클라이언트 접속 이후 처리할 작업을 진행하는 함수
		/// </summary>
		/// <param name="state">함수 호출부에서 전달하는 매개변수(Socket)</param>
		private void ProcessAccept(object? state)
		{
			Socket socket = (Socket)state!;
			Console.WriteLine("Connected {0}", socket.RemoteEndPoint);

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
