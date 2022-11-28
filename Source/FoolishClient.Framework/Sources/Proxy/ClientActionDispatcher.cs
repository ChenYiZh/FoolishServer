using FoolishClient.Action;
using FoolishGames.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishClient.Proxy
{
    /// <summary>
    /// 客户端Action协议生成类
    /// </summary>
    public class ClientActionDispatcher : IClientActionDispatcher
    {
        /// <summary>
        /// Action名称的格式
        /// </summary>
        public string ActionNameFormat { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="actionNameFormat">Action名称的格式</param>
        public ClientActionDispatcher(string actionNameFormat)
        {
            ActionNameFormat = actionNameFormat;
        }

        /// <summary>
        /// 生成Action协议
        /// </summary>
        public virtual ClientAction Provide(int actionId)
        {
            string typeName = string.Format(ActionNameFormat, actionId);
            return ObjectFactory.Create<ClientAction>(typeName);
        }
    }
}
