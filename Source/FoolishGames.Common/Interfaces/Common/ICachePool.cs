using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FoolishGames.Common
{
    /// <summary>
    /// 缓存队列池
    /// </summary>
    public interface ICachePool<T>
    {
        /// <summary>
        /// 中间线程
        /// </summary>
        Thread Thread { get; }
        /// <summary>
        /// 外部用来加锁的工具
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// 缓存池数量
        /// </summary>
        int PoolCount { get; }
        /// <summary>
        /// 把数据加入缓存池
        /// </summary>
        void Push(T entity);
    }
}
