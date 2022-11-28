using FoolishGames.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Net
{
    /// <summary>
    /// 消息发送
    /// </summary>
    public class MessageEventArgs : IMessageEventArgs
    {
        /// <summary>
        /// 发送的套接字
        /// </summary>
        public IRemoteSocket Socket { get; internal set; }

        /// <summary>
        /// 消息
        /// </summary>
        public IMessageReader Message { get; internal set; }
    }
}
