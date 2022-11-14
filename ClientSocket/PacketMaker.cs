using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSocket
{
	/// <summary>
	/// 패킷 생성을 처리
	/// </summary>
	public static class PacketMaker
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		private const int kBaseBufferSize = 1024;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static int s_nBufferSize = kBaseBufferSize;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		/// <summary>
		/// 초기화 함수
		/// </summary>
		/// <param name="nBufferSize">패킷의 버퍼 크기</param>
		public static void Initialize(int nBufferSize)
		{
			s_nBufferSize = nBufferSize;
		}

		/// <summary>
		/// 패킷객체 생성 함수
		/// </summary>
		/// <returns>설정된 버퍼의 크기를 가지는 Packet 객체 반환</returns>
		public static Packet CreatePacket()
		{
			return new Packet(s_nBufferSize);
		}
	}
}
