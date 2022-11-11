using FoolishServer.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC
{
    /// <summary>
    /// 连接消息处理
    /// </summary>
    public interface IConnectionEventArgs
    {
        /// <summary>
        /// 获取封装的套接字
        /// </summary>
        ISocket Socket { get; }

        /// <summary>
        /// 获取消息
        /// </summary>
        IMessage Message { get; }

        /// <summary>
        /// 消息的字节流
        /// </summary>
        byte[] Buffer { get; }
    }
}
