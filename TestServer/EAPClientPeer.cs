using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EAPSocket;

namespace TestServer
{
	/// <summary>
	/// EAPSocket 전용 클라이언트 소켓
	/// </summary>
	public class EAPClientPeer : IPeer
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Guid m_id;
		private UserToken m_token;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token">사용자토큰</param>
		public EAPClientPeer(UserToken token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			m_id = Guid.NewGuid();
			m_token = token;
			m_token.SetPeer(this);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public Guid id
		{
			get { return m_id; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 데이터 송신 함수
		/// </summary>
		/// <param name="packet">송신 패킷 데이터</param>
		public void Send(Packet packet)
		{
			m_token.Send(packet);
		}

		/// <summary>
		/// 데이터 수신시 호출되는 함수
		/// </summary>
		/// <param name="packet">수신 패킷 데이터</param>
		public void OnReceive(Packet packet)
		{
			MessageType type = (MessageType)packet.PopInt16();
			switch (type)
			{
				case MessageType.MSG_REQ:
					{
						string text = packet.PopString();
						Console.WriteLine(text);

						Packet response = Packet.Create();
						response.Push((short)MessageType.MSG_ACK);
						response.Push(text);

						Send(response);
					}
					break;
			}
		}

		/// <summary>
		/// 연결 해제 함수
		/// </summary>
		public void Disconnect()
		{
			m_token.Disconnect();
		}

		/// <summary>
		/// 연결 해제 시 호출되는 함수
		/// </summary>
		public void OnDisconnect()
		{
			EAPSocketNetwork.RemoveClientPeer(m_id);
		}
	}
}
