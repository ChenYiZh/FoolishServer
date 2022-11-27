using FoolishClient.Delegate;
using FoolishGames.IO;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FoolishClient.Net
{
    public interface ISocketsss
    {
        /// <summary>
        /// 标识名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        string Host { get; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 数据是否已经初始化了
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// 是否已经开始工作
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 是否还连接着
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 心跳包间隔
        /// </summary>
        int HeartbeatInterval { get; }

        /// <summary>
        /// 内部套接字
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// 消息偏移值
        /// </summary>
        int MessageOffset { get; }

        /// <summary>
        /// 压缩工具
        /// </summary>
        ICompression Compression { get; set; }

        /// <summary>
        /// 加密工具
        /// </summary>
        ICryptoProvider CryptoProvider { get; set; }

        /// <summary>
        /// 初始化信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="heartbeatInterval"></param>
        void Ready(string name, string host, int port, int heartbeatInterval = 10000);

        /// <summary>
        /// 连接函数
        /// </summary>
        void ConnectAsync(Action<bool> callback = null);

        /// <summary>
        /// 重新构建心跳包数据
        /// </summary>
        void RebuildHeartbeatPackage();

        /// <summary>
        /// 数据发送<c>异步</c>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback">发送回调</param>
        /// <returns>判断有没有发送出去</returns>
        void SendAsync(IMessageWriter message, SendCallback callback = null);

        /// <summary>
        /// 关闭函数
        /// </summary>
        void Close();
    }
}
