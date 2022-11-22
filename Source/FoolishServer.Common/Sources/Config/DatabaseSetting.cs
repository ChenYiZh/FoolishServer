using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FoolishGames.Common;
using FoolishGames.Log;
using FoolishServer.Data;

namespace FoolishServer.Config
{
    /// <summary>
    /// 数据库设置
    /// </summary>
    public class DatabaseSetting : IDatabaseSetting
    {
        /// <summary>
        /// 数据库映射名称
        /// </summary>
        public string ConnectKey { get; private set; }

        /// <summary>
        /// 数据库连接名称
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// 是什么类型的数据库
        /// </summary>
        public EDatabase Kind { get; private set; }

        public DatabaseSetting(XmlNode node)
        {
            ConnectKey = node.GetValue("key", "default");
            ProviderName = node.GetValue("providerName", null);
            ConnectionString = node.GetValue("connectionString", null);
            switch (ProviderName.ToLower())
            {
                case "mysqldataprovider": Kind = EDatabase.MySQL; break;
                default: Kind = EDatabase.Unknow; break;
            }
        }
    }
}
