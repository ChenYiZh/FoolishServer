using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.Runtime
{
    public enum EHostType
    {
        Http, WebSocket, Tcp, Udp
    }
    public interface IRuntimeHost
    {
        bool IsRunning { get; }

        EHostType Type { get; }

        bool Start();

        bool Stop();
    }
}
