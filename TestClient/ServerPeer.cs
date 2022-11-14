using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientSocket;

namespace TestClient
{
	/// <summary>
	/// 소켓 통신을 처리한느 사용자 객체
	/// </summary>
	public class ServerPeer : IPeer
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private UserToken m_token;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token">사용자 토큰</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ServerPeer(UserToken token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			m_token = token;
			m_token.SetPeer(this);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 데이터 수신시 호출되는 함수
		/// </summary>
		/// <param name="packet"></param>
		public void OnReceive(Packet packet)
		{
			MessageType type = (MessageType)packet.PopInt16();
			switch (type)
			{
				case MessageType.MSG_ACK:
					{
						string msg = packet.PopString();
						Console.WriteLine("Response - {0}", msg);
					}
					break;
			}
		}

		/// <summary>
		/// 데이터 송신 함수
		/// </summary>
		/// <param name="packet"></param>
		public void Send(Packet packet)
		{
			m_token.Send(packet);
		}

		/// <summary>
		/// 연결 해제 함수
		/// </summary>
		public void Disconnect()
		{
			m_token.Disconnect();
		}

		/// <summary>
		/// 연결 종료시 호출되는 ㅎ마수
		/// </summary>
		public void OnRemoved()
		{
			Console.WriteLine("OnRemoved");
		}
	}
}
