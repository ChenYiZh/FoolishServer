using System;
using System.Collections.Generic;
using System.Text;
using FoolishGames.IO;

namespace FoolishGames.Net
{
    /// <summary>
    /// 接收到的消息处理
    /// </summary>
    public struct MessageReceiverEventArgs : IMessageEventArgs<ISocket>
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
