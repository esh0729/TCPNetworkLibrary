using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMSocket
{
	/// <summary>
	/// 송수신에 사용되는 모든버퍼를 관리
	/// </summary>
	internal class BufferManager
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nNumBytes;
		private byte[]? m_buffer;
		private int m_nCurrentIndex;
		private int m_nBufferSize;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		///
		/// </summary>
		/// <param name="nTotalBytes">총 버퍼의 크기</param>
		/// <param name="nBufferSize">버퍼 하나의 크기</param>
		public BufferManager(int nTotalBytes, int nBufferSize)
		{
			m_nNumBytes = nTotalBytes;
			m_nCurrentIndex = 0;
			m_nBufferSize = nBufferSize;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 초기화 함수
		/// </summary>
		public void Initialize()
		{
			if (m_nNumBytes <= 0)
				return;

			m_buffer = new byte[m_nNumBytes];
		}

		/// <summary>
		/// 사용자 버퍼 할당 함수
		/// </summary>
		/// <param name="buffer">사용자버퍼</param>
		/// <returns>할당에 성공 시 true 실패 시 false 반환</returns>
		public bool SetBuffer(UserBuffer buffer)
		{
			if (m_nNumBytes <= 0)
				return false;

			if ((m_nCurrentIndex + m_nBufferSize) > m_nNumBytes)
				return false;

			buffer.SetBuffer(m_buffer!, m_nCurrentIndex, m_nBufferSize);
			m_nCurrentIndex += m_nBufferSize;

			return true;
		}
	}
}
