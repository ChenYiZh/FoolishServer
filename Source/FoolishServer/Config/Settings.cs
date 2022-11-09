using FoolishServer.Common;
using FoolishServer.Framework.Config;
using System;
using System.Collections.Generic;
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
        /// 读取配置
        /// </summary>
        internal static void LoadFromFile()
        {
            string xmlFileName = Assembly.GetEntryAssembly().GetName().Name + ".config";
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
            LoadHostSettings(xml);
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
                HostSetting setting = new HostSetting(child);
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
    }
}
