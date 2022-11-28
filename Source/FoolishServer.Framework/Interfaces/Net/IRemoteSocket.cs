using FoolishGames.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Net
{
    /// <summary>
    /// 自定义套接字
    /// </summary>
    public interface IRemoteSocket : ISocket, ISendableSocket, IReceivableSocket
    {
        /// <summary>
        /// 所属服务器
        /// </summary>
        IServerSocket Server { get; }

        /// <summary>
        /// 唯一id
        /// </summary>
        Guid HashCode { get; }

        /// <summary>
        /// 获取时间
        /// </summary>
        DateTime AccessTime { get; }

        /// <summary>
        /// 重置唯一id
        /// </summary>
        /// <param name="key"></param>
        void ResetHashset(Guid key);
    }
}
