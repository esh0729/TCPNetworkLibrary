using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSocket
{
	/// <summary>
	/// 소켓 통신을 처리하는 사용자 객체의 인터페이스
	/// </summary>
	public interface IPeer
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 데이터 송신 함수
		/// </summary>
		/// <param name="packet">송신할 패킷 데이터</param>
		void Send(Packet packet);
		/// <summary>
		/// 데이터 수신시 호출되는 함수
		/// </summary>
		/// <param name="buffer">수신된 데이터</param>
		void OnReceive(Packet packet);
		/// <summary>
		/// 연결 해제 함수
		/// </summary>
		void Disconnect();
		/// <summary>
		/// 연결 해제 시 호출되는 함수
		/// </summary>
		void OnDisconnect();
	}
}
