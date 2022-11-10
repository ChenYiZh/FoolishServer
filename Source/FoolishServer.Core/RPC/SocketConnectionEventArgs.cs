using FoolishServer.Framework.RPC;
using FoolishServer.Framework.RPC.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC
{
    public class SocketConnectionEventArgs : IConnectionEventArgs
    {
        public ISocket Socket { get; internal set; }

        public IMessage Message { get; private set; }

        public byte[] Buffer
        {
            get { return Message == null ? null : Message.Buffer; }
            internal set
            {
                ((SocketMessage)(Message ?? new SocketMessage())).Buffer = value;
            }
        }
    }
}
