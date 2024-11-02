using System.Net.Sockets;
using FoolishGames.Net;

namespace FoolishServer.Net
{
    public sealed class UdpServerSender : SocketSender
    {
        public UdpServerSender(ISocket socket) : base(socket)
        {
        }

        protected override void TrySendAsync(SocketAsyncEventArgs ioEventArgs)
        {
            if (!Socket.Socket.SendToAsync(ioEventArgs))
            {
                ProcessSend(ioEventArgs);
            }
        }
    }
}