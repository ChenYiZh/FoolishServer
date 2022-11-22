﻿using FoolishGames.Common;
using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace FoolishServer.Config
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// 服务器ID
        /// </summary>
        public static int ServerID { get; private set; }
        /// <summary>
        /// 主版本号
        /// </summary>
        public static int MajorVersion { get; private set; }
        /// <summary>
        /// 副版本号
        /// </summary>
        public static int MinorVersion { get; private set; }
        /// <summary>
        /// 修复版本号
        /// </summary>
        public static string RevisionInfo { get; private set; }
        /// <summary>
        /// 构建版本号
        /// </summary>
        public static string BuildInfo { get; private set; }
        /// <summary>
        /// 获取版本
        /// </summary>
        /// <returns></returns>
        public static string GetVersion() { return $"{MajorVersion}.{MinorVersion}.{(string.IsNullOrEmpty(RevisionInfo) ? "0" : RevisionInfo)}.{(string.IsNullOrEmpty(BuildInfo) ? "0" : BuildInfo)}"; }
        /// <summary>
        /// 脚本存放目录
        /// </summary>
        public static string CSScriptsPath { get; private set; }
        /// <summary>
        /// 编译后的程序集名称
        /// </summary>
        public static string AssemblyName { get; private set; }
        /// <summary>
        /// 服务器启动运行时,需要实现FoolishServer.Runtime.CustomRuntime
        /// </summary>
        public static string MainClass { get; private set; }
        ///// <summary>
        ///// Model类的命名空间
        ///// </summary>
        //public static string ModelNamespace { get; private set; }
        /// <summary>
        /// 是否是Debug模式
        /// </summary>
        public static bool IsDebug { get; private set; }
        /// <summary>
        /// 是否是发布模式
        /// </summary>
        public static bool IsRelease { get { return !IsDebug; } }
        /// <summary>
        /// 服务器配置队列
        /// </summary>
        public static IReadOnlyList<IHostSetting> HostSettings { get; private set; }
        /// <summary>
        /// Redis的配置信息
        /// </summary>
        public static IRedisSetting RedisSetting { get; private set; }
        /// <summary>
        /// 数据库连接信息
        /// </summary>
        public static IReadOnlyDictionary<string, IDatabaseSetting> DatabaseSettings { get; private set; }
        /// <summary>
        /// Redis Key 分隔符
        /// </summary>
        internal const char SPLITE_KEY = '_';
        /// <summary>
        /// 当数据(LoadAll)从db中全部拉取进来后，过多久释放
        /// </summary>
        public static int DataReleasePeriod { get; private set; }
        /// <summary>
        /// 每隔多少时间检测修改的数据并提交
        /// </summary>
        public static int DataCommitInterval { get; private set; }
        /// <summary>
        /// 线程锁的Timeout
        /// </summary>
        public static int LockerTimeout { get; private set; } = 100;
        /// <summary>
        /// 修改缓存池的数量，需要 >= 0
        /// </summary>
        public static int ModifiedCacheCount { get; private set; }
        /// <summary>
        /// 读取配置
        /// </summary>
        internal static void LoadFromFile()
        {
            string xmlFileName = "FoolishServer.config";// Assembly.GetEntryAssembly().GetName().Name + ".config";
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlDocument xml = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(xmlFileName, settings))
            {
                xml.Load(reader);
            }
            IsDebug = xml.SelectSingleNode("/configuration/script/debug").GetValue(true);
            ServerID = xml.SelectSingleNode("/configuration/settings/server").GetValue(1);
            ConvertVersion(xml.SelectSingleNode("/configuration/settings/version").GetValue(null));
            CSScriptsPath = xml.SelectSingleNode("/configuration/script/cs-script-path").GetValue(null);
            AssemblyName = xml.SelectSingleNode("/configuration/script/assembly").GetValue("FoolishServer.Runtime.dll");
            MainClass = xml.SelectSingleNode("/configuration/script/main-class-fullname").GetValue(null);
            //ModelNamespace = xml.SelectSingleNode("/configuration/script/model-namespace").GetValue("FoolishServer.Model");
            DataReleasePeriod = xml.SelectSingleNode("/configuration/settings/data-release-period").GetValue(15000);
            DataCommitInterval = xml.SelectSingleNode("/configuration/settings/data-commit-interval").GetValue(1000);
            LockerTimeout = xml.SelectSingleNode("/configuration/settings/locker-timeout").GetValue(100);
            ModifiedCacheCount = xml.SelectSingleNode("/configuration/settings/modified-cache-count").GetValue(3);
            if (ModifiedCacheCount < 1) { ModifiedCacheCount = 3; }
            LoadHostSettings(xml);
            RedisSetting = new RedisSetting(xml.SelectSingleNode("/configuration/redis"));

            XmlNode dbNodes = xml.SelectSingleNode("/configuration/connections");
            Dictionary<string, IDatabaseSetting> dbSettings = new Dictionary<string, IDatabaseSetting>();
            DatabaseSettings = dbSettings;
            foreach (XmlNode node in dbNodes)
            {
                DatabaseSetting setting = new DatabaseSetting(node);
                if (setting.Kind != Data.EDatabase.Unknow)
                {
                    dbSettings.Add(setting.ConnectKey, setting);
                }
                else
                {
                    FConsole.WriteError("Fail to load db setting!");
                }
            }
        }

        private static void LoadHostSettings(XmlDocument xml)
        {
            List<IHostSetting> settings = new List<IHostSetting>();
            HostSettings = settings;
            XmlNode node = xml.SelectSingleNode("/configuration/hosts");
            if (node == null)
            {
                return;
            }
            foreach (XmlNode child in node)
            {
                HostServerSetting setting = new HostServerSetting(child);
                if (setting.IsValid())
                {
                    settings.Add(setting);
                }
            }
        }

        private static void ConvertVersion(string version)
        {
            MajorVersion = MinorVersion = 0;
            RevisionInfo = BuildInfo = null;
            if (!string.IsNullOrEmpty(version))
            {
                string[] values = version.Split('.');
                MajorVersion = int.Parse(values[0]);
                if (values.Length > 1)
                {
                    MinorVersion = int.Parse(values[1]);
                }
                if (values.Length > 2)
                {
                    RevisionInfo = values[2];
                }
                if (values.Length > 3)
                {
                    BuildInfo = values[3];
                }
            }
        }

        public static string GetDefaultConnectKey()
        {
            if (DatabaseSettings == null || DatabaseSettings.Count == 0 || DatabaseSettings.ContainsKey("default"))
            {
                return "default";
            }
            return DatabaseSettings.Keys.First();
        }
    }
}
