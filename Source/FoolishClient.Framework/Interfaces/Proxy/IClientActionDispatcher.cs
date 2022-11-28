using FoolishClient.Action;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishClient.Proxy
{
    /// <summary>
    /// Action工厂
    /// </summary>
    public interface IClientActionDispatcher
    {
        /// <summary>
        /// 获取Action
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        ClientAction Provide(int actionId);
    }
}
