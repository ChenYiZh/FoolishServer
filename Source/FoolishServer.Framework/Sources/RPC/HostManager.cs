using FoolishGames.Collections;
using FoolishServer.RPC;
using FoolishServer.RPC.Host;
using FoolishServer.Config;
using FoolishGames.Log;
using FoolishServer.RPC.Hosts;
using System;
using System.Collections.Generic;
using System.Text;
using FoolishServer.Log;

namespace FoolishServer.RPC
{
    public class HostManager
    {
        private static ThreadSafeDictionary<string, IHost> hosts;

        static HostManager()
        {
            hosts = new ThreadSafeDictionary<string, IHost>();
        }
        /// <summary>
        /// 开启一个服务器
        /// </summary>
        /// <param name="hostType"></param>
        /// <returns></returns>
        public static bool Start(IHostSetting setting)
        {
            if (!CheckSetting(setting))
            {
                return false;
            }
            IHost host = CreateHost(setting);
            try
            {
                if (host.Start(setting))
                {
                    hosts.Add(host.Name, host);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.HOST, e);
                return false;
            }
        }
        /// <summary>
        /// 关闭指定名称的服务器
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static bool Shutdown(string hostName)
        {
            if (hosts.ContainsKey(hostName))
            {
                IHost host = hosts[hostName];
                try
                {
                    host.Shutdown();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.HOST, e);
                }
            }
            return hosts.Remove(hostName);
        }
        /// <summary>
        /// 关闭一个服务器
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool Shutdown(IHost host)
        {
            try
            {
                host.Shutdown();
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.HOST, e);
            }
            return hosts.Remove(host.Name);
        }
        /// <summary>
        /// 关闭所有服务器
        /// </summary>
        public static void Shutdown()
        {
            foreach (KeyValuePair<string, IHost> kv in hosts)
            {
                IHost host = kv.Value;
                try
                {
                    host.Shutdown();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.HOST, e);
                }
            }
            hosts.Clear();
        }
        /// <summary>
        /// 创建Host
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private static IHost CreateHost(IHostSetting setting)
        {
            IHost host = null;
            switch (setting.Type)
            {
                case EHostType.Tcp: host = new TcpHost(); break;
                case EHostType.Udp: host = new UdpHost(); break;
            }
            return host;
        }
        /// <summary>
        /// 判断这个配置是否可用
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static bool CheckSetting(IHostSetting setting)
        {
            if (string.IsNullOrWhiteSpace(setting.Name))
            {
                FConsole.WriteErrorWithCategory(Categories.HOST, "Fail to start the host, because the name of it is empty.");
                return false;
            }
            if (hosts.ContainsKey(setting.Name))
            {
                FConsole.WriteErrorWithCategory(Categories.HOST, "The same host: {0} is running!", setting.Name);
                return false;
            }
            foreach (IHost host in hosts.Values)
            {
                if (host.Port == setting.Port)
                {
                    FConsole.WriteErrorWithCategory(Categories.HOST, "The port of the host: {0}:{1} has been used!", setting.Name, setting.Port);
                    return false;
                }
            }
            return true;
        }
    }
}
