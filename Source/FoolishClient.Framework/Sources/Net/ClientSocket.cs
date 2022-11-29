using FoolishClient.Action;
using FoolishClient.Delegate;
using FoolishClient.Log;
using FoolishClient.Proxy;
using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Net;
using FoolishGames.Proxy;
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
    public abstract class ClientSocket : FSocket, IClientSocket
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
        internal protected SocketSender Sender { get; private set; }

        /// <summary>
        /// 接收管理类
        /// </summary>
        internal protected SocketReceiver<IClientSocket> Receiver { get; private set; }

        /// <summary>
        /// 消息Id
        /// </summary>
        public long MessageNumber { get { return Sender.MessageNumber; } set { Sender.MessageNumber = value; } }

        /// <summary>
        /// Action生成类
        /// </summary>
        public IClientActionDispatcher ActionProvider { get; set; }

        /// <summary>
        /// 消息处理的中转站
        /// </summary>
        public IBoss MessageContractor { get; set; }

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
        /// <param name="actionClassFullName">Action协议类的完整名称</param>
        /// <param name="heartbeatInterval">心跳间隔</param>
        public virtual void Ready(string name, string host, int port,
            string actionClassFullName,
            int heartbeatInterval = 10000)
        {
            Name = name;
            Host = host;
            Port = port;
            address = new IPEndPoint(IPAddress.Parse(host), port);
            Category = string.Format("{0}:{1},{2}", GetType().Name, Host, Port);
            ActionProvider = new ClientActionDispatcher(actionClassFullName);
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
            Awake();
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
            Awake();
            FConsole.WriteInfoFormatWithCategory(Category, "Socket is starting...");
            try
            {
                EventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageSolved);
                IAsyncResult opt = Socket.BeginConnect(Address, null, EventArgs);
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
            ////开始监听数据
            //ThreadProcessReceive = new Thread(new ThreadStart(ProcessReceive));
            //ThreadProcessReceive.Start();

            //BeginReceive();
            Sender.BeginSend();

            FConsole.WriteInfoFormatWithCategory(Category, "Socket connected.");

            return true;
        }

        /// <summary>
        /// 初始化执行
        /// </summary>
        protected virtual void Awake()
        {
            if (Socket == null)
            {
                Socket socket = MakeSocket();
                //socket.ReceiveTimeout = 50;//此方法只能在同步模式下使用
                EventArgs = MakeEventArgs(socket);
            }
            if (Sender == null)
            {
                Sender = new SocketSender(this);
            }
            if (Receiver == null)
            {
                Receiver = new SocketReceiver<IClientSocket>(this);
                Receiver.OnMessageReceived = OnMessageReceived;
            }
        }

        /// <summary>
        /// 创建原生套接字
        /// </summary>
        protected abstract Socket MakeSocket();

        /// <summary>
        /// 等待的线程锁，防止产生多组线程
        /// </summary>
        private int waitingFlag = 0;

        /// <summary>
        /// 当消息处理完执行
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
        /// </summary>
        private void MessageSolved(object sender, SocketAsyncEventArgs e)
        {
            // 等待操作处理
            bool bWaiting = false;
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    bWaiting = !Receiver.ProcessReceive();
                    break;
                case SocketAsyncOperation.Send:
                    bWaiting = !Sender.ProcessSend();
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
            if (bWaiting && Interlocked.CompareExchange(ref waitingFlag, 1, 0) == 0)
            {
                while (Receiver != null && Sender != null && Receiver.BeginReceive() && Sender.BeginSend())
                {
                    Thread.Sleep(10);
                }
                Interlocked.Exchange(ref waitingFlag, 0);
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
                if (!Socket.Connected)
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
                // 心跳包
                if (heartbeatBuffer != null)
                {
                    Sender.SendBytesImmediately(heartbeatBuffer);
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
                if (EventArgs != null)
                {
                    EventArgs.Dispose();
                }
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
            AutoConnect();
            Sender.Send(message);
        }
        /// <summary>
        /// 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用
        /// </summary>
        /// <param name="message">发送的消息</param>
        [Obsolete("Only used in important message. This method will confuse the message queue. You can use 'Send' instead.", false)]
        public void SendImmediately(IMessageWriter message)
        {
            AutoConnect();
            Sender.SendImmediately(message);
        }
        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析
        /// </summary>
        internal protected void SendBytes(byte[] data)
        {
            AutoConnect();
            Sender.SendBytes(data);
        }
        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析，以及解析顺序
        /// </summary>
        internal protected void SendBytesImmediately(byte[] data)
        {
            AutoConnect();
            Sender.SendBytesImmediately(data);
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="e"></param>
        private void OnMessageReceived(IMessageEventArgs<IClientSocket> args)
        {
            try
            {
                if (MessageContractor != null)
                {
                    MessageContractor.CheckIn(new MessageWorker { Message = args.Message, Socket = this });
                }
                else
                {
                    ThreadPool.QueueUserWorkItem((obj) => { ProcessMessage(args.Message); });
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Category, "Process message error.", e);
            }
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        internal virtual void ProcessMessage(IMessageReader message)
        {
            if (message == null)
            {
                FConsole.WriteErrorFormatWithCategory(Category, "{0} receive empty message.", Category);
                return;
            }
            try
            {
                FConsole.Write("MsgId: " + message.ActionId);
                ClientAction action = ActionProvider.Provide(message.ActionId);
                try
                {
                    ActionBoss.Exploit(action, message.ActionId, message);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Category, "Action error.", e);
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Category, "ActionProvider error.", e);
            }
        }
    }
}
