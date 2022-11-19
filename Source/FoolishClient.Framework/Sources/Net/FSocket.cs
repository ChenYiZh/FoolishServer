using FoolishClient.Delegate;
using FoolishClient.Log;
using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.RPC;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FoolishClient.Net
{
    /// <summary>
    /// 套接字父类
    /// </summary>
    public abstract class FSocket : ISocket
    {
        /// <summary>
        /// 标识名称
        /// </summary>
        public virtual string Name { get; private set; }

        /// <summary>
        /// 消息计数
        /// </summary>
        private long messageNumber = DateTime.Now.Ticks;

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 内部套接字
        /// </summary>
        public virtual Socket Socket { get; private set; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        public int MessageOffset { get; set; } = 2;

        /// <summary>
        /// 压缩工具
        /// </summary>
        public ICompression Compression { get; set; } = null;

        /// <summary>
        /// 加密工具
        /// </summary>
        public ICryptoProvider CryptoProvide { get; set; } = null;

        /// <summary>
        /// 运行的标识
        /// </summary>
        private int readyFlag = 0;

        /// <summary>
        /// 数据是否已经初始化了
        /// </summary>
        public virtual bool IsReady { get { return readyFlag == 1; } }

        /// <summary>
        /// 是否已经开始工作
        /// </summary>
        public virtual bool IsRunning { get; protected set; } = false;

        /// <summary>
        /// 是否已经开始运行
        /// </summary>
        public virtual bool Connected { get { return Socket != null && Socket.Connected; } }

        /// <summary>
        /// 心跳间隔
        /// </summary>
        public int HeartbeatInterval { get; private set; }

        /// <summary>
        /// 心跳包线程
        /// </summary>
        internal protected virtual Timer HeartbeatTimer { get; set; } = null;

        /// <summary>
        /// 心跳包
        /// </summary>
        private byte[] heartbeatBuffer;

        /// <summary>
        /// 线程循环等待时间
        /// </summary>
        private const int THREAD_SLEEP_TIMEOUT = 5;

        /// <summary>
        /// 数据接收线程
        /// </summary>
        internal protected virtual Thread ThreadProcessReceive { get; set; } = null;

        /// <summary>
        /// 数据发送线程
        /// </summary>
        internal protected virtual Thread ThreadProcessSend { get; set; } = null;

        /// <summary>
        /// 等待发送的数据
        /// </summary>
        internal protected virtual IThreadSafeQueue<IWaitSendMessage> WaitToSendMessages { get; set; } = null;

        /// <summary>
        /// 设置类别名称
        /// </summary>
        protected virtual string Category { get; set; }

        /// <summary>
        /// 初始化Socket基本信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="heartbeatInterval">心跳间隔</param>
        public virtual void Ready(string name, string host, int port,
            int heartbeatInterval = 10000)
        {
            Name = name;
            Host = host;
            Port = port;
            Category = string.Format("{0}:{1},{2}", GetType().Name, Host, Port);
            HeartbeatInterval = heartbeatInterval;
            WaitToSendMessages = new ThreadSafeQueue<IWaitSendMessage>();
            FConsole.WriteInfoFormatWithCategory(Category, "Socket is ready...");
            Interlocked.Exchange(ref readyFlag, 1);
        }

        /// <summary>
        /// 连接函数[内部异步实现]
        /// </summary>
        public virtual void ConnectAsync(Action<bool> callback = null)
        {
            IsRunning = true;
            ThreadPool.QueueUserWorkItem((state) =>
            {
                bool success = Connect();
                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// 同步连接
        /// </summary>
        protected virtual bool Connect()
        {
            if (!IsReady)
            {
                IsRunning = false;
                FConsole.WriteInfoFormatWithCategory(Categories.SOCKET, "Socket is not ready!");
                return false;
            }
            IsRunning = true;
            FConsole.WriteInfoFormatWithCategory(Category, "Socket is starting...");
            try
            {
                Socket = MakeSocket();
                IAsyncResult opt = Socket.BeginConnect(Host, Port, null, Socket);
                bool success = opt.AsyncWaitHandle.WaitOne(1000, true);
                if (!success || !opt.IsCompleted || !Socket.Connected)
                {
                    IsRunning = false;
                    throw new Exception(string.Format("Socket connect failed!"));
                }
                //Socket.Connect(host, port);//手机上测下来只有同步才有效
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Category, "Fail to connect the server!", e);
                Close();
                return false;
            }
            if (HeartbeatTimer == null)
            {
                //心跳包线程
                HeartbeatTimer = new Timer(SendHeartbeatPackage, null, HeartbeatInterval, HeartbeatInterval);
                RebuildHeartbeatPackage();
            }
            //开始监听数据
            ThreadProcessReceive = new Thread(new ThreadStart(ProcessReceive));
            ThreadProcessReceive.Start();

            //开启发送线程
            ThreadProcessSend = new Thread(new ThreadStart(ProcessSend));
            ThreadProcessSend.Start();

            FConsole.WriteInfoFormatWithCategory(Category, "Socket connected.");

            return true;
        }

        protected abstract Socket MakeSocket();

        /// <summary>
        /// 发送心跳包
        /// </summary>
        /// <param name="state"></param>
        internal protected virtual void SendHeartbeatPackage(object state)
        {
            const string error = "Send heartbeat package failed.";
            try
            {
                // 心跳包
                IAsyncResult result;
                if (heartbeatBuffer != null && !Send(heartbeatBuffer, out result))
                {
                    FConsole.WriteErrorFormatWithCategory(Category, error);
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Category, error, e);
            }
        }

        /// <summary>
        /// 重新构建心跳包数据
        /// </summary>
        public virtual void RebuildHeartbeatPackage()
        {
            heartbeatBuffer = BuildHeartbeatBuffer();
        }

        /// <summary>
        /// 创建默认心跳包数据
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] BuildHeartbeatBuffer()
        {
            // 创建默认心跳包数据
            MessageWriter msg = new MessageWriter();
            msg.OpCode = (sbyte)EOpCode.Pong;
            return PackageFactory.Pack(msg, MessageOffset, null, null);
        }

        /// <summary>
        /// 数据发送<c>异步</c>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback">发送回调</param>
        /// <returns>判断有没有发送出去</returns>
        public virtual void SendAsync(IMessageWriter message, SendCallback callback = null)
        {
            if (IsReady)
            {
                if (!IsRunning)
                {
                    ConnectAsync();
                }
                WaitToSendMessages.Enqueue(new WaitSendMessage(message, callback));
            }
        }

        /// <summary>
        /// 线程发送数据
        /// </summary>
        internal protected virtual void ProcessSend()
        {
            while (ThreadProcessSend != null && WaitToSendMessages != null)
            {
                while (WaitToSendMessages.Count > 0)
                {
                    WaitSendMessage message = WaitToSendMessages.Dequeue() as WaitSendMessage;
                    if (message != null)
                    {
                        if (message.Message == null)
                        {
                            message.Execute(false, null);
                        }
                        else
                        {
                            if (Socket == null)
                            {
                                Connect();
                            }
                            if (Connected)
                            {
                                Interlocked.Increment(ref messageNumber);
                                IAsyncResult result;
                                message.Message.MsgId = messageNumber;
                                byte[] data = PackageFactory.Pack(message.Message, MessageOffset, Compression, CryptoProvide);
                                try
                                {
                                    message.Execute(Send(data, out result), result);
                                }
                                //catch (Exception e)
                                catch
                                {
                                    Close();
                                }
                            }
                            else
                            {
                                message.Execute(false, null);
                            }
                        }
                    }
                }
                Thread.Sleep(THREAD_SLEEP_TIMEOUT);
            }
        }

        /// <summary>
        /// 以同步方式发送
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal protected virtual bool Send(byte[] data, out IAsyncResult result)
        {
            if (data == null)
            {
                result = null;
                return false;
            }
            IAsyncResult asyncSend = Socket.BeginSend(data, 0, data.Length, SocketFlags.None, null, Socket);
            result = asyncSend;
            if (!asyncSend.AsyncWaitHandle.WaitOne(5000, true))
            {
                FConsole.WriteErrorFormatWithCategory(Category, "Process send error and close socket");
                Close();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 线程处理数据接收
        /// </summary>
        internal protected virtual void ProcessReceive()
        {

        }

        /// <summary>
        /// 关闭函数
        /// </summary>
        public virtual void Close()
        {
            try
            {
                lock (this)
                {
                    IsRunning = false;
                    if (Socket != null)
                    {
                        try
                        {
                            Socket.Shutdown(SocketShutdown.Both);
                            Socket.Close();
                        }
                        catch (Exception e)
                        {
                            FConsole.WriteExceptionWithCategory(Category, "Socket close error.", e);
                        }
                        Socket = null;
                    }
                    if (HeartbeatTimer != null)
                    {
                        try
                        {
                            HeartbeatTimer.Dispose();
                        }
                        catch (Exception e)
                        {
                            FConsole.WriteExceptionWithCategory(Category, "HeartbeatTime dispose error.", e);
                        }
                        HeartbeatTimer = null;
                    }
                    if (ThreadProcessReceive != null)
                    {
                        try
                        {
                            ThreadProcessReceive.Abort();
                        }
                        catch (Exception e)
                        {
                            FConsole.WriteExceptionWithCategory(Category, "ThreadProcessReceive abort error.", e);
                        }
                        ThreadProcessReceive = null;
                    }
                    if (ThreadProcessSend != null)
                    {
                        try
                        {
                            ThreadProcessSend.Abort();
                        }
                        catch (Exception e)
                        {
                            FConsole.WriteExceptionWithCategory(Category, "ThreadProcessSend abort error.", e);
                        }
                        ThreadProcessSend = null;
                    }
                }
                WaitToSendMessages.Clear();
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Category, e);
            }
            FConsole.WriteInfoFormatWithCategory(Category, "Socket Closed.");
        }
    }
}
