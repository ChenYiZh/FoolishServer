using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 通讯数据读取
    /// </summary>
    public interface IMessageReader : IMessageHeader
    {
        /// <summary>
        /// 读取Boolean
        /// </summary>
        /// <returns></returns>
        bool ReadBool();

        /// <summary>
        /// 读取Char
        /// </summary>
        /// <returns></returns>
        char ReadChar();

        /// <summary>
        /// 读取Float
        /// </summary>
        /// <returns></returns>
        float ReadFloat();

        /// <summary>
        /// 读取Double
        /// </summary>
        /// <returns></returns>
        double ReadDouble();

        /// <summary>
        /// 读取Decimal
        /// </summary>
        /// <returns></returns>
        decimal ReadDecimal();

        /// <summary>
        /// 读取SByte
        /// </summary>
        /// <returns></returns>
        sbyte ReadSByte();

        /// <summary>
        /// 读取Short
        /// </summary>
        /// <returns></returns>
        short ReadShort();

        /// <summary>
        /// 读取Int
        /// </summary>
        /// <returns></returns>
        int ReadInt();

        /// <summary>
        /// 读取Long
        /// </summary>
        /// <returns></returns>
        long ReadLong();

        /// <summary>
        /// 读取Byte
        /// </summary>
        /// <returns></returns>
        byte ReadByte();

        /// <summary>
        /// 读取UShort
        /// </summary>
        /// <returns></returns>
        ushort ReadUShort();

        /// <summary>
        /// 读取UInt
        /// </summary>
        /// <returns></returns>
        uint ReadUInt();

        /// <summary>
        /// 读取ULong
        /// </summary>
        /// <returns></returns>
        ulong ReadULong();

        /// <summary>
        /// 读取时间
        /// </summary>
        /// <returns></returns>
        DateTime ReadDateTime();

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <returns></returns>
        string ReadString();
    }
}
