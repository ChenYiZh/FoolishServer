using System.Net.Sockets;

namespace FoolishGames.Net
{
    public sealed class TcpClientSender : SocketSender
    {
        public TcpClientSender(ISocket socket) : base(socket)
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