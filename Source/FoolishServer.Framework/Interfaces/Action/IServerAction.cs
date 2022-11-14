using FoolishGames.Action;
using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Action
{
    /// <summary>
    /// 服务器Action
    /// </summary>
    public interface IServerAction : IAction
    {
        /// <summary>
        /// 会话窗口
        /// </summary>
        ISession Session { get; }

        /// <summary>
        /// 玩家Id
        /// </summary>
        long UserId { get; }
    }
}
