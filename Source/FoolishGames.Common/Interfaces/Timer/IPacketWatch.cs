using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Timer
{
    public interface IPacketWatch
    {
        /// <summary>
        /// 获取当前时间
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// 获取UTC时间
        /// </summary>
        DateTime UTC { get; }
    }
}
