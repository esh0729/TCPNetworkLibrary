using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPSocket
{
	/// <summary>
	/// 대기중인 사용자토큰을 관리
	/// </summary>
	internal class UserTokenPool
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Stack<UserToken> m_pool;
		private object m_syncObject;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nCapacity">사용자토큰 풀의 크기</param>
		public UserTokenPool(int nCapacity)
		{
			m_pool = new Stack<UserToken>(nCapacity);
			m_syncObject = new object();
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public int count
		{
			get
			{
				lock (m_syncObject)
				{
					return m_pool.Count;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		/// <summary>
		/// 사용자토큰 저장 함수
		/// </summary>
		/// <param name="token">사용자 토큰</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void Push(UserToken token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			lock (m_syncObject)
			{
				m_pool.Push(token);
			}
		}

		/// <summary>
		/// 사용자토큰 호출 함수
		/// </summary>
		/// <returns>사용대기중인 사용자토큰 반환</returns>
		public UserToken Pop()
		{
			lock (m_syncObject)
			{
				return m_pool.Pop();
			}
		}
	}
}
