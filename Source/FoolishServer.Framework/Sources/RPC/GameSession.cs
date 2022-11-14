﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using FoolishGames.Log;
using FoolishGames.Timer;
using FoolishServer.Log;
using FoolishServer.RPC.Sockets;

namespace FoolishServer.RPC
{
    /// <summary>
    /// 会话窗口
    /// </summary>
    public partial class GameSession : ISession
    {
        /// <summary>
        /// 标识符
        /// </summary>
        public Guid KeyCode { get; private set; }

        /// <summary>
        /// 会话窗口id
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        /// 绑定的UserId
        /// </summary>
        public long UserId { get; private set; }

        /// <summary>
        /// 远端地址
        /// </summary>
        public IPEndPoint RemoteAddress { get { return Socket?.Address; } }

        /// <summary>
        /// 自身的Socket
        /// </summary>
        public ISocket Socket { get; private set; }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get { return Server?.ServerName; } }

        /// <summary>
        /// 所属服务器
        /// </summary>
        private IServerSocket Server { get; set; }

        /// <summary>
        /// 当前的Session是否还有效
        /// </summary>
        public bool IsValid { get { return Connected && !Closed && !Expired && !HeartbeatExpired; } }

        /// <summary>
        /// 是否阻断当前Session
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// 最近活跃时间
        /// </summary>
        public DateTime ActiveTime { get; private set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool Expired { get; private set; }

        /// <summary>
        /// 心跳过期
        /// </summary>
        public bool HeartbeatExpired { get; private set; }

        /// <summary>
        /// 是否已经关闭
        /// </summary>
        public bool Closed { get { return Socket == null || !Socket.IsRunning; } }

        /// <summary>
        /// 是否还连接着
        /// </summary>
        public bool Connected { get { return Socket != null && Socket.Socket != null && Socket.Socket.Connected; } }

        /// <summary>
        /// 心跳到期
        /// </summary>
        internal event OnSessionHeartbeatExpired OnHeartbeatExpired;

        /// <summary>
        /// 内部构造函数
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name=""></param>
        private GameSession(Guid keyCode, ISocket socket, IServerSocket server)
        {
            KeyCode = keyCode;
            Socket = socket;
            Server = server;
            SessionId = GenerateSessionId();
        }

        /// <summary>
        /// 生成会话窗口名称
        /// </summary>
        /// <returns></returns>
        private string GenerateSessionId()
        {
            return string.Format("{0}_{1}", ServerName, KeyCode.ToString("N"));
        }

        /// <summary>
        /// 心跳到期
        /// </summary>
        private void HeartbeatTimeout()
        {
            try
            {
                HeartbeatExpired = true;
                OnHeartbeatExpired?.Invoke(this);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.SESSION, "Heartbeat expired process error.", e);
            }
        }

        /// <summary>
        /// 刷新信息
        /// </summary>
        internal void Refresh()
        {
            Expired = false;
            HeartbeatExpired = false;
            ActiveTime = TimeLord.Now;
        }

        /// <summary>
        /// 关闭Session
        /// </summary>
        public void Close()
        {
            ISession session;
            if (sessions.TryGetValue(KeyCode, out session) && session.Socket != null)
            {
                //设置Socket为Closed的状态, 并未将物理连接马上中断
                ((FSocket)session.Socket).IsRunning = false;
            }
        }
    }
}
