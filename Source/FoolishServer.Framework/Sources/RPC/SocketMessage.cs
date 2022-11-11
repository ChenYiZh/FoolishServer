using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC
{
    public class SocketMessage : IMessage
    {
        public byte[] Buffer { get; internal set; }
    }
}
