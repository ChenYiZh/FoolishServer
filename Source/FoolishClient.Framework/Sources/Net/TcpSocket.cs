using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using FoolishGames.Net;

namespace FoolishClient.Net
{
    /// <summary>
    /// Tcp连接池
    /// </summary>
    public class TcpSocket : ClientSocket
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public TcpSocket() : base()
        {

        }

        /// <summary>
        /// 类型
        /// </summary>
        public override ESocketType Type { get { return ESocketType.Tcp; } }

        /// <summary>
        /// 建立原生套接字
        /// </summary>
        /// <returns></returns>
        protected override Socket MakeSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
