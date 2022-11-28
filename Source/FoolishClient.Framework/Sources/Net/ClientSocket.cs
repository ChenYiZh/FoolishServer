using FoolishClient.Delegate;
using FoolishClient.Log;
using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Net;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FoolishClient.Net
{
    /// <summary>
    /// 套接字父类
    /// </summary>
    public abstract class ClientSocket : FSocket, ISendableSocket, IReceivableSocket, IMsgSocket
    {
        /// <summary>
        /// 地址
        /// </summary>
        protected IPEndPoint address = null;

        /// <summary>
        /// 地址
        /// </summary>
        public override IPEndPoint Address { get { return address; } }

        /// <summary>
        /// 标识名称
        /// </summary>
        public virtual string Name { get; private set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 运行的标识
        /// </summary>
        private int readyFlag = 0;

        /// <summary>
        /// 数据是否已经初始化了
        /// </summary>
        public virtual bool IsReady { get { return readyFlag == 1; } }

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

        ///// <summary>
        ///// 线程循环等待时间
        ///// </summary>
        //private const int THREAD_SLEEP_TIMEOUT = 5;

        ///// <summary>
        ///// 数据接收线程
        ///// </summary>
        //internal protected virtual Thread ThreadProcessReceive { get; set; } = null;

        /// <summary>
        /// 设置类别名称
        /// </summary>
        protected virtual string Category { get; set; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        private int messageOffset = 2;

        /// <summary>
        /// 消息偏移值
        /// </summary>
        public override int MessageOffset { get { return messageOffset; } }

        /// <summary>
        /// 压缩工具
        /// </summary>
        private ICompression compression = null;

        /// <summary>
        /// 压缩工具
        /// </summary>
        public override ICompression Compression { get { return compression; } }

        /// <summary>
        /// 加密工具
        /// </summary>
        private ICryptoProvider cryptoProvider = null;

        /// <summary>
        /// 加密工具
        /// </summary>
        public override ICryptoProvider CryptoProvider { get { return cryptoProvider; } }

        /// <summary>
        /// 发送的管理类
        /// </summary>
        internal protected ISender Sender { get; private set; }

        /// <summary>
        /// 接收管理类
        /// </summary>
        internal protected IReceiver Receiver { get; private set; }

        /// <summary>
        /// 消息Id
        /// </summary>
        public long MessageNumber { get { return Sender.MessageNumber; } set { Sender.MessageNumber = value; } }

        /// <summary>
        /// 初始化
        /// </summary>
        protected ClientSocket() : base(null)
        {
        }

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
            address = new IPEndPoint(IPAddress.Parse(host), port);
            Category = string.Format("{0}:{1},{2}", GetType().Name, Host, Port);
            HeartbeatInterval = heartbeatInterval;
            FConsole.WriteInfoFormatWithCategory(Category, "Socket is ready...");
            Interlocked.Exchange(ref readyFlag, 1);
        }

        /// <summary>
        /// 自动连接
        /// </summary>
        public virtual void AutoConnect()
        {
            if (!IsRunning)
            {
                ConnectAsync();
            }
        }

        /// <summary>
        /// 连接函数[内部异步实现]
        /// </summary>
        public virtual void ConnectAsync(Action<bool> callback = null)
        {
            IsRunning = true;
            ThreadPool.UnsafeQueueUserWorkItem((state) =>
            {
                bool success = Connect();
                (state as Action<bool>)?.Invoke(success);
            }, callback);
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
                Socket socket = MakeSocket();
                EventArgs = MakeEventArgs(socket);
                EventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageSolved);
                Sender = new SocketSender(this);
                Receiver = new SocketReceiver(this);
                IAsyncResult opt = socket.BeginConnect(Address, null, EventArgs);
                bool success = opt.AsyncWaitHandle.WaitOne(1000, true);
                if (!success || !opt.IsCompleted || !socket.Connected)
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
            ////开始监听数据
            //ThreadProcessReceive = new Thread(new ThreadStart(ProcessReceive));
            //ThreadProcessReceive.Start();

            BeginReceive();
            Sender.BeginSend();

            FConsole.WriteInfoFormatWithCategory(Category, "Socket connected.");

            return true;
        }

        /// <summary>
        /// 开始执行发送
        /// </summary>
        void ISender.BeginSend()
        {
            Sender.BeginSend();
        }

        /// <summary>
        /// 创建原生套接字
        /// </summary>
        protected abstract Socket MakeSocket();

        /// <summary>
        /// 当消息处理完执行
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
        /// </summary>
        private void MessageSolved(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    Receiver.ProcessReceive();
                    break;
                case SocketAsyncOperation.Send:
                    Sender.ProcessSend();
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        internal protected virtual void SendHeartbeatPackage(object state)
        {
            const string error = "Send heartbeat package failed.";
            try
            {
                // 心跳包
                if (heartbeatBuffer != null)
                {
                    Sender.SendBytesImmediately(heartbeatBuffer);
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
        /// 关闭函数
        /// </summary>
        public override void Close(EOpCode opCode = EOpCode.Close)
        {
            base.Close(opCode);
            lock (this)
            {
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
                //if (ThreadProcessReceive != null)
                //{
                //    try
                //    {
                //        ThreadProcessReceive.Abort();
                //    }
                //    catch (Exception e)
                //    {
                //        FConsole.WriteExceptionWithCategory(Category, "ThreadProcessReceive abort error.", e);
                //    }
                //    ThreadProcessReceive = null;
                //}          

                //管理类回收
                EventArgs.Dispose();
                EventArgs = null;

                Sender = null;
                Receiver = null;
            }
            FConsole.WriteInfoFormatWithCategory(Category, "Socket Closed.");
        }
        /// <summary>
        /// 设置消息的偏移值
        /// </summary>
        public void SetMessageOffset(int offset)
        {
            messageOffset = offset;
        }
        /// <summary>
        /// 设置压缩方案
        /// </summary>
        public void SetCompression(ICompression compression)
        {
            this.compression = compression;
        }
        /// <summary>
        /// 设置解密方案
        /// </summary>
        public void SetCryptoProvide(ICryptoProvider cryptoProvider)
        {
            this.cryptoProvider = cryptoProvider;
        }
        /// <summary>
        /// 数据发送<c>异步</c>
        /// </summary>
        /// <param name="message">大宋的消息</param>
        /// <returns>判断有没有发送出去</returns>
        public void Send(IMessageWriter message)
        {
            Sender.Send(message);
            AutoConnect();
        }
        /// <summary>
        /// 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用
        /// </summary>
        /// <param name="message">发送的消息</param>
        [Obsolete("Only used in important message. This method will confuse the message queue. You can use 'Send' instead.", false)]
        public void SendImmediately(IMessageWriter message)
        {
            Sender.SendImmediately(message);
            AutoConnect();
        }
        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析
        /// </summary>
        void ISender.SendBytes(byte[] data)
        {
            Sender.SendBytes(data);
            AutoConnect();
        }
        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析，以及解析顺序
        /// </summary>
        void ISender.SendBytesImmediately(byte[] data)
        {
            Sender.SendBytesImmediately(data);
            AutoConnect();
        }
        /// <summary>
        /// 消息发送处理
        /// </summary>
        void ISender.ProcessSend()
        {
            Sender.ProcessSend();
        }
        /// <summary>
        /// 等待消息接收
        /// </summary>
        public void BeginReceive()
        {
            Receiver.BeginReceive();
        }
        /// <summary>
        /// 处理数据接收回调
        /// </summary>
        void IReceiver.ProcessReceive()
        {
            Receiver.ProcessReceive();
        }
    }
}
