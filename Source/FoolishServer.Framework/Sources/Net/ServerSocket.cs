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
using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Net;
using FoolishGames.Security;
using FoolishGames.Timer;
using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FoolishServer.Net
{
    /// <summary>
    /// 连接消息代理
    /// </summary>
    public delegate void ConnectionEventHandler(IServerSocket socket, IRemoteSocket remoteSocket);
    /// <summary>
    /// 收发消息处理
    /// </summary>
    public delegate void MessageEventHandler(IServerSocket socket, IMessageEventArgs<IRemoteSocket> e);
    /// <summary>
    /// 服务器套接字管理
    /// </summary>
    public class ServerSocket : FSocket, IServerSocket, IMsgSocket, IServerMessageProcessor
    {
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectionEventHandler OnConnected;
        private void AfterConnected(IRemoteSocket remoteSocket) { OnConnected?.Invoke(this, remoteSocket); }

        /// <summary>
        /// 握手事件
        /// </summary>
        public event ConnectionEventHandler OnHandshaked;
        private void Handshaked(IRemoteSocket remoteSocket) { OnHandshaked?.Invoke(this, remoteSocket); }

        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ConnectionEventHandler OnDisconnected;
        private void Disconnected(IRemoteSocket remoteSocket) { OnDisconnected?.Invoke(this, remoteSocket); }

        /// <summary>
        /// 接收到数据包事件
        /// </summary>
        public event MessageEventHandler OnMessageReceived;
        void IServerMessageProcessor.MessageReceived(IMessageEventArgs<IRemoteSocket> args) { OnMessageReceived?.Invoke(this, args); }

        /// <summary>
        /// 心跳探索事件
        /// </summary>
        public event MessageEventHandler OnPing;
        void IServerMessageProcessor.Ping(IMessageEventArgs<IRemoteSocket> args) { OnPing?.Invoke(this, args); }

        /// <summary>
        /// 心跳回应事件
        /// </summary>
        public event MessageEventHandler OnPong;
        void IServerMessageProcessor.Pong(IMessageEventArgs<IRemoteSocket> args) { OnPong?.Invoke(this, args); }

        /// <summary>
        /// 内部关键原生Socket
        /// </summary>
        private Socket socket = null;

        /// <summary>
        /// 内部关键原生Socket
        /// </summary>
        public override Socket Socket { get { return socket; } }

        /// <summary>
        /// 封装的地址
        /// </summary>
        private IPEndPoint address = null;

        /// <summary>
        /// 封装的地址
        /// </summary>
        public override IPEndPoint Address { get { return address; } }

        /// <summary>
        /// 绑定的端口
        /// </summary>
        public int Port { get { return Setting.Port; } }

        /// <summary>
        /// 对应Host的名称
        /// </summary>
        public string ServerName { get { return Setting.Name; } }

        /// <summary>
        /// 配置信息
        /// </summary>
        public IHostSetting Setting { get; private set; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        private int messageOffset = 2;

        /// <summary>
        /// 消息偏移值
        /// </summary>
        public override int MessageOffset { get { return messageOffset; } }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        public void SetMessageOffset(int offset) { messageOffset = offset; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        private ICompression compression = null;

        /// <summary>
        /// 压缩工具
        /// </summary>
        public override ICompression Compression { get { return compression; } }

        /// <summary>
        /// 压缩工具
        /// </summary>
        public void SetCompression(ICompression compression) { this.compression = compression; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        private ICryptoProvider cryptoProvider = null;

        /// <summary>
        /// 加密工具
        /// </summary>
        public override ICryptoProvider CryptoProvider { get { return cryptoProvider; } }

        /// <summary>
        /// 加密工具
        /// </summary>
        public void SetCryptoProvide(ICryptoProvider cryptoProvider) { this.cryptoProvider = cryptoProvider; }

        /// <summary>
        /// 类型
        /// </summary>
        public override ESocketType Type { get { return Setting.Type; } }

        /// <summary>
        /// 状态类
        /// </summary>
        private SummaryStatus summary = new SummaryStatus();

        /// <summary>
        /// 输出统计的线程
        /// </summary>
        private Timer summaryTask;

        /// <summary>
        /// 并发管理锁
        /// </summary>
        private Semaphore maxConnectionsEnforcer;

        /// <summary>
        /// 接受连接并发对象池
        /// </summary>
        private ThreadSafeStack<SocketAsyncEventArgs> acceptEventArgsPool;

        /// <summary>
        /// 回复并发对象池
        /// </summary>
        private ThreadSafeStack<SocketAsyncEventArgs> ioEventArgsPool;

        /// <summary>
        /// 等待消息处理的缓存列表，主要用于单线程处理
        /// </summary>
        private ThreadSafeHashSet<RemoteSocket> Sockets = new ThreadSafeHashSet<RemoteSocket>();

        /// <summary>
        /// 生成的所有套接字管理对象都缓存在这里
        /// </summary>
        private ThreadSafeList<SocketAsyncEventArgs> AllEventArgsPool = new ThreadSafeList<SocketAsyncEventArgs>();

        /// <summary>
        /// 字节流池
        /// </summary>
        private BytePool BytePool { get; set; }

        /// <summary>
        /// 待处理的Socket的等待线程
        /// </summary>
        //private Timer WaitingSocketTimer;
        private Thread WaitingSocketThread;

        /// <summary>
        /// 初始化
        /// </summary>
        public ServerSocket() : base(null)
        {

        }

        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="setting"></param>
        public void Start(IHostSetting setting)
        {
            if (IsRunning) { return; }
            //默认参数赋值
            IsRunning = true;
            Setting = setting;
            BytePool = new BytePool(Setting.BufferSize, Setting.MaxIOCapacity);
            //Test测试代码，发布前驱除
            //Compression = new GZipCompression();
            //CryptoProvide = new AESCryptoProvider("FoolishGames", "ChenYiZh");

            //对象池初始化
            acceptEventArgsPool = new ThreadSafeStack<SocketAsyncEventArgs>();
            for (int i = 0; i < setting.MaxAcceptCapacity; i++)
            {
                acceptEventArgsPool.Push(CreateAcceptEventArgs());
            }
            ioEventArgsPool = new ThreadSafeStack<SocketAsyncEventArgs>();
            for (int i = 0; i < setting.MaxIOCapacity; i++)
            {
                ioEventArgsPool.Push(MakeIOEventArgs());
            }
            //并发锁初始化
            maxConnectionsEnforcer = new Semaphore(setting.MaxConnections, setting.MaxConnections);
            //生成套接字
            address = new IPEndPoint(IPAddress.Any, Port);
            if (setting.Type != ESocketType.Tcp)
            {
                socket = new Socket(Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            }
            else
            {
                socket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            //相同端口可以重复绑定
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //绑定端口
            Socket.Bind(Address);
            if (Type == ESocketType.Tcp)
            {
                //设置最大挂载连接数量
                Socket.Listen(setting.Backlog);
            }

            //服务器状态输出周期
            summaryTask = new Timer(WriteSummary, null, 60000, 60000);

            //待接收消息的套接字管理线程
            //int waitingInterval = 10;
            //WaitingSocketTimer = new Timer(ProcessWaiting, null, waitingInterval, waitingInterval);
            WaitingSocketThread = new Thread(() => { while (IsRunning) { ProcessWaiting(null); Thread.Sleep(1); } });
            WaitingSocketThread.Start();
            //启动监听
            PostAccept();
        }

        /// <summary>
        /// 开始接受连接
        /// </summary>
        protected void PostAccept()
        {
            try
            {
                if (!IsRunning) { return; }
                //对象池里拿结构
                SocketAsyncEventArgs acceptEventArgs = acceptEventArgsPool.Pop() ?? CreateAcceptEventArgs();
                if (Socket.ProtocolType == ProtocolType.Tcp)
                {
                    //https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.acceptasync?view=net-6.0
                    if (!Socket.AcceptAsync(acceptEventArgs))
                    {
                        ProcessAccept(acceptEventArgs);
                    }
                }
                else
                {
                    //acceptEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    //acceptEventArgs.AcceptSocket = Socket;
                    //https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.acceptasync?view=net-6.0
                    if (!Socket.AcceptAsync(acceptEventArgs))
                    {
                        ProcessAccept(acceptEventArgs);
                    }
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "Post accept listen error:", e);
            }
        }

        /// <summary>
        /// 处理接收到的连接
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        protected void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                Interlocked.Increment(ref summary.TotalConnectCount);
                maxConnectionsEnforcer.WaitOne();

                if (acceptEventArgs.SocketError != SocketError.Success)
                {
                    Interlocked.Increment(ref summary.RejectedConnectCount);
                    HandleBadAccept(acceptEventArgs);
                }
                else
                {
                    Interlocked.Increment(ref summary.CurrentConnectCount);

                    // 生成连接对象
                    SocketAsyncEventArgs ioEventArgs = null;
                    RemoteSocket socket = null;
                    if (ioEventArgsPool.Count > 0)
                    {
                        ioEventArgs = ioEventArgsPool.Pop();
                        ioEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
                        //IUserToken userToken = (IUserToken)ioEventArgs.UserToken;
                        //ArrangeSocketBuffer(ioEventArgs);
                        socket = new RemoteSocket(this, ioEventArgs);
                        socket.AccessTime = TimeLord.Now;
                        Sockets.Add(socket);
                        //userToken.Socket = socket;
                    }
                    acceptEventArgs.AcceptSocket = null;

                    //release connect when socket has be closed.
                    ReleaseAccept(acceptEventArgs, false);

                    // 连接后处理
                    try
                    {
                        AfterConnected(socket);
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "OnConnected error:", e);
                    }
                    socket.BeginReceive();
                    //PostReceive(ioEventArgs);
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
            }
            finally
            {
                PostAccept();
            }
        }

        /// <summary>
        /// 收到连接时需要做的事情
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="acceptEventArgs"></param>
        protected void AcceptCompleted(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "Accept Completed method error:", e);
                if (acceptEventArgs.AcceptSocket != null)
                {
                    try
                    {
                        acceptEventArgs.AcceptSocket.Close();
                    }
                    catch
                    {
                    }
                    acceptEventArgs.AcceptSocket = null;
                }
                ReleaseAccept(acceptEventArgs);
            }
        }

        /// <summary>
        /// 释放并发锁
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        /// <param name="isRelease"></param>
        private void ReleaseAccept(SocketAsyncEventArgs acceptEventArgs, bool isRelease = true)
        {
            acceptEventArgsPool.Push(acceptEventArgs);
            if (isRelease)
            {
                maxConnectionsEnforcer.Release();
            }
        }

        /// <summary>
        /// 消息接收处理
        /// </summary>
        private void IOCompleted(object sender, SocketAsyncEventArgs ioEventArgs)
        {
            try
            {
                IUserToken userToken = (IUserToken)ioEventArgs.UserToken;
                if (userToken == null) { return; }//TODO:不知道为什么会有空的情况
                RemoteSocket socket = (RemoteSocket)userToken.Socket;
                socket.AccessTime = TimeLord.Now;
                if (socket.MessageSolved(sender, ioEventArgs))
                {
                    Sockets.Add(socket);
                }
            }
            catch (ObjectDisposedException)
            {
                ReleaseIOEventArgs(ioEventArgs);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), $"IOCompleted unkown error:", e);
            }
        }

        /// <summary>
        /// 待接收的套接字处理线程
        /// </summary>
        private void ProcessWaiting(object state)
        {
            List<RemoteSocket> sockets;
            lock (Sockets.SyncRoot)
            {
                sockets = new List<RemoteSocket>(Sockets);
            }
            foreach (RemoteSocket socket in sockets)
            {
                if ((TimeLord.Now - socket.RefreshTime).TotalMilliseconds > 2 * Constants.HeartBeatsInterval)
                {
                    //心跳超时2倍，直接关闭
                    socket.Close();
                    continue;
                }
                ////不做这个处理会认为连接失败
                //if ((TimeLord.Now - socket.AccessTime).TotalSeconds > 1)
                //{
                socket.CheckSendOrReceive();
                //}
            }
        }

        /// <summary>
        /// 释放消息事件
        /// </summary>
        private void ReleaseIOEventArgs(SocketAsyncEventArgs ioEventArgs)
        {
            if (ioEventArgs == null) return;
            IUserToken userToken = (IUserToken)ioEventArgs.UserToken;
            if (userToken != null)
            {
                Sockets.Remove((RemoteSocket)userToken.Socket);
                userToken.Reset();
                //userToken.Socket = null;
            }
            ioEventArgs.AcceptSocket = null;
            ioEventArgsPool.Push(ioEventArgs);
        }

        /// <summary>
        /// 异常连接处理
        /// </summary>
        /// <param name="acceptEventArgs"></param>
        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ResetSocketAsyncEventArgs(acceptEventArgs);
                acceptEventArgs.AcceptSocket = null;
                ReleaseAccept(acceptEventArgs);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 重置Socket连接对象
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ResetSocketAsyncEventArgs(SocketAsyncEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.AcceptSocket != null)
                {
                    eventArgs.AcceptSocket.Close();
                }
            }
            catch (Exception)
            {
            }
            eventArgs.AcceptSocket = null;
        }

        /// <summary>
        /// 当远端连接关闭时，执行一些回收代码
        /// </summary>
        public virtual void OnRemoteSocketClosed(IRemoteSocket socket, EOpCode opCode)
        {
            Interlocked.Decrement(ref summary.CurrentConnectCount);
            Interlocked.Increment(ref summary.CloseConnectCount);

            Sockets.Remove((RemoteSocket)socket);

            IUserToken token = (IUserToken)socket.EventArgs.UserToken;
            if (opCode != EOpCode.Empty)
            {
                try
                {
                    // TODO: 处理关闭握手协议 CloseHandshake(dataToken.Socket, reason);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "Closing error:", e);
                }
            }
            if (socket.EventArgs.AcceptSocket != null)
            {
                try
                {
                    socket.EventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "Closing error:", e);
                }
            }
            try
            {
                maxConnectionsEnforcer.Release();
            }
            catch (Exception e)
            {
                string errorMessage = string.Format("Closed error, connect status: TotalCount={0}, CurrentCount={1}, CloseCount={2}, RejectedCount={3}",
                    summary.TotalConnectCount, summary.CurrentConnectCount, summary.CloseConnectCount, summary.RejectedConnectCount);
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), errorMessage, e);
            }

            try
            {
                Disconnected(socket);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "OnDisconnected error:", e);
            }
            ResetSocketAsyncEventArgs(socket.EventArgs);
            ReleaseIOEventArgs(socket.EventArgs);
        }

        /// <summary>
        /// 输出状态
        /// </summary>
        /// <param name="state"></param>
        private void WriteSummary(object state)
        {
            try
            {
                FConsole.WriteFormatWithCategory(Setting.GetCategory(),
              "Socket connect status: Total Count = {0}, Current Count = {1}, Closed Count = {2}, Rejected Count = {3}",
              summary.TotalConnectCount, summary.CurrentConnectCount, summary.CloseConnectCount, summary.RejectedConnectCount);
            }
            catch { }
        }

        /// <summary>
        /// 设置缓冲区大小
        /// </summary>
        private SocketAsyncEventArgs MakeIOEventArgs()
        {
            //int bufferSize = Setting.BufferSize;
            //ioEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
            byte[] buffer;
            int offset;
            int bufferSize;
            BytePool.GetByteBlock(out buffer, out offset, out bufferSize);
            SocketAsyncEventArgs ioEventArgs = MakeEventArgs(null, buffer, offset, bufferSize);
            ioEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
            AllEventArgsPool.Add(ioEventArgs);
            return ioEventArgs;
        }

        /// <summary>
        /// 创建待建立连接的套接字缓存类
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateAcceptEventArgs()
        {
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
            AllEventArgsPool.Add(acceptEventArg);
            return acceptEventArg;
        }

        /// <summary>
        /// 关闭操作
        /// </summary>
        public override void Close(EOpCode opCode = EOpCode.Close)
        {
            IsRunning = false;
            DisposeEventArgs();
            List<RemoteSocket> sockets = new List<RemoteSocket>();
            foreach (RemoteSocket socket in Sockets)
            {
                sockets.Add(socket);
            }
            foreach (RemoteSocket socket in sockets)
            {
                try
                {
                    socket.Close();
                }
                catch { }
            }
            Sockets.Clear();
            //if (WaitingSocketTimer != null)
            //{
            //    WaitingSocketTimer.Dispose();
            //    WaitingSocketTimer = null;
            //}
            if (WaitingSocketThread != null)
            {
                try
                {
                    WaitingSocketThread.Abort();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
                }
                WaitingSocketThread = null;
            }

            if (summaryTask != null)
            {
                try
                {
                    summaryTask.Dispose();
                }
                catch { }
                summaryTask = null;
            }

            base.Close();


            if (socket != null)
            {
                //try
                //{
                //    socket.Shutdown(SocketShutdown.Both);
                //}
                //catch (Exception e)
                //{
                //    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
                //}
                try
                {
                    socket.Close();
                    socket.Dispose();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
                }
                socket = null;
            }
            foreach (SocketAsyncEventArgs eventArgs in AllEventArgsPool)
            {
                eventArgs.Dispose();
            }
            AllEventArgsPool.Clear();
            acceptEventArgsPool.Clear();
            ioEventArgsPool.Clear();
        }
    }
}
