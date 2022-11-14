using FoolishGames.Action;
using FoolishGames.IO;
using FoolishServer.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Action
{
    /// <summary>
    /// 服务器Action
    /// </summary>
    public abstract class ServerAction : GameAction, IServerAction
    {
        /// <summary>
        /// 对应的会话窗口
        /// </summary>
        public ISession Session { get; internal set; }

        /// <summary>
        /// 玩家Id
        /// </summary>
        public long UserId { get { return Session == null ? 0 : Session.UserId; } }

        /// <summary>
        /// 判断是否有效
        /// </summary>
        /// <returns></returns>
        public override bool Check()
        {
            return Session.IsValid();
        }
    }
}
