using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Proxy
{
    /// <summary>
    /// 消息处理方案，直接处理
    /// </summary>
    public class DirectMessageProcessor : IBoss
    {
        /// <summary>
        /// 直接进行事务处理
        /// </summary>
        public void CheckIn(IWorker worker)
        {
            worker.Work();
        }
    }
}
