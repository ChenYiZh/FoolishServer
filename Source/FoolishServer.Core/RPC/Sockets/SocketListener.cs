using FoolishServer.Collections;
using FoolishServer.Framework.Collections;
using FoolishServer.Framework.RPC;
using FoolishServer.Framework.RPC.Sockets;
using FoolishServer.Framework.Config;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FoolishServer.Framework.Delegate;

namespace FoolishServer.RPC.Sockets
{
    public class SocketListener : IServerSocket
    {
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectionEventHandler OnConnected;
        private void Connected(IConnectionEventArgs e) { OnConnected?.Invoke(this, e); }

        /// <summary>
        /// 握手事件
        /// </summary>
        public event ConnectionEventHandler OnHandshaked;
        private void Handshaked(IConnectionEventArgs e) { OnHandshaked?.Invoke(this, e); }

        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ConnectionEventHandler OnDisconnected;
        private void Disconnected(IConnectionEventArgs e) { OnDisconnected?.Invoke(this, e); }

        /// <summary>
        /// 接收到数据包事件
        /// </summary>
        public event ConnectionEventHandler OnMessageReceived;
        private void MessageReceived(IConnectionEventArgs e) { OnMessageReceived?.Invoke(this, e); }

        /// <summary>
        /// 心跳探索事件
        /// </summary>
        public event ConnectionEventHandler OnPing;
        private void Ping(IConnectionEventArgs e) { OnPing?.Invoke(this, e); }

        /// <summary>
        /// 心跳回应事件
        /// </summary>
        public event ConnectionEventHandler OnPong;
        private void Pong(IConnectionEventArgs e) { OnPong?.Invoke(this, e); }

        /// <summary>
        /// 是否在运行
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// 内部关键原生Socket
        /// </summary>
        public Socket Socket { get; private set; } = null;

        /// <summary>
        /// 封装的地址
        /// </summary>
        public IPEndPoint Address { get; private set; } = null;

        /// <summary>
        /// 绑定的端口
        /// </summary>
        public int Port { get { return Setting.Port; } }

        /// <summary>
        /// 对应Host的名称
        /// </summary>
        public string HostName { get { return Setting.Name; } }

        /// <summary>
        /// 配置信息
        /// </summary>
        public IHostSetting Setting { get; private set; }

        /// <summary>
        /// 类型
        /// </summary>
        public EHostType Type { get { return Setting.Type; } }

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
        private IThreadSafeStack<SocketAsyncEventArgs> acceptEventArgsPool;

        /// <summary>
        /// 回复并发对象池
        /// </summary>
        private IThreadSafeStack<SocketAsyncEventArgs> ioEventArgsPool;

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
            //对象池初始化
            acceptEventArgsPool = new ThreadSafeStack<SocketAsyncEventArgs>(setting.MaxAcceptCapacity);
            for (int i = 0; i < setting.MaxAcceptCapacity; i++)
            {
                acceptEventArgsPool.Push(CreateAcceptEventArgs());
            }
            ioEventArgsPool = new ThreadSafeStack<SocketAsyncEventArgs>(setting.MaxIOCapacity);
            for (int i = 0; i < setting.MaxIOCapacity; i++)
            {
                SocketAsyncEventArgs ioEventArgs = new SocketAsyncEventArgs();
                // TODO: 默认消息Buffer
                ioEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                UserToken userToken = new UserToken();
                ioEventArgs.UserToken = userToken;
                ioEventArgsPool.Push(ioEventArgs);
            }
            //并发锁初始化
            maxConnectionsEnforcer = new Semaphore(setting.MaxConnections, setting.MaxConnections);
            //生成套接字
            Address = new IPEndPoint(IPAddress.Any, Port);
            if (setting.Type != EHostType.Tcp)
            {
                Socket = new Socket(Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            }
            else
            {
                Socket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            //相同端口可以重复绑定
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //绑定端口
            Socket.Bind(Address);
            if (Type == EHostType.Tcp)
            {
                //设置最大挂载连接数量
                Socket.Listen(setting.Backlog);
            }
            //服务器状态输出周期
            summaryTask = new Timer(WriteSummary, null, 60000, 60000);

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
                //https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.acceptasync?view=net-6.0
                if (!Socket.AcceptAsync(acceptEventArgs))
                {
                    ProcessAccept(acceptEventArgs);
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
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
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
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
                    FSocket socket = null;
                    if (ioEventArgsPool.Count > 0)
                    {
                        ioEventArgs = ioEventArgsPool.Pop();
                        ioEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
                        UserToken userToken = (UserToken)ioEventArgs.UserToken;
                        ioEventArgs.SetBuffer(0, Setting.BufferSize);
                        socket = new FSocket(ioEventArgs.AcceptSocket);
                        socket.AccessTime = DateTime.Now;
                        userToken.Socket = socket;
                    }
                    acceptEventArgs.AcceptSocket = null;

                    //release connect when socket has be closed.
                    ReleaseAccept(acceptEventArgs, false);

                    // 连接后处理
                    try
                    {
                        Connected(new SocketConnectionEventArgs() { Socket = socket });
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
                    }
                    PostReceive(ioEventArgs);
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
            UserToken userToken = (UserToken)ioEventArgs.UserToken;
            try
            {
                ((FSocket)userToken.Socket).AccessTime = DateTime.Now;
                switch (ioEventArgs.LastOperation)
                {
                    case SocketAsyncOperation.Receive: ProcessReceive(ioEventArgs); break;
                    case SocketAsyncOperation.Send: ProcessSend(ioEventArgs); break;
                    default: throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (ObjectDisposedException)
            {
                ReleaseIOEventArgs(ioEventArgs);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
            }
        }

        /// <summary>
        /// 释放消息事件
        /// </summary>
        private void ReleaseIOEventArgs(SocketAsyncEventArgs ioEventArgs)
        {
            if (ioEventArgs == null) return;
            UserToken userToken = (UserToken)ioEventArgs.UserToken;
            if (userToken != null)
            {
                userToken.Reset();
                userToken.Socket = null;
            }
            ioEventArgs.AcceptSocket = null;
            ioEventArgsPool.Push(ioEventArgs);
        }

        /// <summary>
        /// 投递接收数据请求
        /// </summary>
        /// <param name="ioEventArgs"></param>
        private void PostReceive(SocketAsyncEventArgs ioEventArgs)
        {
            if (ioEventArgs.AcceptSocket == null) return;

            //https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.receiveasync?view=net-6.0#system-net-sockets-socket-receiveasync(system-net-sockets-socketasynceventargs)
            if (!ioEventArgs.AcceptSocket.ReceiveAsync(ioEventArgs))
            {
                ProcessReceive(ioEventArgs);
            }
        }

        /// <summary>
        /// 处理数据接收回调
        /// </summary>
        /// <param name="ioEventArgs"></param>
        private void ProcessReceive(SocketAsyncEventArgs ioEventArgs)
        {
            FConsole.Write("Process Receive...");
            // TODO: Process Receive
        }

        /// <summary>
        /// 发送接口
        /// </summary>
        /// <param name="ioEventArgs"></param>
        private void ProcessSend(SocketAsyncEventArgs ioEventArgs)
        {
            FConsole.Write("Process Send...");
            // TODO: Process Send
        }
        /// <summary>
        /// 创建连接代理
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateAcceptEventArgs()
        {
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
            return acceptEventArg;
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
        /// 输出状态
        /// </summary>
        /// <param name="state"></param>
        private void WriteSummary(object state)
        {
            try
            {
                FConsole.WriteInfoWithCategory(Setting.GetCategory(),
              "Socket connect status: Total Count = {0}, Current Count = {1}, Closed Count = {2}, Rejected Count = {3}",
              summary.TotalConnectCount, summary.CurrentConnectCount, summary.CloseConnectCount, summary.RejectedConnectCount);
            }
            catch { }
        }

        /// <summary>
        /// 关闭操作
        /// </summary>
        public void Close()
        {
            IsRunning = false;
            if (summaryTask != null)
            {
                summaryTask.Dispose();
                summaryTask = null;
            }
            if (Socket != null)
            {
                try
                {
                    Socket.Close();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
                }
                Socket = null;
            }
        }
    }
}
