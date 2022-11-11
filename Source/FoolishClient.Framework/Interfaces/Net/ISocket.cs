using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FoolishClient.Net
{
    public interface ISocket
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
        /// 内部套接字
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// 连接函数
        /// </summary>
        void Connect(string name, string host, int port);

        /// <summary>
        /// 数据发送接口
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data);

        /// <summary>
        /// 关闭函数
        /// </summary>
        void Close();
    }
}
