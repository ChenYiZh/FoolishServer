using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FoolishServer.Action;

namespace FoolishServer.Proxy
{
    /// <summary>
    /// 默认反射创建Action
    /// </summary>
    public class ServerActionDispatcher : IServerActionDispatcher
    {
        /// <summary>
        /// Action名称的格式
        /// </summary>
        public string ActionNameFormat { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="actionNameFormat">Action名称的格式</param>
        public ServerActionDispatcher(string actionNameFormat)
        {
            ActionNameFormat = actionNameFormat;
        }

        public ServerAction Provide(int actionId)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            return (ServerAction)assembly.CreateInstance(string.Format(ActionNameFormat, actionId), true);
        }
    }
}
