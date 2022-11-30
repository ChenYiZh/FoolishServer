using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
                    && EventArgs.AcceptSocket != null
                    && EventArgs.AcceptSocket.Connected;
            }
        }

        /// <summary>
        /// 地址
        /// </summary>
        public abstract IPEndPoint Address { get; }

        /// <summary>
        /// 原生套接字
        /// </summary>
        public virtual Socket Socket { get { return EventArgs?.AcceptSocket; } }

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
        public virtual IBoss MessageEventProcessor { get; protected set; } = new DirectMessageProcessor();

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
                if (EventArgs != null)
                {
                    ((UserToken)EventArgs.UserToken).ResetSendOrReceiveState(0);
                    if (EventArgs.AcceptSocket != null)
                    {
                        try
                        {
                            EventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
                            EventArgs.AcceptSocket.Close();
                            EventArgs.AcceptSocket.Dispose();
                        }
                        catch (Exception e)
                        {
                            FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Socket close error.", e);
                        }
                        finally
                        {
                            EventArgs.AcceptSocket = null;
                        }
                    }
                    UserToken userToken;
                    if ((userToken = EventArgs.UserToken as UserToken) != null && userToken.Socket == this)
                    {
                        userToken.Socket = null;
                    }
                }
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
