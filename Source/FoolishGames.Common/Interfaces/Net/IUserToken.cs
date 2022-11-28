using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Net
{
    /// <summary>
    /// 连接套接字管理类内部对象
    /// </summary>
    public interface IUserToken
    {
        /// <summary>
        /// 嵌套的Socket
        /// </summary>
        ISocket Socket { get; }

        /// <summary>
        /// 原本的生成时的offset
        /// </summary>
        int OriginalOffset { get; }

        ///// <summary>
        ///// 设置原本的生成时的offset
        ///// </summary>
        //void SetOriginalOffset(int offset);

        /// <summary>
        /// 重置数据
        /// </summary>
        void Reset();
    }
}
