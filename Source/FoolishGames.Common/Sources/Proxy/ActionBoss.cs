using FoolishGames.Action;
using FoolishGames.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Proxy
{
    /// <summary>
    /// Action的老板
    /// </summary>
    public static class ActionBoss
    {
        /// <summary>
        /// 剥削劳动力
        /// </summary>
        public static void Exploit(GameAction action, int actionId, IMessageReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("The message is empty.");
            }
            action?.Work(actionId, reader);
        }
    }
}
