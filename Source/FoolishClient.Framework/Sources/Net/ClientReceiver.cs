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

using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FoolishClient.Net;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息接收处理类
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
    /// </summary>
    public abstract class ClientReceiver : SocketReceiver<IClientSocket>
    {
        /// <summary>
        /// 增强类
        /// </summary>
        public SocketAsyncEventArgs EventArgs
        {
            get { return Socket.EventArgs; }
        }

        /// <summary>
        /// 数据管理对象
        /// </summary>
        internal UserToken UserToken { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public ClientReceiver(ISocket socket) : base(socket)
        {
            if (socket.EventArgs == null)
            {
                throw new NullReferenceException(
                    "Fail to create socket receiver, because the SocketAsyncEventArgs is null.");
            }

            UserToken usertoken = socket.UserToken;
            if (usertoken == null)
            {
                throw new NullReferenceException(
                    "Fail to create socket receiver, because the UserToken of SocketAsyncEventArgs is null.");
            }

            UserToken = usertoken;
        }

        /// <summary>
        /// 关闭操作
        /// </summary>
        protected override void Close(SocketAsyncEventArgs ioEventArgs, EOpCode opCode)
        {
            if (ioEventArgs != null && ioEventArgs.UserToken != null)
            {
                ((UserToken) ioEventArgs.UserToken).Socket?.Close(opCode);
            }
        }
    }
}