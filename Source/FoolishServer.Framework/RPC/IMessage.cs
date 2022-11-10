using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.RPC
{
    /// <summary>
    /// 消息接口
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 消息的字节流
        /// </summary>
        byte[] Buffer { get; }
    }
}
