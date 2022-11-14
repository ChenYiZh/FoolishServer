using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FoolishServer.RPC.Sockets
{
    public interface ISocketMini
    {
        /// <summary>
        /// 是否在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 封装的地址
        /// </summary>
        IPEndPoint Address { get; }

        /// <summary>
        /// 内部关键原生Socket
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// 获取类型
        /// </summary>
        EServerType Type { get; }

        /// <summary>
        /// 关闭函数
        /// </summary>
        void Close();
    }
}
