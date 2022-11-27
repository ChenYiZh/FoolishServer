using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 操作结果
    /// </summary>
    internal enum EResultCode
    {
        /// <summary>
        /// 等待阶段
        /// </summary>
        Wait,
        /// <summary>
        /// 操作成功
        /// </summary>
        Success,
        /// <summary>
        /// Socket已经关闭
        /// </summary>
        Close,
        /// <summary>
        /// Socket 已报错
        /// </summary>
        Error
    }
    /// <summary>
    /// 套接字处理结果
    /// </summary>
    internal class SocketAsyncResult
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
        public Action<SocketAsyncResult> OnCallback { get; internal set; }

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
