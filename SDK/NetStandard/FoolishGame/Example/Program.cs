using Example.Log;
using FoolishGame.Log;
using FoolishGame.Net;
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
            new TcpSocket().Connect("default", "127.0.0.1", 9001);
            while (true)
            {
                Console.Read();
            }
        }
    }
}
