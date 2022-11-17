using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FoolishGames.Reflection;
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

        public virtual ServerAction Provide(int actionId)
        {
            string typeName = string.Format(ActionNameFormat, actionId);
            return ObjectFactory.Create<ServerAction>(typeName);
        }
    }
}
