using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAPSocket
{
	/// <summary>
	/// 수신된 데이터를 분석하여 처리가능한 패킷으로 전달
	/// </summary>
	internal class MessageResolver
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 헤더에 등록된 메세지의 길이
		private int m_nMessageSize;
		// 메세지버퍼
		private byte[] m_messageBuffer;
		// 메세지 버퍼의 복사 시작 위치
		private int m_nCurrentPosition;
		// 복사 해야할 목표데이터 위치
		private int m_nPositionToRead;
		// 현재 남은 데이터 길이
		private int m_nRemainBytes;

		// 종료 신호
		private ManualResetEvent m_endSignal;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Delegate / Event

		// 패킷 완성시 호출될 대리자
		public delegate void CompletedMessageCallback(byte[] buffer);

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nBufferSize">내부 버퍼의 사이즈</param>
		public MessageResolver(int nBufferSize)
		{
			if (nBufferSize <= 0)
				throw new ArgumentOutOfRangeException("nBufferSize");

			m_nMessageSize = 0;
			m_messageBuffer = new byte[nBufferSize];
			m_nCurrentPosition = 0;
			m_nPositionToRead = 0;
			m_nRemainBytes = 0;

			m_endSignal = new ManualResetEvent(true);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 목표위치만큼 메세지버퍼에 복사하는 함수
		/// </summary>
		/// <param name="buffer">복사할 버퍼</param>
		/// <param name="nOffset">시작 위치</param>
		/// <returns>패킷 완성시 true, 패킷 미완성시 false 반환</returns>
		public bool ReadUtil(byte[] buffer, ref int nOffset)
		{
			// 복사할 데이터 길이 계산
			int nCopySize = m_nPositionToRead - m_nCurrentPosition;

			// 남은 데이터가 복사할 데이터 길이보다 적을 경우 가능한 만큼만 복사
			if (m_nRemainBytes < nCopySize)
				nCopySize = m_nRemainBytes;

			// 복사 처리
			Array.Copy(buffer, nOffset, m_messageBuffer, m_nCurrentPosition, nCopySize);
			// 오프셋 위치 변경
			nOffset += nCopySize;

			// 현재 위치 변경
			m_nCurrentPosition += nCopySize;
			// 남은 데이터 복사한 길이 만큼 차감
			m_nRemainBytes -= nCopySize;

			// 목표 위치까지 데이터를 읽지 못했다면 다음 수신 데이터까지 처리 필요
			if (m_nCurrentPosition < m_nPositionToRead)
				return false;

			return true;
		}

		/// <summary>
		/// 수신된 데이터를 완성된 패킷으로 처리하는 함수
		/// 패킷이 완성되었을 경우 콜백함수를 통해 이후 처리 및 사용된 데이터를 메세지 버퍼에서 삭제
		/// 미완성일 경우 현재 저장된 데이터와 다음 수신된 데이터로 패킷 처리
		/// </summary>
		/// <param name="buffer">수신된 데이터 버퍼</param>
		/// <param name="nOffset">버퍼의 시작 위치</param>
		/// <param name="nCount">버퍼의 유효 길이</param>
		/// <param name="callback">패킷 완성후 호출될 콜백함수</param>
		public void OnReceive(byte[] buffer, int nOffset, int nCount, CompletedMessageCallback callback)
		{
			// 수신 데이터가 들어올 경우 종료 신호를 차단 설정
			m_endSignal.Reset();

			// 남은 데이터 길이 저장
			m_nRemainBytes = nCount;

			// 패킷이 분리되어 있을 경우 사용될 시작 위치 등록
			int nSrcPosition = nOffset;

			// 여러 패킷이 수신 되었을 경우 완성된 패킷데이터를 제외한 데이터로 반복 처리
			while (m_nRemainBytes > 0)
			{
				bool bCompleted = false;

				// 현재 위치가 헤더사이즈의 크기보다 작을 경우
				if (m_nCurrentPosition < Defines.HEADSIZE)
				{
					// 헤더 사이즈를 목표로 설정
					m_nPositionToRead = Defines.HEADSIZE;

					// 헤더사이즈 크기만큼 메세지 버퍼로 복사
					bCompleted = ReadUtil(buffer, ref nSrcPosition);

					if (!bCompleted)
						break;

					// 복사할 바디 크기 계산
					m_nMessageSize = GetBodySize();
					// 목표위치 설정
					m_nPositionToRead = m_nMessageSize;
				}

				// 목표로 설정된 데이터 크기만큼 메세지 버퍼로 복사
				bCompleted = ReadUtil(buffer, ref nSrcPosition);

				// 패킷이 완성되었을 경우 콜백함수 호출 및 사용된 메세지 버퍼의 데이터 초기화
				// 완성 되지 않았을 경우 현재 데이터 + 다음 수신된 데이터로 패킷 처리
				if (bCompleted)
				{
					callback(m_messageBuffer);

					ClearBuffer();
				}
			}

			// 수신 데이터 처리가 끝났을 경우 종료 신호 설정
			m_endSignal.Set();
		}

		/// <summary>
		/// 바디(실제 사용될 데이터)의 크기 계산 함수
		/// </summary>
		/// <returns>헤더크기 반환</returns>
		private int GetBodySize()
		{
			return m_messageBuffer[0] | (m_messageBuffer[1] << 8) | (m_messageBuffer[2] << 16) | (m_messageBuffer[3] << 24);
		}

		/// <summary>
		/// 메세지 버퍼 초기화 함수
		/// </summary>
		private void ClearBuffer()
		{
			Array.Clear(m_messageBuffer, 0, m_messageBuffer.Length);

			m_nCurrentPosition = 0;
			m_nMessageSize = 0;
		}

		/// <summary>
		/// 종료 함수
		/// </summary>
		public void Stop()
		{
			// 수신 데이터 처리가 끝날때까지 대기
			m_endSignal.WaitOne();

			// 버퍼 초기화
			ClearBuffer();
		}
	}
}
