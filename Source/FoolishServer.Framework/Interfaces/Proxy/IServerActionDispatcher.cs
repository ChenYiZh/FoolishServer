using FoolishServer.Action;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Proxy
{
    /// <summary>
    /// Action工厂
    /// </summary>
    public interface IServerActionDispatcher
    {
        /// <summary>
        /// 获取Action
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        ServerAction Provide(int actionId);
    }
}
