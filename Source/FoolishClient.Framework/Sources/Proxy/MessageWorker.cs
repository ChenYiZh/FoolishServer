using FoolishClient.Net;
using FoolishGames.IO;
using FoolishGames.Proxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishClient.Proxy
{
    /// <summary>
    /// 消息处理的代理
    /// </summary>
    public class MessageWorker : IWorker
    {
        /// <summary>
        /// 消息处理窗口
        /// </summary>
        internal IMessageReader Message { get; set; }

        /// <summary>
        /// 服务器
        /// </summary>
        internal ClientSocket Socket { get; set; }

        /// <summary>
        /// 需要处理的工作
        /// </summary>
        public void Work()
        {
            Socket.ProcessMessage(Message);
        }
    }
}
