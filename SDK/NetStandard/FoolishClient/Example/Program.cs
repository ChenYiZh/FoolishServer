using Example.Log;
using FoolishClient.Net;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
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
            FConsole.RegistLogger(new Logger());
            TcpSocket socket = new TcpSocket();
            socket.Ready("default", "127.0.0.1", 9001);
            MessageWriter message = new MessageWriter();
            message.MsgId = 132;
            message.WriteString("Hello World!");
            byte[] buffer = PackageFactory.Pack(message, 3, true);
            Console.WriteLine(buffer.Length);
            socket.SendAsync(buffer);
            while (true)
            {
                Console.Read();
            }
        }
    }
}
