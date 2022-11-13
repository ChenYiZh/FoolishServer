using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC.Sockets
{
    /// <summary>
    /// 操作码
    /// </summary>
    public enum EOpCode : sbyte
    {
        /// <summary>
        /// 空数据
        /// </summary>
        Empty = -1,
        ///// <summary>
        ///// 连续性数据
        ///// </summary>
        //Continuation = 0,
        /// <summary>
        /// 文本数据
        /// </summary>
        Text = 1,
        /// <summary>
        /// 二进制数据
        /// </summary>
        Binary = 2,
        /// <summary>
        /// 关闭操作数据
        /// </summary>
        Close = 8,
        /// <summary>
        /// 服务器检测客户端是否存活的协议，暂时没有用到
        /// <para>原WebSocket中服务器向客户端发送的检测数据</para>
        /// </summary>
        Ping = 9,
        /// <summary>
        /// 客户端发送给服务器的心跳包
        /// <para>原WebSocket中客户端接收到服务器的Ping协议的回复</para>
        /// </summary>
        Pong = 10
    }
}
