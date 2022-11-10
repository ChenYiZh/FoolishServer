using FoolishGame.Framework.Net;
using FoolishGame.Log;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FoolishGame.Net
{
    public class TcpSocket : ISocket
    {
        /// <summary>
        /// 标识名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 内部套接字
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// 连接函数
        /// </summary>
        public void Connect(string name, string host, int port)
        {
            FConsole.WriteInfoWithCategory(Categories.FOOLISH_GAME, "TcpSocket is starting...");
            Name = name;
            Host = host;
            Port = port;
            Socket = CreateSocket();
            try
            {
                //IAsyncResult opt = Socket.BeginConnect(host, port, null, Socket);
                //bool success = opt.AsyncWaitHandle.WaitOne(1000, true);
                //if (!success || !opt.IsCompleted || !Socket.Connected)
                //{
                //    FConsole.WriteErrorWithCategory(Categories.FOOLISH_GAME, "Connect Failed!");
                //}
                Socket.Connect(host, port);//手机上测下来只有同步才有效
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.FOOLISH_GAME, e);
                FConsole.WriteErrorWithCategory(Categories.FOOLISH_GAME, "Fail to connect the server!");
                //Net.Instance.Connected = false;
                //EventManager.Instance.Send(Events.Disconnected);
                //socket.Dispose();
                //_socket = null;
                Close();
                throw;
            }
        }

        protected virtual Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Send(byte[] data)
        {
            FConsole.Write(Socket.Send(data).ToString());
        }

        /// <summary>
        /// 关闭函数
        /// </summary>
        public void Close()
        {
            if (Socket != null)
            {
                try
                {
                    Socket.Close();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.FOOLISH_GAME, e);
                }
                Socket = null;
            }
            FConsole.WriteInfoWithCategory(Categories.FOOLISH_GAME, "TcpSocket Closed.");
        }
    }
}
