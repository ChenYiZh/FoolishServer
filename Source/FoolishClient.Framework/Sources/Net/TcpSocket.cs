using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FoolishClient.Net
{
    /// <summary>
    /// Tcp连接池
    /// </summary>
    public class TcpSocket : ClientSocket
    {
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
