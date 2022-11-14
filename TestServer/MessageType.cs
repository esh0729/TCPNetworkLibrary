using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
	/// <summary>
	/// 사용자 메세지의 타입
	/// </summary>
	public enum MessageType : short
	{
		MSG_REQ,
		MSG_ACK
	}
}
