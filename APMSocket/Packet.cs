using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMSocket
{
	/// <summary>
	/// 소켓 통신에서 데이터의 송신 및 수신에 사용될 데이터 저장
	/// </summary>
	public class Packet
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte[] m_buffer;
		private int m_nPosition;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructros

		/// <summary>
		/// 데이터 수신시 사용하는 생성자
		/// </summary>
		/// <param name="buffer">수신된 데이터가 저장된 버퍼</param>
		internal Packet(byte[] buffer)
		{
			m_buffer = buffer;
			m_nPosition = Defines.HEADSIZE;
		}

		/// <summary>
		/// 데이터 송신시 사용하는 생성자
		/// </summary>
		/// <param name="nBufferSize">내부에서 사용할 버퍼의 크기</param>
		internal Packet(int nBufferSize)
		{
			m_buffer = new byte[nBufferSize];
			m_nPosition = Defines.HEADSIZE;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public byte[] buffer
		{
			get { return m_buffer; }
		}

		public int position
		{
			get { return m_nPosition; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//
		// Receive
		//

		/// <summary>
		/// 버퍼에서 패킷의 타입을 꺼내오는 함수
		/// </summary>
		/// <returns>패킷 타입 반환</returns>
		internal PacketType PopPacketType()
		{
			// 헤더사이즈가 등록된 다음의 데이터에 1바이트 형식으로 패킷타입이 송신됨
			return (PacketType)PopByte();
		}

		/// <summary>
		/// 버퍼에서 byte 형태의 데이터를 꺼내오는 함수
		/// </summary>
		/// <returns>현재 위치의 byte 타입 데이터 반환</returns>
		public byte PopByte()
		{
			return m_buffer[m_nPosition++];
		}

		/// <summary>
		/// 버퍼에서 short 형태의 데이터를 꺼내오는 함수
		/// </summary>
		/// <returns>현재 위치의 short 타입 데이터 반환</returns>
		public short PopInt16()
		{
			return (short)(m_buffer[m_nPosition++] | (m_buffer[m_nPosition++] << 8));
		}

		/// <summary>
		/// 버퍼에서 int 형태의 데이터를 꺼내오는 함수
		/// </summary>
		/// <returns>현재 위치의 int 타입 데이터 반환</returns>
		public int PopInt32()
		{
			return (int)(m_buffer[m_nPosition++] | (m_buffer[m_nPosition++] << 8) | (m_buffer[m_nPosition++] << 16) | (m_buffer[m_nPosition++] << 24));
		}

		/// <summary>
		/// 버퍼에서 float 형태의 데이터를 꺼내오는 함수
		/// </summary>
		/// <returns>현재 위치의 float 타입 데이터 반환</returns>
		public float PopSingle()
		{
			float fValue = BitConverter.ToSingle(m_buffer, m_nPosition);
			m_nPosition += sizeof(float);

			return fValue;
		}

		/// <summary>
		/// 버퍼에서 특정 길이의 string 형태의 데이터를 꺼내오는 함수
		/// </summary>
		/// <returns>현재 위치의 string 타입 데이터 반환</returns>
		public string PopString()
		{
			int nLength = PopInt16();

			string sValue = Encoding.UTF8.GetString(m_buffer, m_nPosition, nLength);
			m_nPosition += nLength;

			return sValue;
		}

		//
		// Send
		//

		/// <summary>
		/// 패킷의 길이를 버퍼의 헤더사이즈 위치에 저장하는 함수
		/// </summary>
		public void RecordSize()
		{
			int nPosition = 0;

			m_buffer[nPosition++] = (byte)(0x000000ff & m_nPosition);
			m_buffer[nPosition++] = (byte)(0x000000ff & (m_nPosition >> 8));
			m_buffer[nPosition++] = (byte)(0x000000ff & (m_nPosition >> 16));
			m_buffer[nPosition] = (byte)(0x000000ff & (m_nPosition >> 24));
		}

		/// <summary>
		/// byte 형태의 데이터를 버퍼의 현재 위치에 저장하는 함수
		/// </summary>
		/// <param name="value">byte 데이터</param>
		public void Push(byte value)
		{
			m_buffer[m_nPosition++] = value;
		}

		/// <summary>
		/// short 형태의 데이터를 버퍼의 현재 위치에 저장하는 함수
		/// </summary>
		/// <param name="value">short 데이터</param>
		public void Push(short value)
		{
			m_buffer[m_nPosition++] = (byte)(0x00ff & value);
			m_buffer[m_nPosition++] = (byte)(0x00ff & (value >> 8));
		}

		/// <summary>
		/// int 형태의 데이터를 버퍼의 현재 위치에 저장하는 함수
		/// </summary>
		/// <param name="value">int 데이터</param>
		public void Push(int value)
		{
			m_buffer[m_nPosition++] = (byte)(0x000000ff & value);
			m_buffer[m_nPosition++] = (byte)(0x000000ff & (value >> 8));
			m_buffer[m_nPosition++] = (byte)(0x000000ff & (value >> 16));
			m_buffer[m_nPosition++] = (byte)(0x000000ff & (value >> 24));
		}

		/// <summary>
		/// float 형태의 데이터를 버퍼의 현재 위치에 저장하는 함수
		/// </summary>
		/// <param name="value">float 데이터</param>
		public void Push(float value)
		{
			// float 데이터 byte[]로 변환
			byte[] temp = BitConverter.GetBytes(value);
			// 변환한 byte[]을 버퍼의 현재 위치에서 부터 복사
			temp.CopyTo(m_buffer, m_nPosition);
			// 현재위치 변환한 byte[]의 길이만큼 증가
			m_nPosition += temp.Length;
		}

		/// <summary>
		/// string 형태의 데이터를 버퍼의 현재 위치에 저장하는 함수
		/// </summary>
		/// <param name="value">string 데이터</param>
		public void Push(string value)
		{
			// string 데이터를 UTF8 인코딩으로 byte[] 변환
			byte[] temp = Encoding.UTF8.GetBytes(value);
			// byte[] 의 길이 저장
			Push((short)temp.Length);

			// 변환한 byte[]을 버퍼의 현재 위치에서 부터 복사
			temp.CopyTo(m_buffer, m_nPosition);
			// 현재위치 변환한 byte[]의 길이만큼 증가
			m_nPosition += temp.Length;
		}

		//
		//
		//

		/// <summary>
		/// 패킷을 데이터 송신이 가능한 ArraySegment 형태로 변환하는 함수
		/// </summary>
		/// <returns>변환한 ArraySegment 객체</returns>
		public ArraySegment<byte> ToArraySegment()
		{
			return new ArraySegment<byte>(m_buffer, 0, m_nPosition);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		/// <summary>
		/// 패킷 생성 함수(수신 전용)
		/// </summary>
		/// <param name="buffer">수신된 데이터가 저장된 버퍼</param>
		/// <returns>생성된 Packet 객체 반환</returns>
		internal static Packet Create(byte[] buffer)
		{
			return new Packet(buffer);
		}

		/// <summary>
		/// 패킷 생성 함수(송신 전용)
		/// </summary>
		/// <returns>생성된 Packet 객체 반환</returns>
		public static Packet Create()
		{
			// 패킷 생성
			Packet packet = PacketMaker.CreatePacket();
			// 패킷의 타입을 사용자가 송신하는 메세지 타입으로 설정하여 버퍼에 저장
			packet.Push((byte)PacketType.USER_Message);

			return packet;
		}
	}
}
