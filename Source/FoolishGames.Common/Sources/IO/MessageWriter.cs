using FoolishGames.Common;
using FoolishGames.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 通讯数据构建
    /// </summary>
    public class MessageWriter : MessageInfo, IMessageHeader, IMessageWriter
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public long MsgId { get; set; }

        /// <summary>
        /// 操作码
        /// </summary>
        public sbyte OpCode { get; set; } = (sbyte)EOpCode.Binary;

        /// <summary>
        /// 通讯协议Id
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// 是否数据压缩
        /// </summary>
        public bool Compress { get; set; } = false;

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool Secret { get; set; } = false;

        /// <summary>
        /// 是否有报错
        /// </summary>
        public bool IsError { get { return !string.IsNullOrEmpty(Error); } }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; } = null;

        /// <summary>
        /// 内容长度
        /// </summary>
        public int ContextLength { get; private set; } = 0;

        /// <summary>
        /// 包体长度
        /// </summary>
        public int GetPacketLength()
        {
            return HeaderLength + ContextLength;
        }

        /// <summary>
        /// 内容信息
        /// </summary>
        public byte[] GetContext()
        {
            return stream.GetBuffer();
        }

        /// <summary>
        /// 缓存池
        /// </summary>
        private MemoryStream stream = new MemoryStream();

        /// <summary>
        /// 析构函数
        /// </summary>
        ~MessageWriter()
        {
            stream.Dispose();
        }

        /// <summary>
        /// 写入消息头数据
        /// </summary>
        public void WriteHeader(byte[] buffer, int offset)
        {
            int index = offset;
            byte[] data = BitConverter.GetBytes(MsgId);
            for (int i = 0; i < data.Length; i++)
            {
                buffer[index + i] = data[i];
            }
            index += data.Length;
            buffer[index] = (byte)OpCode;
            index += 1;
            data = BitConverter.GetBytes(ActionId);
            for (int i = 0; i < data.Length; i++)
            {
                buffer[index + i] = data[i];
            }
        }

        /// <summary>
        /// 写入Boolean
        /// </summary>
        public void WriteBool(bool value)
        {
            WriteByte(value ? ByteUtil.ONE : ByteUtil.ZERO);
        }

        /// <summary>
        /// 写入Byte
        /// </summary>
        public void WriteByte(byte value)
        {
            stream.WriteByte(value);
            ContextLength += 1;
        }

        /// <summary>
        /// 写入Char
        /// </summary>
        public void WriteChar(char value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入时间
        /// </summary>
        public void WriteDateTime(DateTime value)
        {
            WriteBytes(BitConverter.GetBytes(value.Ticks));
        }

        /// <summary>
        /// 写入Decimal
        /// </summary>
        public void WriteDecimal(decimal value)
        {
            int[] bits = Decimal.GetBits(value);
            foreach (int bit in bits)
            {
                WriteInt(bit);
            }
        }

        /// <summary>
        /// 写入Double
        /// </summary>
        public void WriteDouble(double value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入Float
        /// </summary>
        public void WriteFloat(float value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入Int
        /// </summary>
        public void WriteInt(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入Long
        /// </summary>
        public void WriteLong(long value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入SByte
        /// </summary>
        public void WriteSByte(sbyte value)
        {
            WriteByte((byte)value);
        }

        /// <summary>
        /// 写入Short
        /// </summary>
        public void WriteShort(short value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        public void WriteString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(bytes.Length);
            WriteBytes(bytes);
        }

        /// <summary>
        /// 写入UInt
        /// </summary>
        public void WriteUInt(uint value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入ULong
        /// </summary>
        public void WriteULong(ulong value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 写入UShort
        /// </summary>
        public void WriteUShort(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        /// <summary>
        /// 写入字节流
        /// </summary>
        /// <param name="bytes"></param>
        private void WriteBytes(byte[] bytes)
        {
            ContextLength += bytes.Length;
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
