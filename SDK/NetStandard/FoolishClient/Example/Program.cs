using Example.Log;
using FoolishClient.Net;
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
            socket.Connect("default", "127.0.0.1", 9001);
            socket.Send(Encoding.UTF8.GetBytes("Hello World!"));
            while (true)
            {
                Console.Read();
            }
        }
    }
}
