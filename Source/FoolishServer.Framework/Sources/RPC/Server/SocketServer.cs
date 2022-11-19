using FoolishServer.RPC;
using FoolishServer.RPC.Server;
using FoolishServer.RPC.Sockets;
using FoolishServer.Config;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Text;
using FoolishGames.Log;
using FoolishGames.IO;
using FoolishGames.Security;
using System.Threading.Tasks;
using FoolishGames.Proxy;
using FoolishServer.Proxy;
using System.Threading;
using FoolishServer.Action;

namespace FoolishServer.RPC.Server
{
    public abstract class SocketServer : IServer
    {
        /// <summary>
        /// 是否启动
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// 标识名称
        /// </summary>
        public string Name { get { return Setting.Name; } }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get { return Setting.Port; } }

        /// <summary>
        /// 类型
        /// </summary>
        public EServerType Type { get { return Setting.Type; } }

        /// <summary>
        /// 监听套接字
        /// </summary>
        public IServerSocket SocketListener { get; private set; }

        /// <summary>
        /// 压缩工具
        /// </summary>
        protected ICompression Compression
        {
            get { return SocketListener?.Compression; }
            set
            {
                if (SocketListener != null)
                {
                    SocketListener.Compression = value;
                }
            }
        }

        /// <summary>
        /// 加密工具
        /// </summary>
        protected ICryptoProvider CryptoProvider
        {
            get { return SocketListener?.CryptoProvider; }
            set
            {
                if (SocketListener != null)
                {
                    SocketListener.CryptoProvider = value;
                }
            }
        }

        /// <summary>
        /// 生成Action
        /// </summary>
        protected IServerActionDispatcher ActionProvider { get; set; } = new ServerActionDispatcher("FoolishServer.Action{0}");

        /// <summary>
        /// 消息处理的中转站
        /// </summary>
        protected ISupervisor MessageContractor { get; set; }

        /// <summary>
        /// 配置文件
        /// </summary>
        public IHostSetting Setting { get; private set; }

        /// <summary>
        /// 启动结构
        /// </summary>
        public bool Start(IHostSetting setting)
        {
            if (IsRunning) { return false; }
            Setting = setting;
            FConsole.WriteInfoFormatWithCategory(setting.GetCategory(), "Server is starting...", setting.Name);
            try
            {
                OnStart();
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "An error occurred on start.", e);
            }
            try
            {
                if (SocketListener != null)
                {
                    SocketListener.Close();
                }
                SocketListener = new SocketListener();
                SocketListener.OnConnected += OnSocketConnected;
                SocketListener.OnDisconnected += OnSocketDisconnected;
                SocketListener.OnPong += OnSocketReceiveHeartbeat;
                SocketListener.OnMessageReceived += ProcessMessage;
                SocketListener.Start(setting);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(setting.GetCategory(), e);
                return false;
            }
            IsRunning = true;
            return true;
        }

        protected virtual void OnStart()
        {

        }

        /// <summary>
        /// 消息处理
        /// </summary>
        private void ProcessMessage(IServerSocket socket, IMessageEventArgs args)
        {
            try
            {
                ISession session = GameSession.Get(args.Socket?.HashCode);
                if (session != null)
                {
                    OnReceiveMessage(session, args.Message);
                    if (MessageContractor != null)
                    {
                        MessageContractor.CheckIn(new MessageWorker { Session = session, Message = args.Message, Server = this });
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem((obj) => { ProcessMessage(session, args.Message); });
                    }
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "Process message error.", e);
            }
        }

        /// <summary>
        /// 开始接收数据，第一部处理
        /// </summary>
        protected virtual void OnReceiveMessage(ISession session, IMessageReader message)
        {
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        internal virtual void ProcessMessage(ISession session, IMessageReader message)
        {
            if (message == null)
            {
                FConsole.WriteErrorFormatWithCategory(Categories.SESSION, "{0} receive empty message.", session.SessionId);
                return;
            }
            try
            {
                ServerAction action = ActionProvider.Provide(message.ActionId);
                action.Session = session;
                try
                {
                    ActionBoss.Exploit(action, message.ActionId, message);
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.ACTION, "Action error.", e);
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.ACTION, "ActionProvider error.", e);
            }
        }

        /// <summary>
        /// Socket连接时执行
        /// </summary>
        private void OnSocketConnected(IServerSocket socket, ISocket remoteSocket)
        {
            try
            {
                GameSession session = (GameSession)GameSession.CreateNew(remoteSocket.HashCode, remoteSocket, socket);
                session.OnHeartbeatExpired += OnSessionHeartbeatExpired;
                OnSessionConnected(session);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
            }
        }

        /// <summary>
        /// 在客户端连接时执行
        /// </summary>
        protected virtual void OnSessionConnected(ISession session)
        {
            //在客户端连接时执行
            FConsole.WriteFormat("Hello {0}!", session.SessionId);
        }

        private void OnSocketDisconnected(IServerSocket socket, ISocket remoteSocket)
        {
            try
            {
                ISession session = GameSession.Get(remoteSocket.HashCode);
                if (session != null)
                {
                    try
                    {
                        OnSessionDisonnected(session);
                    }
                    catch (Exception ex)
                    {
                        FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "OnSessionDisonnected error.", ex);
                    }
                    session.Close();
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
            }
        }

        /// <summary>
        /// 在客户端断开时执行
        /// </summary>
        protected virtual void OnSessionDisonnected(ISession session)
        {
            //在客户端断开时执行
            FConsole.WriteFormat("Bye {0}!", session.SessionId);
        }

        private void OnSocketReceiveHeartbeat(IServerSocket socket, IMessageEventArgs args)
        {
            try
            {
                GameSession session = GameSession.Get(args.Socket?.HashCode) as GameSession;
                if (session != null)
                {
                    session.Refresh();
                    OnSessionHeartbeat(session);
                    // TODO: 是否要做心跳包回复
                }
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
            }
        }

        /// <summary>
        /// 收到客户端的心跳包时执行
        /// </summary>
        protected virtual void OnSessionHeartbeat(ISession session)
        {
            //收到客户端的心跳包时执行
            FConsole.WriteFormat("Beat {0}!", session.SessionId);
        }

        /// <summary>
        /// 在客户端心跳包过期时执行
        /// </summary>
        protected virtual void OnSessionHeartbeatExpired(ISession session)
        {
            //在客户端心跳包过期时执行
        }

        /// <summary>
        /// 在关闭前处理
        /// </summary>
        protected virtual void OnClose()
        {
            //在关闭前处理
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Shutdown()
        {
            try
            {
                OnClose();
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Setting.GetCategory(), "An error occurred on close.", e);
            }
            if (Setting == null)
            {
                return;
            }
            if (SocketListener != null)
            {
                try
                {
                    SocketListener.Close();
                    SocketListener = null;
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Setting.GetCategory(), e);
                }
            }
        }
    }
}
