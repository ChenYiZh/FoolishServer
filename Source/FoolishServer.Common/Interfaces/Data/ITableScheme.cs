using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 表结构
    /// </summary>
    public interface ITableScheme : IEntityTable
    {
        /// <summary>
        /// Attribute信息
        /// </summary>
        IEntityTable EntityTable { get; }

        /// <summary>
        /// 结构类型
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// 列信息
        /// </summary>
        IReadOnlyDictionary<string, ITableFieldScheme> Fields { get; }

        /// <summary>
        /// 用于存数据库的表名
        /// </summary>
        string GetTableName();
    }
}
