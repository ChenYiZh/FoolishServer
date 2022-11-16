using FoolishServer.Model;
using FoolishServer.Runtime;
using System;

namespace FoolishServer
{
    class Program
    {
        static void Main(string[] args)
        {
            RuntimeHost.Startup();
            while (true)
            {
                Console.Read();
            }
        }
    }
}
