using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAPSocket
{
	/// <summary>
	/// 소켓 통신시 사용자의 구현이 필요한 인터페이스
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
		/// <param name="packet">수신된 패킷 데이터</param>
		void OnReceive(Packet packet);
		/// <summary>
		/// 연결 종료시 호출되는 함수
		/// </summary>
		void OnRemoved();
		/// <summary>
		/// 연결 해제 함수
		/// </summary>
		void Disconnect();
	}
}
