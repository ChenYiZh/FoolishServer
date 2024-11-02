using System.Net.Sockets;

namespace FoolishGames.Net
{
    public sealed class UdpClientSender : SocketSender
    {
        public UdpClientSender(ISocket socket) : base(socket)
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