using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSocket
{
	/// <summary>
	/// 사용자 송수신에 사용되는 버퍼를 관리
	/// </summary>
	public class UserBuffer
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte[] m_buffer;
		private int m_nOffset;
		private int m_nCount;

		private int m_nPosition;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		public UserBuffer()
		{
			m_nPosition = 0;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public byte[] buffer
		{
			get { return m_buffer; }
		}

		public int offset
		{
			get { return m_nOffset; }
		}

		public int count
		{
			get { return m_nCount; }
		}

		public int position
		{
			get { return m_nPosition; }
		}

		public bool isSending
		{
			get { return m_nPosition > 0; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 버퍼 설정 함수
		/// </summary>
		/// <param name="buffer">버퍼</param>
		/// <param name="nOffset">버퍼 시작 위치</param>
		/// <param name="nCount">버퍼 크기</param>
		public void SetBuffer(byte[] buffer, int nOffset, int nCount)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			m_buffer = buffer;
			m_nOffset = nOffset;
			m_nCount = nCount;
		}

		/// <summary>
		/// 패킷데이터를 버퍼의 현재 위치에 저장하는 함수
		/// </summary>
		/// <param name="packet">저장 할 패킷</param>
		/// <returns>저장 가능할 경우 true, 불가능할 경우 false 반환</returns>
		public bool Push(Packet packet)
		{
			if (packet == null)
				throw new ArgumentNullException("packet");

			int nPacketPosition = packet.position;

			if (m_nCount - m_nPosition < nPacketPosition)
				return false;

			Array.Copy(packet.buffer, 0, m_buffer, m_nOffset + m_nPosition, nPacketPosition);
			m_nPosition += nPacketPosition;

			return true;
		}

		/// <summary>
		/// 버퍼의 특정 위치를 0번째 인덱스로 이동하는 함수
		/// </summary>
		/// <param name="nCount">이동할 시작 데이터 위치</param>
		public void Shift(int nCount)
		{
			Array.Copy(m_buffer, m_nOffset + nCount, m_buffer, m_nOffset, m_nPosition - nCount);
		}

		/// <summary>
		/// 버퍼 데이터 삭제 함수
		/// 실제 데이터를 삭제하는것이 아닌 위치를 초기 위치로 변경하여 첫 인덱스에 덮어 씌워지도록 처리
		/// </summary>
		public void Clear()
		{
			m_nPosition = 0;
		}
	}
}
