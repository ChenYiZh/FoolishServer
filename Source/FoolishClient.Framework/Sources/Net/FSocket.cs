using FoolishClient.Log;
using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FoolishClient.Net
{
    /// <summary>
    /// 套接字父类
    /// </summary>
    public abstract class FSocket : ISocket
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
            FConsole.WriteInfoWithCategory(Categories.FOOLISH_GAME, "{0} is starting...", GetType().Name);
            Name = name;
            Host = host;
            Port = port;
            Socket = MakeSocket();
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

        protected abstract Socket MakeSocket();

        /// <summary>
        /// 数据发送接口
        /// </summary>
        /// <param name="data"></param>
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
            FConsole.WriteInfoWithCategory(Categories.FOOLISH_GAME, "{0} Closed.", GetType().Name);
        }
    }
}
