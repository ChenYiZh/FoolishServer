﻿using FoolishServer.Common;
using FoolishServer.Framework.RPC;
using FoolishServer.Framework.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FoolishServer.Config
{
    public class HostSetting : IHostSetting
    {
        /// <summary>
        /// 服务器标识
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 类型
        /// </summary>
        public EHostType Type { get; private set; }

        /// <summary>
        /// 挂起连接的最大长度
        /// </summary>
        public int Backlog { get; private set; }

        /// <summary>
        /// 最大并发数量
        /// </summary>
        public int MaxConnections { get; private set; }

        /// <summary>
        /// 默认连接对象池容量
        /// </summary>
        public int MaxAcceptCapacity { get; private set; }

        /// <summary>
        /// 执行类
        /// </summary>
        public string MainClass { get; private set; }

        /// <summary>
        /// 获取类别显示
        /// </summary>
        public string GetCategory()
        {
            //return $"Host: {Name} [{Type.ToString()}:{Port}]";
            return $"Host: {Port}";
        }

        internal HostSetting(XmlNode node)
        {
            Name = node.GetValue("name", null);
            Port = node.GetValue("port", 9001);
            string type = node.GetValue("type", "tcp");
            switch (type.ToLower())
            {
                case "tcp": Type = EHostType.Tcp; break;
                case "http": Type = EHostType.Http; break;
                case "web-socket": Type = EHostType.WebSocket; break;
                case "udp": Type = EHostType.Udp; break;
            }
            Backlog = node.SelectSingleNode("backlog").GetValue(10000);
            MaxConnections = node.SelectSingleNode("max-connections").GetValue(1000);
            MaxAcceptCapacity = node.SelectSingleNode("max-accept-capacity").GetValue(1000);
        }
        /// <summary>
        /// 判断是否有效
        /// </summary>
        /// <returns></returns>
        internal bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}
