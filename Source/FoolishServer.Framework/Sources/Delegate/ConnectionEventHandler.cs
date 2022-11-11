using FoolishServer.RPC;
using FoolishServer.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Delegate
{
    /// <summary>
    /// 连接消息代理
    /// </summary>
    public delegate void ConnectionEventHandler(IServerSocket socket, IConnectionEventArgs e);
}
