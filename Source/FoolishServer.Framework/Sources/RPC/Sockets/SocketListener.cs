using FoolishGames.Collections;
using FoolishServer.RPC;
using FoolishServer.RPC.Sockets;
using FoolishServer.Config;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FoolishServer.Delegate;
using FoolishGames.Log;
using FoolishServer.RPC.Tokens;
using FoolishGames.IO;
using FoolishGames.Common;
using FoolishGames.Security;
using FoolishGames.Timer;

namespace FoolishServer.RPC.Sockets
{
    public class SocketListener : IServerSocket
    {
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectionEventHandler OnConnected;
        private void Connected(ISocket remoteSocket) { OnConnected?.Invoke(this, remoteSocket); }

        /// <summary>
        /// 握手事件
        /// </summary>
        public event ConnectionEventHandler OnHandshaked;
        private void Handshaked(ISocket remoteSocket) { OnHandshaked?.Invoke(this, remoteSocket); }

        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ConnectionEventHandler OnDisconnected;
        private void Disconnected(ISocket remoteSocket) { OnDisconnected?.Invoke(this, remoteSocket); }

        /// <summary>
        /// 接收到数据包事件
        /// </summary>
        public event MessageEventHandler OnMessageReceived;
        private void MessageReceived(IMessageEventArgs args) { OnMessageReceived?.Invoke(this, args); }

        /// <summary>
        /// 心跳探索事件
        /// </summary>
        public event MessageEventHandler OnPing;
        private void Ping(IMessageEventArgs args) { OnPing?.Invoke(this, args); }

        /// <summary>
        /// 心跳回应事件
        /// </summary>
        public event MessageEventHandler OnPong;
        private void Pong(IMessageEventArgs args) { OnPong?.Invoke(this, args); }

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
        public string ServerName { get { return Setting.Name; } }

        /// <summary>
        /// 配置信息
        /// </summary>
        public IHostSetting Setting { get; private set; }

        /// <summary>
        /// 压缩工具
        /// </summary>
        public ICompression Compression { get; set; } = null;

        /// <summary>
        /// 加密工具
        /// </summary>
        public ICryptoProvider CryptoProvider { get; set; } = null;

        /// <summary>
        /// 类型
        /// </summary>
        public EServerType Type { get { return Setting.Type; } }

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

            //Test测试代码，发布前驱除
            //Compression = new GZipCompression();
            //CryptoProvide = new AESCryptoProvider("FoolishGames", "ChenYiZh");

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
                // 设置缓冲区大小
                ArrangeSocketBuffer(ioEventArgs);
                ioEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                UserToken userToken = new UserToken();
                ioEventArgs.UserToken = userToken;
                ioEventArgsPool.Push(ioEventArgs);
            }
            //并发锁初始化
            maxConnectionsEnforcer = new Semaphore(setting.MaxConnections, setting.MaxConnections);
            //生成套接字
            Address = new IPEndPoint(IPAddress.Any, Port);
            if (setting.Type != EServerType.Tcp)
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
            if (Type == EServerType.Tcp)
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
        /// 设置缓冲区大小
        /// </summary>
        /// <param name="ioEventArgs"></param>
        private void ArrangeSocketBuffer(SocketAsyncEventArgs ioEventArgs)
        {
            int bufferSize = Setting.BufferSize;
            ioEventArgs.SetBuffer(new byte[bufferSize], 0, bufferSize);
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
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "Post accept listen error:", e);
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
                        ArrangeSocketBuffer(ioEventArgs);
                        socket = new FSocket(ioEventArgs.AcceptSocket);
                        socket.AccessTime = TimeLord.Now;
                        userToken.Socket = socket;
                    }
                    acceptEventArgs.AcceptSocket = null;

                    //release connect when socket has be closed.
                    ReleaseAccept(acceptEventArgs, false);

                    // 连接后处理
                    try
                    {
                        Connected(socket);
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "OnConnected error:", e);
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
            IUserToken userToken = (IUserToken)ioEventArgs.UserToken;
            try
            {
                ((FSocket)userToken.Socket).AccessTime = TimeLord.Now;
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
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), $"IP {(userToken != null && userToken.Socket != null ? userToken.Socket.Address?.ToString() : "")} IOCompleted unkown error:", e);
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
            if (ioEventArgs.BytesTransferred == 0)
            {
                Close(ioEventArgs, EOpCode.Empty);
                return;
            }

            UserToken token = (UserToken)ioEventArgs.UserToken;

            if (ioEventArgs.SocketError != SocketError.Success)
            {
                FConsole.WriteErrorWithCategory(Setting.GetCategory(),
                    "Process Receive IP {0} SocketError:{1}, bytes len:{2}",
                    (token != null ? token.Socket.Address?.ToString() : ""),
                    ioEventArgs.SocketError.ToString(),
                    ioEventArgs.BytesTransferred);
                Close(ioEventArgs);
                return;
            }

            //Process Receive
            ISocket socket = token?.Socket;
            if (ioEventArgs.BytesTransferred > 0)
            {
                //从当前位置数据开始解析
                int offset = 0;
                //先缓存数据
                byte[] buffer = new byte[ioEventArgs.BytesTransferred];
                Buffer.BlockCopy(ioEventArgs.Buffer, ioEventArgs.Offset, buffer, 0, buffer.Length);

                //消息处理的队列
                List<IMessageReader> messages = new List<IMessageReader>();
                try
                {
                    //继续接收上次未接收完毕的数据
                    if (token.TempBuffer != null)
                    {
                        //上次连报头都没接收完
                        if (token.TempStartIndex < 0)
                        {
                            byte[] tBuffer = new byte[buffer.Length + token.TempBuffer.Length];
                            Buffer.BlockCopy(token.TempBuffer, 0, tBuffer, 0, token.TempBuffer.Length);
                            Buffer.BlockCopy(buffer, 0, tBuffer, token.TempBuffer.Length, buffer.Length);
                            buffer = tBuffer;
                            token.TempBuffer = null;
                        }
                        //数据仍然接收不完
                        else if (token.TempStartIndex + buffer.Length < token.TempBuffer.Length)
                        {
                            Buffer.BlockCopy(buffer, 0, token.TempBuffer, token.TempStartIndex, buffer.Length);
                            token.TempStartIndex += buffer.Length;
                            offset += buffer.Length;
                        }
                        //这轮数据可以接受完
                        else
                        {
                            int deltaLength = token.TempBuffer.Length - token.TempStartIndex;
                            Buffer.BlockCopy(buffer, 0, token.TempBuffer, token.TempStartIndex, deltaLength);
                            IMessageReader bigMessage = PackageFactory.Unpack(token.TempBuffer, Setting.Offset, Compression, CryptoProvider);
                            token.TempBuffer = null;
                            messages.Add(bigMessage);
                            offset += deltaLength;
                        }
                    }

                    //针对接收到的数据进行完整解析
                    while (offset < buffer.Length)
                    {
                        int totalLength = PackageFactory.GetTotalLength(buffer, offset + Setting.Offset);
                        //包头解析不全
                        if (totalLength < 0)
                        {
                            token.TempStartIndex = -1;
                            token.TempBuffer = new byte[buffer.Length - offset];
                            Buffer.BlockCopy(buffer, offset, token.TempBuffer, 0, token.TempBuffer.Length);
                            break;
                        }

                        //包体解析不全
                        if (totalLength > buffer.Length)
                        {
                            token.TempStartIndex = buffer.Length - offset;
                            token.TempBuffer = new byte[totalLength - offset];
                            Buffer.BlockCopy(buffer, offset, token.TempBuffer, 0, buffer.Length - offset);
                            break;
                        }

                        offset += Setting.Offset;
                        IMessageReader message = PackageFactory.Unpack(buffer, offset, Compression, CryptoProvider);
                        messages.Add(message);
                        offset = totalLength;
                    }
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "Process Receive error.", e);
                }

                for (int i = 0; i < messages.Count; i++)
                {
                    IMessageReader message = messages[i];
                    try
                    {
                        if (message.IsError)
                        {
                            FConsole.WriteErrorWithCategory(Categories.SOCKET, message.Error);
                            continue;
                        }
                        switch (message.OpCode)
                        {
                            case (sbyte)EOpCode.Close:
                                {
                                    // TODO: 检查关闭协议是否有效
                                    Close(ioEventArgs, EOpCode.Empty);
                                }
                                break;
                            case (sbyte)EOpCode.Ping:
                                {
                                    Ping(new MessageEventArgs { Socket = socket, Message = message });
                                }
                                break;
                            case (sbyte)EOpCode.Pong:
                                {
                                    Pong(new MessageEventArgs { Socket = socket, Message = message });
                                }
                                break;
                            default:
                                {
                                    MessageReceived(new MessageEventArgs { Socket = socket, Message = message });
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteExceptionWithCategory(Categories.FOOLISH_SERVER, "An exception occurred when resolve the message.", e);
                    }
                }
            }
            else if (token.TempBuffer != null)
            {
                //数据错乱
                token.TempBuffer = null;
            }

            PostReceive(ioEventArgs);

            //延迟关闭
            if (socket == null || !socket.IsRunning)
            {
                ResetSocketAsyncEventArgs(ioEventArgs);
            }
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

        internal protected void Close(SocketAsyncEventArgs ioEventArgs, EOpCode opCode = EOpCode.Close, string reason = "")
        {
            Interlocked.Decrement(ref summary.CurrentConnectCount);
            Interlocked.Increment(ref summary.CloseConnectCount);

            IUserToken token = (IUserToken)ioEventArgs.UserToken;
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
            if (ioEventArgs.AcceptSocket != null)
            {
                try
                {
                    ioEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
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
                Disconnected(token.Socket);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "OnDisconnected error:", e);
            }
            ResetSocketAsyncEventArgs(ioEventArgs);
            ReleaseIOEventArgs(ioEventArgs);
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
                FConsole.WriteWithCategory(Setting.GetCategory(),
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
