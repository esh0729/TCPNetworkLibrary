using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace EAPSocket
{
	/// <summary>
	/// 송수신에 사용되는 모든버퍼를 관리
	/// </summary>
	public class BufferManager
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
		/// <param name="nTotalBytes">총 버퍼 크기</param>
		/// <param name="nBufferSize">버퍼 하나의 크기</param>
		public BufferManager(int nTotalBytes, int nBufferSize)
		{
			m_nNumBytes = nTotalBytes;
			m_nCurrentIndex = 0;
			m_nBufferSize = nBufferSize;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void InitBuffer()
		{
			if (m_nNumBytes <= 0)
				return;

			m_buffer = new byte[m_nNumBytes];
		}

		public bool SetBuffer(SocketAsyncEventArgs args)
		{
			if (m_nNumBytes <= 0)
				return false;

			if ((m_nCurrentIndex + m_nBufferSize) > m_nNumBytes)
				return false;

			args.SetBuffer(m_buffer, m_nCurrentIndex, m_nBufferSize);
			m_nCurrentIndex += m_nBufferSize;

			return true;
		}
	}
}
