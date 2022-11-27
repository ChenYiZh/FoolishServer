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
    /// <summary>
    /// 数据发送的回调
    /// </summary>
    /// <param name="success">操作是否成功，不包含结果</param>
    /// <param name="result">同步的结果</param>
    public delegate void SendCallback(bool success, IAsyncResult result);

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
                return Socket != null
                    && Socket.AcceptSocket != null
                    && Socket.AcceptSocket.Connected;
            }
        }

        /// <summary>
        /// 地址
        /// </summary>
        public virtual IPEndPoint Address { get; protected set; }

        /// <summary>
        /// 内部关键原生Socket
        /// </summary>
        public virtual SocketAsyncEventArgs Socket { get; protected set; }

        /// <summary>
        /// 类型
        /// </summary>
        public virtual ESocketType Type { get; protected set; }

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
        protected FSocket(SocketAsyncEventArgs socket)
        {
            if (socket == null)
            {
                throw new NullReferenceException("SocketAsyncEventArgs is null! Create socket failed.");
            }
            UserToken userToken;
            if ((userToken = socket.UserToken as UserToken) == null)
            {
                userToken = new UserToken();
                socket.UserToken = userToken;
            }
            userToken.Socket = this;
        }

        /// <summary>
        /// 关闭函数
        /// </summary>
        public virtual void Close()
        {
            lock (this)
            {
                IsRunning = false;
                if (Socket != null && Socket.AcceptSocket != null)
                {
                    try
                    {
                        Socket.AcceptSocket.Shutdown(SocketShutdown.Both);
                        Socket.AcceptSocket.Close();
                        Socket.AcceptSocket.Dispose();
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.SOCKET, "Socket close error.", e);
                    }
                    finally
                    {
                        Socket.AcceptSocket = null;
                    }
                }
            }
        }

        /// <summary>
        /// 创建Socket的超类
        /// </summary>
        public static SocketAsyncEventArgs MakeSocket(Socket socket, int bufferSize = 8192)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[bufferSize], 0, bufferSize);
            UserToken userToken = new UserToken();
            args.AcceptSocket = socket;
            return args;
        }
    }
}
