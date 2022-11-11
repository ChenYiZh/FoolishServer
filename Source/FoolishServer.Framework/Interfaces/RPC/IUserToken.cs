using FoolishServer.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC.Tokens
{
    /// <summary>
    /// 寄宿在原生Socket的管理类
    /// </summary>
    public interface IUserToken
    {
        /// <summary>
        /// 嵌套的Socket
        /// </summary>
        ISocket Socket { get; }

        /// <summary>
        /// 套接字处理结果
        /// </summary>
        ISocketAsyncResult AsyncResult { get; }

        /// <summary>
        /// 重置数据
        /// </summary>
        void Reset();
    }
}
