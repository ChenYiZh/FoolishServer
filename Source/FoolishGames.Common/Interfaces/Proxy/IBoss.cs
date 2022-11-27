using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Proxy
{
    /// <summary>
    /// 工头
    /// </summary>
    public interface IBoss
    {
        /// <summary>
        /// 工人加入
        /// </summary>
        void CheckIn(IWorker worker);
    }
}
