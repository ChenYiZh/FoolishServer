using FoolishServer.Framework.RPC;
using FoolishServer.Framework.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.Delegate
{
    /// <summary>
    /// 连接消息代理
    /// </summary>
    public delegate void ConnectionEventHandler(IServerSocket socket, IConnectionEventArgs e);
}
