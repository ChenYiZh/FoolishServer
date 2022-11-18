using FoolishServer.Config;
using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 表结构
    /// </summary>
    public class TableScheme : ITableScheme
    {
        /// <summary>
        /// 映射的数据库链接名称
        /// </summary>
        public string ConnectKey { get; private set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; private set; }
        /// <summary>
        /// 表名的格式
        /// </summary>
        public string TableNameFormat { get; private set; }

        internal TableScheme(Type type)
        {
            EntityTableAttribute entityTable = type.GetCustomAttribute<EntityTableAttribute>();
            ConnectKey = entityTable == null ? Settings.GetDefaultConnectKey() : entityTable.ConnectKey;
            string defaultTableName = type.Name.EndsWith("s") ? type.Name : (type.Name + "s");
            TableName = entityTable == null || string.IsNullOrEmpty(entityTable.TableName) ? defaultTableName : entityTable.TableName;
            TableNameFormat = entityTable == null || string.IsNullOrEmpty(entityTable.TableNameFormat) ? "{0}" : entityTable.TableNameFormat;
        }
    }
}
