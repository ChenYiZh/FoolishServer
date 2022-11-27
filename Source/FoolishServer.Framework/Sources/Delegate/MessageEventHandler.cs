using FoolishServer.Net;
using FoolishServer.RPC;
using FoolishServer.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Delegate
{
    /// <summary>
    /// 收发消息处理
    /// </summary>
    public delegate void MessageEventHandler(IServerSocket socket, IMessageEventArgs e);
}
