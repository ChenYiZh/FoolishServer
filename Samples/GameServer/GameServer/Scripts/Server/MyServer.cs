using FoolishGames.Log;
using FoolishServer.Net;
using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Server
{
    public class MyServer : TcpServer
    {
        protected override void OnSessionConnected(ISession session)
        {
            base.OnSessionConnected(session);
            FConsole.WriteFormat("MyServer Hello {0}!", session.SessionId);
        }

        protected override void OnSessionDisonnected(ISession session)
        {
            base.OnSessionDisonnected(session);
            FConsole.WriteFormat("MyServer Bye {0}!", session.SessionId);
        }

        protected override void OnSessionHeartbeat(ISession session)
        {
            base.OnSessionHeartbeat(session);
            FConsole.WriteFormat("MyServer Beat {0}!", session.SessionId);
        }
    }
}
