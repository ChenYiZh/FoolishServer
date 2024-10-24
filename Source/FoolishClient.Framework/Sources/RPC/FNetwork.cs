﻿using FoolishClient.Log;
using FoolishClient.Net;
using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.IO;
using FoolishGames.Log;
using System;
using System.Collections.Generic;

namespace FoolishClient.RPC
{
    /// <summary>
    /// 网络处理类
    /// </summary>
    public static class FNetwork
    {
        /// <summary>
        /// 默认使用的套接字名称
        /// </summary>
        public static string DefaultUsableSocketName { get; set; } = "default";

        /// <summary>
        /// 客户端套接字列表
        /// </summary>
        private static IDictionary<string, IClientSocket> _sockets = new ThreadSafeDictionary<string, IClientSocket>();

        /// <summary>
        /// 创建套接字
        /// </summary>
        /// <param name="name">标识名称</param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="actionClassFullName">Action协议类的完整名称</param>
        /// <param name="heartbeatInterval">心跳间隔</param>
        public static TcpClientSocket MakeTcpSocket(string name, string host, int port,
            string actionClassFullName,
            int heartbeatInterval = Constants.HeartBeatsInterval,
            bool usePing = true, int pingInterval = Constants.PingInterval)
        {
            TcpClientSocket socket = new TcpClientSocket();
            if (!MakeSocket(name, socket))
            {
                throw new Exception("The same key: " + name + " exists in Network sockets.");
            }
            socket.Ready(name, host, port, actionClassFullName, heartbeatInterval, usePing, pingInterval);
            return socket;
        }

        /// <summary>
        /// 创建套接字
        /// </summary>
        /// <param name="name">标识名称</param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="actionClassFullName">Action协议类的完整名称</param>
        /// <param name="heartbeatInterval">心跳间隔</param>
        public static UdpClientSocket MakeUdpSocket(string name, string host, int port,
            string actionClassFullName,
            int heartbeatInterval = Constants.HeartBeatsInterval,
            bool usePing = true, int pingInterval = Constants.PingInterval)
        {
            UdpClientSocket socket = new UdpClientSocket();
            if (!MakeSocket(name, socket))
            {
                throw new Exception("The same key: " + name + " exists in Network sockets.");
            }
            socket.Ready(name, host, port, actionClassFullName, heartbeatInterval, usePing, pingInterval);
            return socket;
        }

        /// <summary>
        /// 创建套接字
        /// </summary>
        private static bool MakeSocket(string name, IClientSocket socket)
        {
            IClientSocket exists;
            if (!_sockets.TryGetValue(name, out exists))
            {
                _sockets[name] = socket;
                return true;
            }
            if (exists.IsRunning)
            {
                return false;
            }
            _sockets[name] = socket;
            return true;
        }

        /// <summary>
        /// 获取指定的Socket
        /// </summary>
        public static IClientSocket GetSocket(string name)
        {
            IClientSocket socket;
            if (_sockets.TryGetValue(name, out socket))
            {
                return socket;
            }
            return null;
        }

        /// <summary>
        /// 使用默认套接字来发送消息
        /// <para>通过DefaultUsableSocketName来配置默认的套接字名称</para>
        /// </summary>
        public static void Send(int actionId, IMessageWriter message)
        {
            Send(DefaultUsableSocketName, actionId, message);
        }

        /// <summary>
        /// 使用套接字来发送消息
        /// </summary>
        public static void Send(string socketName, int actionId, IMessageWriter message)
        {
            message.ActionId = actionId;
            message.OpCode = 0;
            GetSocket(socketName)?.Send(message);
        }

        /// <summary>
        /// 关闭所有套接字连接
        /// </summary>
        public static void Shutdown()
        {
            foreach (KeyValuePair<string, IClientSocket> socket in _sockets)
            {
                try
                {
                    socket.Value.Close();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.SOCKET, e);
                }
            }
            _sockets.Clear();
        }

        /// <summary>
        /// 获取 Ping 的往返时间，最大值为Constants.MaxRoundtripTime
        /// </summary>
        /// <returns></returns>
        public static int GetRoundtripTime()
        {
            return GetRoundtripTime(DefaultUsableSocketName);
        }

        /// <summary>
        /// 获取 Ping 的往返时间，最大值为Constants.MaxRoundtripTime
        /// </summary>
        /// <returns></returns>
        public static int GetRoundtripTime(string serverName)
        {
            return GetSocket(serverName).RoundtripTime;
        }
    }
}
