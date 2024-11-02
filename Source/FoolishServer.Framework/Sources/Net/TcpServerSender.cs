using System.Net.Sockets;
using FoolishGames.Net;

namespace FoolishServer.Net
{
    public sealed class TcpServerSender : SocketSender
    {
        public TcpServerSender(ISocket socket) : base(socket)
        {
        }

        protected override void TrySendAsync(SocketAsyncEventArgs ioEventArgs)
        {
            if (!ioEventArgs.AcceptSocket.SendAsync(ioEventArgs))
            {
                ProcessSend(ioEventArgs);
            }
        }
    }
}