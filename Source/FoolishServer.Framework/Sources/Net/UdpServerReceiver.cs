using System.Net.Sockets;
using FoolishServer.Net;

namespace FoolishGames.Net
{
    public sealed class UdpServerReceiver : ServerReceiver
    {
        /// <summary>
        /// 投递接收数据请求
        /// </summary>
        /// <param name="ioEventArgs"></param>
        public override void PostReceive(SocketAsyncEventArgs ioEventArgs)
        {
            if (!Socket.TryReceive())
            {
                return;
            }

            if (!Socket.Socket.ReceiveFromAsync(ioEventArgs))
            {
                ProcessReceive(ioEventArgs);
            }
        }

        /// <summary>
        /// 关闭操作
        /// </summary>
        protected override void Close(SocketAsyncEventArgs ioEventArgs, EOpCode opCode)
        {
            base.Close(ioEventArgs, opCode);
            if (ioEventArgs != null)
            {
                ioEventArgs.AcceptSocket = null;
            }
        }

        public UdpServerReceiver(ServerSocket serverSocket) : base(serverSocket)
        {
        }
    }
}