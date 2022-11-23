using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// SQL标准数据库
    /// </summary>
    public interface ISQLDatabase : IDatabase
    {
        /// <summary>
        /// 连接器
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// 先建立连接
        /// </summary>
        void CreateConnection(IDatabaseSetting setting);

        /// <summary>
        /// 对表结构进行调整或创建
        /// </summary>
        void GenerateOrUpdateTableScheme(ITableScheme table);
    }
}
