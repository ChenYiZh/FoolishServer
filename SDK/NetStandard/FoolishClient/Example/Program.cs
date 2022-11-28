using Example.Log;
using FoolishClient.Net;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            FConsole.LogStackTracker = true;
            FConsole.RegistLogger(new Logger());
            TcpSocket socket = new TcpSocket();
            //socket.MessageOffset = 2;
            //socket.Compression = new GZipCompression();
            //socket.CryptoProvide = new AESCryptoProvider("FoolishGames", "ChenYiZh");
            socket.Ready("default", "127.0.0.1", 9001, "Example.Actions.Action{0}");
            MessageWriter message = new MessageWriter();
            //message.Secret = false;
            //message.Compress = false;
            message.MsgId = 1;
            message.OpCode = -1;
            message.ActionId = 1;
            message.WriteString("Hello World!");
            //Console.Read();
            socket.Send(message);
            Console.Read();
            socket.Send(message);
            socket.Send(message);
            socket.Send(message);
            while (true)
            {
                Console.ReadLine();
                if (socket.IsRunning)
                {
                    socket.Close();
                }
                else
                {
                    socket.Send(message);
                    socket.Send(message);
                    socket.Send(message);
                    socket.Send(message);
                }
            }
        }
    }
}
