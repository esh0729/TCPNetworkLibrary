using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAPSocket
{
	/// <summary>
	/// 클라이언트와 연결된 사용자토큰을 관리
	/// </summary>
	internal class UserTokenManager
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_syncObject;
		private List<UserToken> m_tokens;

		private Timer? m_heartBeatTimer;
		private long m_lnHeartBeatLifetimeTicks;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public UserTokenManager()
		{
			m_syncObject = new object();
			m_tokens = new List<UserToken>();
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//
		// UserToken
		//

		/// <summary>
		/// 관리대상 사용자토큰 저장 함수
		/// </summary>
		/// <param name="token">관리할 사용자토큰</param>
		public void AddUserToken(UserToken token)
		{
			lock (m_syncObject)
			{
				m_tokens.Add(token);
			}
		}

		/// <summary>
		/// 관리대상 사용자토큰 삭제 함수
		/// </summary>
		/// <param name="token">삭제할 사용자토큰</param>
		public void RemoveUserToken(UserToken token)
		{
			lock (m_syncObject)
			{
				m_tokens.Remove(token);
			}
		}

		/// <summary>
		/// 연결된 모든 사용자토큰을 종료시키는 함수
		/// </summary>
		public void CloseAll()
		{
			lock (m_syncObject)
			{
				while (m_tokens.Count > 0)
				{
					UserToken token = m_tokens[0];

					token.Close();
				}
			}
		}

		//
		// HeartBeat
		//

		/// <summary>
		/// 연결상태 체크 시작 함수
		/// </summary>
		/// <param name="nInterval">체크간격(millisecond)</param>
		/// <param name="nLifetime">생명주기(millisecond)</param>
		public void StartHeartBeat(int nInterval, int nLifetime)
		{
			if (m_heartBeatTimer != null)
				return;

			// Tick 체크 시간으로 변환
			m_lnHeartBeatLifetimeTicks = nLifetime * TimeSpan.TicksPerMillisecond;

			// 일정간격으로 연결상태를 체크하는 타이머 생성
			m_heartBeatTimer = new Timer(OnHeartBeatTick);
			m_heartBeatTimer.Change(nInterval, nInterval);
		}

		/// <summary>
		/// 연결상태 체크 종료 함수
		/// </summary>
		public void StopHeartBeat()
		{
			if (m_heartBeatTimer == null)
				return;

			m_heartBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
			m_heartBeatTimer.Dispose();
			m_heartBeatTimer = null;
		}

		/// <summary>
		/// 연결상태 체크 함수
		/// </summary>
		/// <param name="state">타이머 설정시 전달한 매개변수(사용X)</param>
		private void OnHeartBeatTick(object? state)
		{
			// 현재시각을 기준으로 유효한 타임틱을 계산
			long lnValidTimeTicks = DateTime.Now.Ticks - m_lnHeartBeatLifetimeTicks;
			int nIndex = 0;

			lock (m_syncObject)
			{
				while (nIndex < m_tokens.Count)
				{
					UserToken token = m_tokens[nIndex];

					if (token.CheckHeartBeat(lnValidTimeTicks))
					{
						nIndex++;
						continue;
					}

					token.Close();
				}
			}
		}
	}
}
