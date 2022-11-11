using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC.Sockets
{
    /// <summary>
    /// 操作结果
    /// </summary>
    public enum EResultCode
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
    public interface ISocketAsyncResult
    {
        /// <summary>
        /// 内部关联的Socket
        /// </summary>
        ISocket Socket { get; }

        /// <summary>
        /// 需要发送的消息大小
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// 处理结果
        /// </summary>
        EResultCode Result { get; }

        /// <summary>
        /// 报错信息
        /// </summary>
        Exception Error { get; }

        /// <summary>
        /// 数据处理完成后的回调
        /// </summary>
        Action<ISocketAsyncResult> OnCallback { get; }

        /// <summary>
        /// 消息处理完成时执行
        /// </summary>
        void Execute();
    }
}
