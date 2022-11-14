using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
	/// <summary>
	/// 서버 메인 클래스
	/// </summary>
	public static class ServerMain
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		// 메인 시작 함수
		public static void Start()
		{
			Console.WriteLine("1. APMSocket Server");
			Console.WriteLine("2. EAPSocket Server");
			Console.WriteLine("3. TAPSocket Server");
			Console.Write("Server Select - ");

			string? str = Console.ReadLine();
			int n = Convert.ToInt32(str);

			switch (n)
			{
				case 1: APMSocketNetwork.Start(); break;
				case 2: EAPSocketNetwork.Start(); break;
				case 3: TAPSocketNetwork.Start(); break;

				default: Console.WriteLine("Not Exist Server {0}", n); break;
			}
		}
	}

	//
	//
	//

	/// <summary>
	/// APMSocket 서버
	/// </summary>
	public static class APMSocketNetwork
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static object s_syncObject = new object();
		private static Dictionary<Guid, APMClientPeer> m_peers = new Dictionary<Guid, APMClientPeer>();

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		/// <summary>
		/// 서버 시작 함수
		/// </summary>
		public static void Start()
		{
			APMSocket.NetworkService service = new APMSocket.NetworkService(SystemConfig.maxConnections, SystemConfig.bufferSize, 1);
			service.onCreatedSession += OnCreatedSession;
			service.Initialize();
			service.Listen(SystemConfig.host, SystemConfig.port, SystemConfig.backlog);

			Console.WriteLine("APMSocketNetwork Server Started!");

			string? str;

			while (true)
			{
				str = Console.ReadLine();

				if (str == "exit")
					break;
			}

			service.Close();

			Console.WriteLine("APMSocketNetwork Server Exited");
		}

		/// <summary>
		/// 클라이언트 연결 완료시 호출되는 함수
		/// </summary>
		/// <param name="token">연결된 클라이언트 사용자 토큰</param>
		private static void OnCreatedSession(APMSocket.UserToken token)
		{
			APMClientPeer peer = new APMClientPeer(token);

			AddClientPeer(peer);
		}

		/// <summary>
		/// 피어 컬렉션 추가 함수
		/// </summary>
		/// <param name="peer">사용자 피어</param>
		public static void AddClientPeer(APMClientPeer peer)
		{
			lock (s_syncObject)
			{
				m_peers.Add(peer.id, peer);
			}
		}

		/// <summary>
		/// 피어 컬렉션 제거 함수
		/// </summary>
		/// <param name="id">사용자 피어 ID</param>
		public static void RemoveClientPeer(Guid id)
		{
			lock (s_syncObject)
			{
				m_peers.Remove(id);
			}
		}
	}

	//
	//
	//

	/// <summary>
	/// EAPSocket 서버
	/// </summary>
	public static class EAPSocketNetwork
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static object s_syncObject = new object();
		private static Dictionary<Guid, EAPClientPeer> m_peers = new Dictionary<Guid, EAPClientPeer>();

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		/// <summary>
		/// 서버 시작 함수
		/// </summary>
		public static void Start()
		{
			EAPSocket.NetworkService service = new EAPSocket.NetworkService(SystemConfig.maxConnections, SystemConfig.bufferSize, 1);
			service.onCreatedSession += OnCreatedSession;
			service.Initialize();
			service.Listen(SystemConfig.host, SystemConfig.port, SystemConfig.backlog);

			Console.WriteLine("EAPSocketNetwork Server Started!");

			string? str;

			while (true)
			{
				str = Console.ReadLine();

				if (str == "exit")
					break;
			}

			service.Close();

			Console.WriteLine("APMSocketNetwork Server Exited");
		}

		/// <summary>
		/// 클라이언트 연결 완료시 호출되는 함수
		/// </summary>
		/// <param name="token">연결된 클라이언트 사용자 토큰</param>
		private static void OnCreatedSession(EAPSocket.UserToken token)
		{
			EAPClientPeer peer = new EAPClientPeer(token);

			AddClientPeer(peer);
		}

		/// <summary>
		/// 피어 컬렉션 추가 함수
		/// </summary>
		/// <param name="peer">사용자 피어</param>
		public static void AddClientPeer(EAPClientPeer peer)
		{
			lock (s_syncObject)
			{
				m_peers.Add(peer.id, peer);
			}
		}

		/// <summary>
		/// 피어 컬렉션 제거 함수
		/// </summary>
		/// <param name="id">사용자 피어 ID</param>
		public static void RemoveClientPeer(Guid id)
		{
			lock (s_syncObject)
			{
				m_peers.Remove(id);
			}
		}
	}

	//
	//
	//

	/// <summary>
	/// TAPSocket 서버
	/// </summary>
	public static class TAPSocketNetwork
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static object s_syncObject = new object();
		private static Dictionary<Guid, TAPClientPeer> m_peers = new Dictionary<Guid, TAPClientPeer>();

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		/// <summary>
		/// 서버 시작 함수
		/// </summary>
		public static void Start()
		{
			TAPSocket.NetworkService service = new TAPSocket.NetworkService(SystemConfig.maxConnections, SystemConfig.bufferSize, 1);
			service.onCreatedSession += OnCreatedSession;
			service.Initialize();
			service.Listen(SystemConfig.host, SystemConfig.port, SystemConfig.backlog);

			Console.WriteLine("TAPSocketNetwork Server Started!");

			string? str;

			while (true)
			{
				str = Console.ReadLine();

				if (str == "exit")
					break;
			}

			service.Close();

			Console.WriteLine("APMSocketNetwork Server Exited");
		}

		/// <summary>
		/// 클라이언트 연결 완료시 호출되는 함수
		/// </summary>
		/// <param name="token">연결된 클라이언트 사용자 토큰</param>
		private static void OnCreatedSession(TAPSocket.UserToken token)
		{
			TAPClientPeer peer = new TAPClientPeer(token);

			AddClientPeer(peer);
		}

		/// <summary>
		/// 피어 컬렉션 추가 함수
		/// </summary>
		/// <param name="peer">사용자 피어</param>
		public static void AddClientPeer(TAPClientPeer peer)
		{
			lock (s_syncObject)
			{
				m_peers.Add(peer.id, peer);
			}
		}

		/// <summary>
		/// 피어 컬렉션 제거 함수
		/// </summary>
		/// <param name="id">사용자 피어 ID</param>
		public static void RemoveClientPeer(Guid id)
		{
			lock (s_syncObject)
			{
				m_peers.Remove(id);
			}
		}
	}
}
