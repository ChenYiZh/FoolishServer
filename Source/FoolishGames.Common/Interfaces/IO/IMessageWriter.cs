/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2030 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/
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
