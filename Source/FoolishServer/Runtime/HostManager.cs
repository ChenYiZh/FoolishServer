using FoolishServer.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Runtime
{
    public class HostManager
    {
        private static Dictionary<string, IRuntimeHost> hosts;

        static HostManager()
        {
            hosts = new Dictionary<string, IRuntimeHost>();
        }
        /// <summary>
        /// 开启一个服务器
        /// </summary>
        /// <param name="hostType"></param>
        /// <returns></returns>
        public static bool Start(EHostType hostType)
        {
            return true;
        }
        /// <summary>
        /// 关闭指定名称的服务器
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static bool Shutdown(string hostName)
        {
            return true;
        }
        /// <summary>
        /// 关闭一个服务器
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool Shutdown(IRuntimeHost host)
        {
            return true;
        }
        /// <summary>
        /// 关闭所有服务器
        /// </summary>
        public static void Shutdown()
        {

        }
    }
}
