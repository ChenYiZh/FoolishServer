using FoolishServer.Config;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// mysql连接管理
    /// </summary>
    public sealed class MySQLDatabase : Database
    {
        /// <summary>
        /// 创建连接对象
        /// </summary>
        protected override DbConnection CreateDbConnection(IDatabaseSetting setting)
        {
            return new MySqlConnection(setting.ConnectionString);
        }

        /// <summary>
        /// 对表结构进行调整或创建
        /// </summary>
        /// <param name="table"></param>
        public override void GenerateOrUpdateTableScheme(ITableScheme table)
        {
            throw new NotImplementedException();
        }
    }
}
