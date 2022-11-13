using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 数据写入
    /// </summary>
    public interface IMessageWriter : IMessageHeader
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        long MsgId { set; }

        /// <summary>
        /// 操作码
        /// </summary>
        sbyte OpCode { set; }

        /// <summary>
        /// 通讯协议Id
        /// </summary>
        int ActionId { set; }

        /// <summary>
        /// 写入消息头数据
        /// </summary>
        void WriteHeader(byte[] buffer, int offset);

        /// <summary>
        /// 写入Boolean
        /// </summary>
        void WriteBool(bool value);

        /// <summary>
        /// 写入Char
        /// </summary>
        void WriteChar(char value);

        /// <summary>
        /// 写入Float
        /// </summary>
        void WriteFloat(float value);

        /// <summary>
        /// 写入Double
        /// </summary>
        void WriteDouble(double value);

        /// <summary>
        /// 写入Decimal
        /// </summary>
        void WriteDecimal(decimal value);

        /// <summary>
        /// 写入SByte
        /// </summary>
        void WriteSByte(sbyte value);

        /// <summary>
        /// 写入Short
        /// </summary>
        void WriteShort(short value);

        /// <summary>
        /// 写入Int
        /// </summary>
        void WriteInt(int value);

        /// <summary>
        /// 写入Long
        /// </summary>
        void WriteLong(long value);

        /// <summary>
        /// 写入Byte
        /// </summary>
        void WriteByte(byte value);

        /// <summary>
        /// 写入UShort
        /// </summary>
        void WriteUShort(ushort value);

        /// <summary>
        /// 写入UInt
        /// </summary>
        void WriteUInt(uint value);

        /// <summary>
        /// 写入ULong
        /// </summary>
        void WriteULong(ulong value);

        /// <summary>
        /// 写入时间
        /// </summary>
        void WriteDateTime(DateTime value);

        /// <summary>
        /// 写入字符串
        /// </summary>
        void WriteString(string value);
    }
}
