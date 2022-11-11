using FoolishServer.RPC.Sockets;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Text;
using FoolishGames.Log;

namespace FoolishServer.RPC.Sockets
{
    /// <summary>
    /// 套接字处理结果
    /// </summary>
    internal class SocketAsyncResult : ISocketAsyncResult
    {
        /// <summary>
        /// 内部关联的Socket
        /// </summary>
        public ISocket Socket { get; internal set; }

        /// <summary>
        /// 需要发送的消息大小
        /// </summary>
        public byte[] Buffer { get; internal set; }

        /// <summary>
        /// 处理结果
        /// </summary>
        public EResultCode Result { get; internal set; }

        /// <summary>
        /// 报错信息
        /// </summary>
        public Exception Error { get; internal set; } = null;

        /// <summary>
        /// 数据处理完成后的回调
        /// </summary>
        public Action<ISocketAsyncResult> OnCallback { get; internal set; }

        /// <summary>
        /// 消息处理完成时执行
        /// </summary>
        public void Execute()
        {
            try
            {
                OnCallback?.Invoke(this);
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.SOCKET, e);
            }
        }
    }
}
