/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/
using Example.Log;
using FoolishClient.Net;
using FoolishClient.RPC;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            FConsole.LogStackTracker = true;
            //FConsole.LogStackLevels.Add("Debug");
            FConsole.LogStackLevels.Add("Warn");
            FConsole.RegistLogger(new Logger());

            IClientSocket tcpSocket = FNetwork.MakeTcpSocket("default", "127.0.0.1", 9001, "Example.Actions.Action{0}");
            IClientSocket udpSocket = FNetwork.MakeUdpSocket("udpSocket", "127.0.0.1", 9002, "Example.Actions.Action{0}");
            //socket.MessageOffset = 2;
            //socket.Compression = new GZipCompression();
            //socket.CryptoProvide = new AESCryptoProvider("FoolishGames", "ChenYiZh");

            //((UdpSocket)socket).ConnectAsync();
            //Console.ReadLine();
            //return;

            MessageWriter message = new MessageWriter();
            //message.Secret = false;
            //message.Compress = false;
            message.WriteString("Hello World!");
            //Console.Read();
            //FNetwork.Send(1000, message);
            //Console.Read();
            //FNetwork.Send(1000, message);
            //FNetwork.Send(1000, message);
            //FNetwork.Send(1000, message);

            FNetwork.Send("default", 1000, message);
            FNetwork.Send("udpSocket", 1000, message);

            //ThreadPool.UnsafeQueueUserWorkItem((stat) =>
            //{
            //    for (int i = 0; i < 10000; i++)
            //    {
            //        FNetwork.Send("default", 1000, message);
            //        FNetwork.Send("udpSocket", 1000, message);
            //        Thread.Sleep(1);
            //    }
            //}, null);

            //for (int i = 0; i < 100; i++)
            //{
            //    int ping = FNetwork.GetRoundtripTime();
            //    FConsole.Write(ping);
            //    Thread.Sleep(100);
            //}

            Console.Read();
            //while (true)
            //{
            //    Console.ReadLine();
            //    if (socket.IsRunning)
            //    {
            //        socket.Close();
            //    }
            //    else
            //    {
            //        FNetwork.Send(1000, message);
            //        FNetwork.Send(1000, message);
            //        FNetwork.Send(1000, message);
            //        FNetwork.Send(1000, message);
            //    }
            //}
        }
    }
}
