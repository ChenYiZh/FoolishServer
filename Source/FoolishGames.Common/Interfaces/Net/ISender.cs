using FoolishGames.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 套接字消息发送的接口定义
    /// </summary>
    public interface ISender
    {
        /// <summary>
        /// 开始执行发送
        /// </summary>
        void BeginSend();
        /// <summary>
        /// 消息Id
        /// </summary>
        long MessageNumber { get; set; }
        /// <summary>
        /// 数据发送<c>异步</c>
        /// </summary>
        /// <param name="message">大宋的消息</param>
        /// <returns>判断有没有发送出去</returns>
        void Send(IMessageWriter message);
        /// <summary>
        /// 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用
        /// </summary>
        /// <param name="message">发送的消息</param>
        [Obsolete("Only used in important message. This method will confuse the message queue. You can use 'Send' instead.", false)]
        void SendImmediately(IMessageWriter message);
        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析
        /// </summary>
        void SendBytes(byte[] data);
        /// <summary>
        /// 内部函数，直接传bytes，会影响数据解析，以及解析顺序
        /// </summary>
        void SendBytesImmediately(byte[] data);
        /// <summary>
        /// 消息发送处理
        /// </summary>
        bool ProcessSend();
    }
}
