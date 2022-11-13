using FoolishServer.RPC;
using FoolishServer.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

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
