using FoolishGames.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FoolishServer.Config
{
    /// <summary>
    /// Redis设置信息
    /// </summary>
    public class RedisSetting : IRedisSetting
    {
        /// <summary>
        /// Redis链接地址
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// Redis端口
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// 连接密码
        /// </summary>
        public string Password { get; private set; }
        /// <summary>
        /// DbIndex
        /// </summary>
        public int DbIndex { get; private set; }
        /// <summary>
        /// 连接Timeout
        /// </summary>
        public int Timeout { get; private set; }

        public RedisSetting(XmlNode node)
        {
            Host = node.GetValue("host", "127.0.0.1");
            Port = node.GetValue("port", 6379);
            Password = node.GetValue("password", null);
            DbIndex = node.GetValue("db", 0);
            Timeout = node.SelectSingleNode("timeout").GetValue(5000);
            HostCheck();
        }

        private void HostCheck()
        {
            string[] values = Host.Split('@', ':');
            if (values.Length == 3)
            {
                Password = values[0];
                Host = values[1];
                Port = int.Parse(values[2]);
            }
            else if (values.Length == 2)
            {
                if (Host.Contains("@"))
                {
                    Password = values[0];
                    Host = values[1];
                }
                else
                {
                    Host = values[0];
                    Port = int.Parse(values[2]);
                }
            }
        }
    }
}
