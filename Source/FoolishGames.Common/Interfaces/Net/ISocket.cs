/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
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
using FoolishGames.IO;
using FoolishGames.Proxy;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 套接字扩充接口
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// 是否在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Socket是否还连接着？
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 封装的地址
        /// </summary>
        IPEndPoint Address { get; }

        /// <summary>
        /// 原生套接字
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// 套接字增强类
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
        /// </summary>
        SocketAsyncEventArgs EventArgs { get; }

        /// <summary>
        /// 获取类型
        /// </summary>
        ESocketType Type { get; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        int MessageOffset { get; }

        /// <summary>
        /// 压缩工具
        /// </summary>
        ICompression Compression { get; }

        /// <summary>
        /// 加密工具
        /// </summary>
        ICryptoProvider CryptoProvider { get; }

        /// <summary>
        /// 消息处理方案
        /// </summary>
        IBoss MessageEventProcessor { get; }

        /// <summary>
        /// 关闭函数
        /// </summary>
        void Close(EOpCode opCode = EOpCode.Close);
    }
}
