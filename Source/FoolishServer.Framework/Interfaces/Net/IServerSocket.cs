using FoolishGames.IO;
using FoolishGames.Net;
using FoolishGames.Security;
using FoolishServer.Config;
using FoolishServer.Delegate;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FoolishServer.Net
{
    /// <summary>
    /// 服务器套接字接口定义
    /// </summary>
    public interface IServerSocket : ISocket, IMsgSocket
    {
        /// <summary>
        /// 对应Host的名称
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// 绑定的端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        IHostSetting Setting { get; }

        /// <summary>
        /// 连接事件
        /// </summary>
        event ConnectionEventHandler OnConnected;

        /// <summary>
        /// 握手事件
        /// </summary>
        event ConnectionEventHandler OnHandshaked;

        /// <summary>
        /// 断开连接事件
        /// </summary>
        event ConnectionEventHandler OnDisconnected;

        /// <summary>
        /// 接收到数据包事件
        /// </summary>
        event MessageEventHandler OnMessageReceived;

        /// <summary>
        /// 心跳探索事件
        /// </summary>
        event MessageEventHandler OnPing;

        /// <summary>
        /// 心跳回应事件
        /// </summary>
        event MessageEventHandler OnPong;

        /// <summary>
        /// 启动函数
        /// </summary>
        void Start(IHostSetting setting);

        /// <summary>
        /// 消息发送，
        /// 会影响到客户端解析
        /// </summary>
        void PostAsync(ISocket socket, byte[] buffer);
    }
}
