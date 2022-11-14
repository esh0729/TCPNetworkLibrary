using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ClientSocket;

namespace TestClient
{
	/// <summary>
	/// 클라이언트 메인 클래스
	/// </summary>
	public class ClientMain
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static NetworkService m_service;
		private static ServerPeer s_serverPeer;
		private static object s_lock;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		/// <summary>
		/// 메인 함수
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			s_lock = new object();

			// 네트워크 서비스 객체 생성 및 설정 이후 서버 접속 시작
			m_service = new NetworkService(SystemConfig.bufferSize);
			m_service.onConnected += OnConnected;
			m_service.Initialize();
			m_service.Connect(SystemConfig.host, SystemConfig.port);

			// 별도의 네트워브 서비스 객체의 갱신을 처리할 스레드 생성
			Thread serviceThread = new Thread(Service);
			serviceThread.Start();

			// 메세지 입력 후 입력 받은 메세지를 서버에 전달
			while (true)
			{
				string sLine = Console.ReadLine();
				// "q" 메세지를 입력 받았을 경우 반복문을 빠져 나옴
				if (sLine == "q")
					break;

				Packet packet = Packet.Create();
				packet.Push((short)MessageType.MSG_REQ);
				packet.Push(sLine);

				lock (s_lock)
				{
					s_serverPeer.Send(packet);
				}
			}

			// 반복문 종료후 서버 종료 처리
			lock (s_lock)
			{
				s_serverPeer.Disconnect();
				s_serverPeer = null;
			}

			// 서비스 스레드가 종료 될때까지 대기
			serviceThread.Join();

			Console.WriteLine("Exit...");
			Console.ReadKey();
		}

		/// <summary>
		/// 서버 접속이 완료 됬을 경우 호출되는 함수
		/// </summary>
		/// <param name="token">사용자 토큰</param>
		private static void OnConnected(UserToken token)
		{
			s_serverPeer = new ServerPeer(token);
			Console.WriteLine("Connected!");
		}

		/// <summary>
		/// 일정시간 마다 서비스 객체의 갱신을 처리하는 함수
		/// 별도의 스레드에서 동작
		/// </summary>
		private static void Service()
		{
			while (true)
			{
				Thread.Sleep(1000);

				lock (s_lock)
				{
					if (s_serverPeer == null)
						break;

					m_service.Service();
				}
			}
		}
	}
}
