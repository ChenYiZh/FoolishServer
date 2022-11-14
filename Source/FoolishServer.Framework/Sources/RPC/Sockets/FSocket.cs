using FoolishGames.Collections;
using FoolishServer.RPC;
using FoolishServer.RPC.Sockets;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FoolishGames.Log;

namespace FoolishServer.RPC.Sockets
{
    /// <summary>
    /// 套接字嵌套层
    /// </summary>
    public class FSocket : ISocket
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        public Guid HashCode { get; private set; }

        /// <summary>
        /// 获取时间
        /// </summary>
        public DateTime AccessTime { get; internal set; }

        ///// <summary>
        ///// 发送状态标识
        ///// </summary>
        //private int sendingFlag = 0;

        ///// <summary>
        ///// 正在发送消息？
        ///// </summary>
        //public bool IsSending { get { return Interlocked.CompareExchange(ref sendingFlag, 1, 0) == 1; } }

        /// <summary>
        /// Socket是否还连接着？
        /// </summary>
        public bool Connected { get { return Socket == null ? false : Socket.Connected; } }

        /// <summary>
        /// 待发送队列
        /// </summary>
        private ThreadSafeQueue<ISocketAsyncResult> sendQueue { get; set; }

        /// <summary>
        /// 待发送的消息队列长度
        /// </summary>
        public int MessageQueueCount { get { return sendQueue.Count; } }

        /// <summary>
        /// 是否在运行
        /// </summary>
        public bool IsRunning { get; internal set; } = true;

        /// <summary>
        /// 地址
        /// </summary>
        public IPEndPoint Address { get; private set; }

        /// <summary>
        /// 内部关键原生Socket
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// 类型
        /// </summary>
        public EServerType Type { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public FSocket(Socket socket)
        {
            HashCode = Guid.NewGuid();
            sendQueue = new ThreadSafeQueue<ISocketAsyncResult>();
            Socket = socket;
            try
            {
                Address = socket.RemoteEndPoint as IPEndPoint;
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.SOCKET, e);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            try
            {
                Socket?.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.SOCKET, e);
            }
            finally
            {
                Socket = null;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void Send(byte[] data, Action<ISocketAsyncResult> callback)
        {
            if (Socket == null)
            {
                return;// false;
            }
            lock (Socket)
            {
                sendQueue.Enqueue(new SocketAsyncResult() { Buffer = data, Socket = this, Result = EResultCode.Wait });
                //return !IsSending;
            }
        }

        /// <summary>
        /// 获取最早等待发送的消息
        /// </summary>
        /// <returns>是否有消息</returns>
        public bool TryDequeueOrReset(out ISocketAsyncResult result)
        {
            result = null;
            if (Socket == null)
            {
                return false;
            }
            lock (Socket)
            {
                if (sendQueue.Count > 0)
                {
                    result = sendQueue.Dequeue();
                    return true;
                }
                //Interlocked.Exchange(ref sendingFlag, 0);
                return false;
            }
        }

        /// <summary>
        /// 重置唯一id
        /// </summary>
        /// <param name="key">新的id</param>
        public void ResetHashset(Guid key)
        {
            HashCode = key;
        }

        /// <summary>
        /// 重置发送标识
        /// </summary>
        public void ResetSendFlag()
        {
            //Interlocked.Exchange(ref sendingFlag, 0);
        }
    }
}
