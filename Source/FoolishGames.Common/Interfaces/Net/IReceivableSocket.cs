using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息接收的处理套接字接口定义
    /// </summary>
    public interface IReceivableSocket : ISocket, IReceiver
    {
        /// <summary>
        /// 接收管理类
        /// </summary>
        IReceiver Receiver { get; }
    }
}
