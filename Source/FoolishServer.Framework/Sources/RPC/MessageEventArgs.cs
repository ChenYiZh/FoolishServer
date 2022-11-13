using FoolishGames.IO;
using FoolishServer.RPC;
using FoolishServer.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC
{
    /// <summary>
    /// 消息发送
    /// </summary>
    public class MessageEventArgs : IMessageEventArgs
    {
        /// <summary>
        /// 发送的套接字
        /// </summary>
        public ISocket Socket { get; internal set; }

        /// <summary>
        /// 消息
        /// </summary>
        public IMessageReader Message { get; internal set; }
    }
}
