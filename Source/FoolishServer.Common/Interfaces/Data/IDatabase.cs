using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 数据连接的基本类
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// 是否还连接着
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 建立连接
        /// </summary>
        bool Connect();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        /// <summary>
        /// 操作一堆数据
        /// </summary>
        bool CommitModifiedEntitys(IEnumerable<DbCommition> commitions);

        /// <summary>
        /// 读取表中所有
        /// </summary>
        IEnumerable<T> LoadAll<T>() where T : MajorEntity, new();

        /// <summary>
        /// 通过EntityKey，查询某一条数据，没有就返回空
        /// </summary>
        T Find<T>(EntityKey key) where T : MajorEntity, new();
    }
}
