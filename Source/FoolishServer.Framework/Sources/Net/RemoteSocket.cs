using FoolishGames.Collections;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FoolishGames.Log;
using FoolishGames.Net;
using FoolishGames.IO;
using FoolishGames.Security;

namespace FoolishServer.Net
{
    /// <summary>
    /// 套接字嵌套层
    /// </summary>
    public sealed class RemoteSocket : FSocket, IRemoteSocket
    {
        /// <summary>
        /// 是否在运行
        /// </summary>
        internal void SetIsRunning(bool value) { IsRunning = value; }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public IServerSocket Server { get; private set; }

        /// <summary>
        /// 唯一id
        /// </summary>
        public Guid HashCode { get; private set; }

        /// <summary>
        /// 获取时间
        /// </summary>
        public DateTime AccessTime { get; internal set; }

        /// <summary>
        /// 地址
        /// </summary>
        private IPEndPoint address = null;

        /// <summary>
        /// 地址
        /// </summary>
        public override IPEndPoint Address { get { return address; } }

        /// <summary>
        /// 类型
        /// </summary>
        public override ESocketType Type { get { return Server.Type; } }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        public override int MessageOffset { get { return Server.MessageOffset; } }

        /// <summary>
        /// 压缩工具
        /// </summary>
        public override ICompression Compression { get { return Server.Compression; } }

        /// <summary>
        /// 加密工具
        /// </summary>
        public override ICryptoProvider CryptoProvider { get { return Server.CryptoProvider; } }

        /// <summary>
        /// 发送的管理类
        /// </summary>
        internal SocketSender Sender { get; private set; }

        /// <summary>
        /// 接收管理类
        /// </summary>
        internal SocketReceiver Receiver { get; private set; }

        /// <summary>
        /// 消息Id
        /// </summary>
        public long MessageNumber { get { return Sender.MessageNumber; } set { Sender.MessageNumber = value; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RemoteSocket(IServerSocket server, SocketAsyncEventArgs eventArgs) : base(eventArgs)
        {
            if (server == null)
            {
                throw new NullReferenceException("Fail to create remove socket, because the server is null.");
            }
            Sender = new SocketSender(this);
            Receiver = new SocketReceiver(this);
            Server = server;
            IsRunning = true;
            HashCode = Guid.NewGuid();
            try
            {
                address = Socket.RemoteEndPoint as IPEndPoint;
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.SOCKET, e);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public override void Close(EOpCode opCode = EOpCode.Close)
        {
            base.Close(opCode);
            Sender = null;
            Receiver = null;
            Server.OnRemoteSocketClosed(this, opCode);
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
        /// 开始执行发送
        /// </summary>
        void ISender.BeginSend()
        {
            Sender.BeginSend();
        }

        /// <summary>
        /// 数据发送<c>异步</c>
        /// </summary>
        /// <param name="message">大宋的消息</param>
        /// <returns>判断有没有发送出去</returns>
        public void Send(IMessageWriter message)
        {
            Sender.Send(message);
        }

        /// <summary>
        /// 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用
        /// </summary>
        /// <param name="message">发送的消息</param>
        [Obsolete("Only used in important message. This method will confuse the message queue. You can use 'Send' instead.", false)]
        public void SendImmediately(IMessageWriter message)
        {
            Sender.SendImmediately(message);
        }

        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析
        /// </summary>
        void ISender.SendBytes(byte[] data)
        {
            Sender.SendBytes(data);
        }

        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析，以及解析顺序
        /// </summary>
        void ISender.SendBytesImmediately(byte[] data)
        {
            Sender.SendBytesImmediately(data);
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

        /// <summary>
        /// 当消息处理完执行
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
        /// </summary>
        internal void MessageSolved(object sender, SocketAsyncEventArgs e)
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
    }
}
