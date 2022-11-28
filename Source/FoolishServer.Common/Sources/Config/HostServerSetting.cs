﻿using FoolishGames.Common;
using FoolishServer.RPC;
using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FoolishGames.Net;

namespace FoolishServer.Config
{
    internal class HostServerSetting : IHostSetting
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
        public ESocketType Type { get; private set; }

        /// <summary>
        /// 执行类
        /// </summary>
        public string MainClass { get; private set; }

        /// <summary>
        /// 消息处理的完整类名，用{0}嵌入id
        /// </summary>
        public string ActionClassFullName { get; private set; }

        /// <summary>
        /// TCP全连接队列长度
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
        /// 默认消息处理连接池容量大小
        /// </summary>
        public int MaxIOCapacity { get; private set; }

        /// <summary>
        /// 数据通讯缓存字节大小
        /// </summary>
        public int BufferSize { get; private set; }

        /// <summary>
        /// 通讯内容整体偏移
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// 是否使用压缩
        /// </summary>
        public bool UseGZip { get; private set; }

        /// <summary>
        /// 获取类别显示
        /// </summary>
        public string GetCategory()
        {
            //return $"Host: {Name} [{Type.ToString()}:{Port}]";
            return $"{Type.ToString()}Server: {Port}";
        }

        internal HostServerSetting(XmlNode node)
        {
            Name = node.GetValue("name", null);
            Port = node.GetValue("port", 9001);
            string type = node.GetValue("type", "tcp");
            switch (type.ToLower())
            {
                case "tcp": Type = ESocketType.Tcp; break;
                case "http": Type = ESocketType.Http; break;
                case "web": Type = ESocketType.Web; break;
                case "udp": Type = ESocketType.Udp; break;
            }
            Backlog = node.SelectSingleNode("backlog").GetValue(10000);
            MaxConnections = node.SelectSingleNode("max-connections").GetValue(1000);
            MaxAcceptCapacity = node.SelectSingleNode("max-accept-capacity").GetValue(1000);
            MaxIOCapacity = node.SelectSingleNode("max-io-capacity").GetValue(1000);
            BufferSize = node.SelectSingleNode("buffer-size").GetValue(8192);
            Offset = node.SelectSingleNode("offset").GetValue(8192);
            UseGZip = node.SelectSingleNode("use-gzip").GetValue(true);
            ActionClassFullName = node.SelectSingleNode("action-class-fullname").GetValue(null);
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
