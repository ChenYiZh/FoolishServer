using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 操作码
    /// </summary>
    public enum EOpCode : sbyte
    {
        /// <summary>
        /// 空数据
        /// </summary>
        Empty = -1,
        ///// <summary>
        ///// 连续性数据
        ///// </summary>
        //Continuation = 0,
        /// <summary>
        /// 文本数据
        /// </summary>
        Text = 1,
        /// <summary>
        /// 二进制数据
        /// </summary>
        Binary = 2,
        /// <summary>
        /// 关闭操作数据
        /// </summary>
        Close = 8,
        /// <summary>
        /// Ping数据
        /// </summary>
        Ping = 9,
        /// <summary>
        /// Pong数据
        /// </summary>
        Pong = 10
    }
}
