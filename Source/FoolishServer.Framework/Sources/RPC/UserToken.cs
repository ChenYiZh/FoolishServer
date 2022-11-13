using FoolishServer.RPC.Sockets;
using FoolishServer.RPC.Tokens;
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
        /// 已经接收的数据长度
        /// </summary>
        internal int TempStartIndex { get; set; } = 0;

        /// <summary>
        /// 解析包时解析不完的数据
        /// </summary>
        internal byte[] TempBuffer { get; set; } = null;

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            TempBuffer = null;
            AsyncResult = null;
        }
    }
}
