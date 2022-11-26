﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 数据对象池，
    /// 默认只加载缓存中的数据。
    /// 如果需要针对完整数据进行遍历，先执行LoadAll
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntitySet<T> : IEnumerable<T> where T : MajorEntity, new()
    {
        ///// <summary>
        ///// 查找，如果缓存中找不到，会从数据库中查询
        ///// </summary>
        //T Find(long entityId);

        /// <summary>
        /// 主键查找，如果缓存中找不到，会从数据库中查询
        /// </summary>
        T Find(params object[] keys);

        /// <summary>
        /// 主键类查询
        /// </summary>
        T Find(EntityKey key);

        /// <summary>
        /// 根据Lamda返回新的列表，不会影响内部数据列表
        /// </summary>
        IList<T> Find(Func<T, bool> condition);

        /// <summary>
        /// 添加数据
        /// </summary>
        bool AddOrUpdate(T entity);

        /// <summary>
        /// 删除数据，同时删除缓存，Redis，数据库中的数据
        /// </summary>
        bool Remove(T entity);

        /// <summary>
        /// 通过Key删除数据，同时删除缓存，Redis，数据库中的数据
        /// </summary>
        bool Remove(EntityKey key);

        /// <summary>
        /// 加载完整的数据，从数据库中加载并附加到缓存中
        /// </summary>
        void LoadAll();
    }
}
