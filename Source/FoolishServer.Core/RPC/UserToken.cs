using FoolishServer.Framework.RPC.Sockets;
using FoolishServer.Framework.RPC.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC
{
    /// <summary>
    /// 寄宿在原生Socket的管理类
    /// </summary>
    public class UserToken : IUserToken
    {
        /// <summary>
        /// 嵌套的Socket
        /// </summary>
        public ISocket Socket { get; internal set; }

        /// <summary>
        /// 套接字处理结果
        /// </summary>
        public ISocketAsyncResult AsyncResult { get; internal set; }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            AsyncResult = null;
        }
    }
}
