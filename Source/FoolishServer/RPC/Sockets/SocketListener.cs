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

namespace FoolishServer.RPC.Sockets
{
    public class SocketListener : ISocket
    {
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
        /// 对象池
        /// </summary>
        private IThreadSafeStack<SocketAsyncEventArgs> acceptEventArgsPool;

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
            //连接对象池初始化
            acceptEventArgsPool = new ThreadSafeStack<SocketAsyncEventArgs>(setting.MaxAcceptCapacity);
            for (int i = 0; i < setting.MaxAcceptCapacity; i++)
            {
                acceptEventArgsPool.Push(CreateAcceptEventArgs());
            }
            //并发锁初始化
            maxConnectionsEnforcer = new Semaphore(setting.MaxConnections, setting.MaxConnections);
            //生成套接字
            Address = new IPEndPoint(IPAddress.Any, Port);
            if (setting.Type == EHostType.Tcp)
            {
                Socket = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            else if (setting.Type == EHostType.Udp)
            {
                Socket = new Socket(Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            }
            else
            {
                throw new InvalidCastException("The type of socket is error.");
            }
            //相同端口可以重复绑定
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //绑定端口
            Socket.Bind(Address);
            if (Type == EHostType.Tcp)
            {
                //监听
                Socket.Listen(setting.Backlog);
            }
            //服务器状态输出周期
            summaryTask = new Timer(WriteSummary, null, 0, 60000);
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

                    // TODO: 生成连接对象

                    //release connect when socket has be closed.
                    ReleaseAccept(acceptEventArgs, false);

                    // TODO: 连接后处理
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
