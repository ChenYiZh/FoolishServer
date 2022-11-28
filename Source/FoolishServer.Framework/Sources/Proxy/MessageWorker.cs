using FoolishGames.IO;
using FoolishGames.Proxy;
using FoolishServer.Net;
using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Proxy
{
    /// <summary>
    /// 消息处理的代理
    /// </summary>
    public struct MessageWorker : IWorker
    {
        /// <summary>
        /// 会话窗口
        /// </summary>
        internal ISession Session { get; set; }

        /// <summary>
        /// 消息处理窗口
        /// </summary>
        internal IMessageReader Message { get; set; }

        /// <summary>
        /// 服务器
        /// </summary>
        internal SocketServer Server { get; set; }

        /// <summary>
        /// 需要处理的工作
        /// </summary>
        public void Work()
        {
            Server.ProcessMessage(Session, Message);
        }
    }
}
