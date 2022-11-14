using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.RPC
{
    public static class SessionExtensions
    {
        /// <summary>
        /// 判断有效性
        /// </summary>
        public static bool IsValid(this ISession session)
        {
            GameSession mSession = session as GameSession;
            return mSession != null && mSession.IsValid && mSession.UserId > 0;
        }
    }
}
