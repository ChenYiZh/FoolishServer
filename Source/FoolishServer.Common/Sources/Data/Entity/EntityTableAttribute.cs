using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 实体表映射属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EntityTableAttribute : Attribute
    {
        /// <summary>
        /// 数据库映射名称
        /// </summary>
        public string ConnectKey { get; set; }

        /// <summary>
        /// 数据表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 表名的格式
        /// </summary>
        public string TableNameFormat { get; set; }

        public EntityTableAttribute(string tableName)
        {
            ConnectKey = Settings.GetDefaultConnectKey();
            TableName = tableName;
            TableNameFormat = "{0}";
        }

        public EntityTableAttribute(string connectKey, string tableName) : this(tableName)
        {
            ConnectKey = connectKey;
        }

        public EntityTableAttribute(string connectKey, string tableName, string tableNameFormat) : this(connectKey, tableName)
        {
            TableNameFormat = tableNameFormat;
        }
    }
}
