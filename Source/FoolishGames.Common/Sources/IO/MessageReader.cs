using FoolishGames.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 通讯包解析
    /// </summary>
    public class MessageReader : MessageInfo, IMessageHeader, IMessageReader
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public long MsgId { get; private set; }

        /// <summary>
        /// 操作码
        /// </summary>
        public sbyte OpCode { get; private set; }

        /// <summary>
        /// 通讯协议Id
        /// </summary>
        public int ActionId { get; private set; }

        /// <summary>
        /// 是否数据压缩
        /// </summary>
        public bool Compress { get; private set; }

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool Secret { get; private set; }

        /// <summary>
        /// 是否有报错
        /// </summary>
        public bool IsError { get { return !string.IsNullOrEmpty(Error); } }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// 内容长度
        /// </summary>
        public int ContextLength { get; private set; } = 0;

        /// <summary>
        /// 读取的数据指针
        /// </summary>
        private int readIndex = 0;

        /// <summary>
        /// 通信包内容
        /// </summary>
        private byte[] context = null;

        /// <summary>
        /// 内容信息
        /// </summary>
        public byte[] GetContext()
        {
            return context;
        }

        /// <summary>
        /// 包体长度
        /// </summary>
        private int packetLength = 0;

        /// <summary>
        /// 包体长度
        /// </summary>
        public int GetPacketLength()
        {
            return packetLength;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="package"></param>
        /// <param name="offset"></param>
        public MessageReader(byte[] package, int offset, bool compress, bool secret)
        {
            Compress = compress;
            Secret = secret;
            packetLength = package.Length - offset;
            if (packetLength < HeaderLength)
            {
                Error = "The message's length is error.";
                return;
            }

            //读取报头
            ReadHeader(package, offset);

            //内容获取
            ContextLength = packetLength - HeaderLength;
            context = new byte[ContextLength];
            Buffer.BlockCopy(package, offset + HeaderLength, context, 0, ContextLength);
        }

        /// <summary>
        /// 消息的基本数据读取
        /// </summary>
        protected virtual void ReadHeader(byte[] package, int offset)
        {
            int index = offset;
            MsgId = BitConverter.ToInt64(package, index);
            index += SizeUtil.LongSize;
            OpCode = (sbyte)package[index];
            index += 1;
            ActionId = BitConverter.ToInt32(package, index);
        }

        /// <summary>
        /// 读取Boolean
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return ReadByte() == ByteUtil.ONE;
        }

        /// <summary>
        /// 读取Byte
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            byte b = context[readIndex];
            readIndex += 1;
            return b;
        }

        /// <summary>
        /// 读取Char
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            char c = BitConverter.ToChar(context, readIndex);
            readIndex += SizeUtil.CharSize;
            return c;
        }

        /// <summary>
        /// 读取时间
        /// </summary>
        /// <returns></returns>
        public DateTime ReadDateTime()
        {
            return new DateTime(ReadLong());
        }

        /// <summary>
        /// 读取Decimal
        /// </summary>
        /// <returns></returns>
        public decimal ReadDecimal()
        {
            int[] bits = new int[4];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = ReadInt();
            }
            return new decimal(bits);
        }

        /// <summary>
        /// 读取Double
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            double d = BitConverter.ToDouble(context, readIndex);
            readIndex += SizeUtil.DoubleSize;
            return d;
        }

        /// <summary>
        /// 读取Float
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            float f = BitConverter.ToSingle(context, readIndex);
            readIndex += SizeUtil.FloatSize;
            return f;
        }

        /// <summary>
        /// 读取Int
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            int i = BitConverter.ToInt32(context, readIndex);
            readIndex += SizeUtil.IntSize;
            return i;
        }

        /// <summary>
        /// 读取Long
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            long l = BitConverter.ToInt64(context, readIndex);
            readIndex += SizeUtil.LongSize;
            return l;
        }

        /// <summary>
        /// 读取SByte
        /// </summary>
        /// <returns></returns>
        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        /// <summary>
        /// 读取Short
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            short s = BitConverter.ToInt16(context, readIndex);
            readIndex += SizeUtil.ShortSize;
            return s;
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            int length = ReadInt();
            string str = Encoding.UTF8.GetString(context, readIndex, length);
            readIndex += length;
            return str;
        }

        /// <summary>
        /// 读取UInt
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt()
        {
            uint ui = BitConverter.ToUInt32(context, readIndex);
            readIndex += SizeUtil.UIntSize;
            return ui;
        }

        /// <summary>
        /// 读取ULong
        /// </summary>
        /// <returns></returns>
        public ulong ReadULong()
        {
            ulong ul = BitConverter.ToUInt64(context, readIndex);
            readIndex += SizeUtil.ULongSize;
            return ul;
        }

        /// <summary>
        /// 读取UShort
        /// </summary>
        /// <returns></returns>
        public ushort ReadUShort()
        {
            ushort us = BitConverter.ToUInt16(context, readIndex);
            readIndex += SizeUtil.UShortSize;
            return us;
        }
    }
}
