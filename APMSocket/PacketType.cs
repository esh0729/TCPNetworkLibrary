using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMSocket
{
	/// <summary>
	/// 패킷의 타입
	/// </summary>
	internal enum PacketType : byte
	{
		// 소켓 연결상태 확인 요청
		SYS_HeartBeatREQ = 1,
		// 소켓 연결상태 확인 응답
		SYS_HeartBeatACK = 2,

		// 연결 종료 요청
		SYS_DisconnectSignal = 3,

		// 사용자 메세지
		USER_Message = 4
	}
}
