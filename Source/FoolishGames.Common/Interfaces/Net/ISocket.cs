using FoolishGames.IO;
using FoolishGames.Proxy;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 套接字扩充接口
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// 是否在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Socket是否还连接着？
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 封装的地址
        /// </summary>
        IPEndPoint Address { get; }

        /// <summary>
        /// 原生套接字
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// 套接字增强类
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs</para>
        /// </summary>
        SocketAsyncEventArgs EventArgs { get; }

        /// <summary>
        /// 获取类型
        /// </summary>
        ESocketType Type { get; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        int MessageOffset { get; }

        /// <summary>
        /// 压缩工具
        /// </summary>
        ICompression Compression { get; }

        /// <summary>
        /// 加密工具
        /// </summary>
        ICryptoProvider CryptoProvider { get; }

        /// <summary>
        /// 消息处理方案
        /// </summary>
        IBoss MessageEventProcessor { get; }

        /// <summary>
        /// 关闭函数
        /// </summary>
        void Close(EOpCode opCode = EOpCode.Close);
    }
}
