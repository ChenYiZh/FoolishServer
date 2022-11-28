using FoolishGames.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishClient.Net
{
    /// <summary>
    /// 客户端套接字连接管理类
    /// </summary>
    public interface IClientSocket : ISendableSocket, IReceivableSocket, IMsgSocket
    {
    }
}
