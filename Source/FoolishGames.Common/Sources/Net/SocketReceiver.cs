using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息接收处理类
    /// </summary>
    public sealed class SocketReceiver : IReceiver
    {
        /// <summary>
        /// 封装的套接字
        /// </summary>
        public ISocket Socket { get; private set; }
        /// <summary>
        /// 增强类
        /// </summary>
        public SocketAsyncEventArgs EventArgs { get; private set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public SocketReceiver(ISocket socket)
        {
            Socket = socket;
        }
    }
}
