using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC.Sockets
{
    /// <summary>
    /// 自定义套接字
    /// </summary>
    public interface ISocket : ISocketMini
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        Guid HashCode { get; }

        /// <summary>
        /// 获取时间
        /// </summary>
        DateTime AccessTime { get; }

        ///// <summary>
        ///// 正在发送消息？
        ///// </summary>
        //bool IsSending { get; }

        /// <summary>
        /// Socket是否还连接着？
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 待发送的消息队列长度
        /// </summary>
        int MessageQueueCount { get; }

        /// <summary>
        /// 重置唯一id
        /// </summary>
        /// <param name="key"></param>
        void ResetHashset(Guid key);

        /// <summary>
        /// 重置发送标识
        /// </summary>
        void ResetSendFlag();

        /// <summary>
        /// 发送消息
        /// </summary>
        void Send(byte[] data, Action<ISocketAsyncResult> callback);

        /// <summary>
        /// 获取最早等待发送的消息
        /// </summary>
        /// <returns>是否有消息</returns>
        bool TryDequeueOrReset(out ISocketAsyncResult result);
    }
}
