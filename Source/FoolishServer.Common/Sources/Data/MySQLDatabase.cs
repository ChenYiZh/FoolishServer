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
        protected override DbConnection CreateDbConnection(IDatabaseSetting setting)
        {
            return new MySqlConnection(setting.ConnectionString);
        }
    }
}
