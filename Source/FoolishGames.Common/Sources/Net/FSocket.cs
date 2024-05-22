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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Proxy;
using FoolishGames.Security;

namespace FoolishGames.Net
{
    ///// <summary>
    ///// 数据发送的回调
    ///// </summary>
    ///// <param name="success">操作是否成功，不包含结果</param>
    ///// <param name="result">同步的结果</param>
    //public delegate void SendCallback(bool success, IAsyncResult result);

    /// <summary>
    /// 套接字管理基类
    /// </summary>
    public abstract class FSocket : ISocket
    {
        /// <summary>
        /// 是否在运行
        /// </summary>
        public virtual bool IsRunning { get; protected set; } = false;

        /// <summary>
        /// 是否已经开始运行
        /// </summary>
        public virtual bool Connected
        {
            get
            {
                return EventArgs != null
                    && Socket != null
                    && Socket.Connected;
            }
        }

        /// <summary>
        /// 地址
        /// </summary>
        public abstract IPEndPoint Address { get; }

        /// <summary>
        /// 原生套接字
        /// </summary>
        public virtual Socket Socket { get; protected set; }

        /// <summary>
        /// 内部关键原生Socket
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
        /// </summary>
        public virtual SocketAsyncEventArgs EventArgs { get; protected set; }

        /// <summary>
        /// 类型
        /// </summary>
        // TODO: 添加新的类型时需要修改构造函数
        public abstract ESocketType Type { get; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        public abstract int MessageOffset { get; }

        /// <summary>
        /// 压缩工具
        /// </summary>
        public abstract ICompression Compression { get; }

        /// <summary>
        /// 加密工具
        /// </summary>
        public abstract ICryptoProvider CryptoProvider { get; }

        /// <summary>
        /// 消息处理方案
        /// </summary>
        public virtual IBoss MessageEventProcessor { get; set; } = new DirectMessageProcessor();

        /// <summary>
        /// 初始化
        /// </summary>
        protected FSocket(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                //throw new NullReferenceException("SocketAsyncEventArgs is null! Create socket failed.");
                return;
            }
            EventArgs = eventArgs;
            Socket = EventArgs.AcceptSocket;
            UserToken userToken = eventArgs.UserToken as UserToken;
            if (userToken == null)
            {
                userToken = new UserToken(eventArgs);
                eventArgs.UserToken = userToken;
            }
            userToken.Socket = this;
        }

        /// <summary>
        /// 关闭函数
        /// </summary>
        public virtual void Close(EOpCode opCode = EOpCode.Close)
        {
            lock (this)
            {
                IsRunning = false;

                DisposeEventArgs();

                if (Socket != null)
                {
                    try
                    {
                        Socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception e)
                    {
                        //FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Socket shutdown error.", e);
                    }
                    //try
                    //{
                    //    Socket.Disconnect(false);
                    //}
                    //catch (Exception e)
                    //{
                    //    FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Socket disconnect error.", e);
                    //}
                    try
                    {
                        Socket.Close();
                        Socket.Dispose();
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Socket close error.", e);
                    }
                    finally
                    {
                        Socket = null;
                    }
                }
            }
        }

        protected void DisposeEventArgs()
        {
            if (EventArgs != null)
            {
                try
                {
                    ((UserToken)EventArgs.UserToken).ResetSendOrReceiveState(0);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.SOCKET, "UserToken reset error.", e);
                }

                UserToken userToken;
                if ((userToken = EventArgs.UserToken as UserToken) != null && userToken.Socket == this)
                {
                    userToken.Socket = null;
                }

                if (EventArgs.ConnectSocket != null)
                {
                    try
                    {
                        EventArgs.ConnectSocket.Close();
                        EventArgs.ConnectSocket.Dispose();
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.SOCKET, "EventArgs close socket error.", e);
                    }
                }

                try
                {
                    EventArgs.Dispose();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.SOCKET, "EventArgs dispose error.", e);
                }
                EventArgs = null;
            }
        }

        /// <summary>
        /// 创建Socket的超类
        /// </summary>
        public static SocketAsyncEventArgs MakeEventArgs(Socket socket, byte[] buffer = null, int offset = 0, int bufferSize = 8192)
        {
            if (buffer == null)
            {
                buffer = new byte[offset + bufferSize];
            }
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            // 设置缓冲区大小
            args.SetBuffer(buffer, offset % buffer.Length, bufferSize);
            UserToken userToken = new UserToken(args);
            args.UserToken = userToken;
            userToken.SetOriginalOffset(offset, bufferSize);
            args.AcceptSocket = socket;
            return args;
        }
    }
}
