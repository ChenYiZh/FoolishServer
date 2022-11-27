using FoolishGames.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 带有发送函数的套接字接口定义
    /// </summary>
    public interface ISendableSocket : ISocket, ISender
    {
        /// <summary>
        /// 发送的管理类
        /// </summary>
        ISender Sender { get; }
    }
}
