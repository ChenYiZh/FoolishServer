using FoolishGames.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Net
{
    /// <summary>
    /// 消息处理类
    /// </summary>
    public interface IServerMessageProcessor
    {
        /// <summary>
        /// 接收到数据包事件
        /// </summary>
        void MessageReceived(IMessageEventArgs<IRemoteSocket> args);

        /// <summary>
        /// 心跳探索事件
        /// </summary>
        void Ping(IMessageEventArgs<IRemoteSocket> args);

        /// <summary>
        /// 心跳回应事件
        /// </summary>
        void Pong(IMessageEventArgs<IRemoteSocket> args);
    }
}
