﻿using FoolishGames.IO;
using FoolishServer.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FoolishServer.RPC
{
    /// <summary>
    /// Session心跳到期处理
    /// </summary>
    public delegate void OnSessionHeartbeatExpired(ISession session);

    /// <summary>
    /// 会话窗口
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// 标识符
        /// </summary>
        Guid KeyCode { get; }

        /// <summary>
        /// 会话窗口id
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// 绑定的UserId
        /// </summary>
        long UserId { get; }

        /// <summary>
        /// 远端地址
        /// </summary>
        IPEndPoint RemoteAddress { get; }

        /// <summary>
        /// 服务器名称
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// 自身的Socket
        /// </summary>
        IRemoteSocket Socket { get; }

        ///// <summary>
        ///// 当前的Session是否还有效
        ///// </summary>
        //bool IsValid { get; }

        /// <summary>
        /// 是否阻断当前Session
        /// </summary>
        bool Blocked { get; set; }

        /// <summary>
        /// 最近活跃时间
        /// </summary>
        DateTime ActiveTime { get; }

        /// <summary>
        /// 是否过期
        /// </summary>
        bool Expired { get; }

        /// <summary>
        /// 心跳过期
        /// </summary>
        bool HeartbeatExpired { get; }

        /// <summary>
        /// 是否已经关闭
        /// </summary>
        bool Closed { get; }

        /// <summary>
        /// 是否还连接着
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 关闭会话窗口
        /// </summary>
        void Close();

        /// <summary>
        /// 异步发送一条数据
        /// </summary>
        void Send(IMessageWriter message);
    }
}
