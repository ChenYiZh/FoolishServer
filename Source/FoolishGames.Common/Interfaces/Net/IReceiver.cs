using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 消息接收处理类
    /// </summary>
    public interface IReceiver
    {
        /// <summary>
        /// 等待消息接收
        /// </summary>
        void BeginReceive();
        /// <summary>
        /// 处理数据接收回调
        /// </summary>
        bool ProcessReceive();
    }
}
