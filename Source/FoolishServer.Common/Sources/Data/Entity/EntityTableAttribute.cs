using FoolishServer.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 存储类型
    /// </summary>
    public enum EStorageType
    {
        /// <summary>
        /// 通过默认的策略从Redis和db加载保存
        /// </summary>
        ReadWrite = 0,
        /// <summary>
        /// 在Redis层读写，只往Db写
        /// </summary>
        ReadWriteRedisAndOnlyWriteToDb = 5,
        /// <summary>
        /// 只在Redis保存加载
        /// </summary>
        OnlyRedis = 10,
        /// <summary>
        /// 只在Db保存加载
        /// </summary>
        OnlyDb = 20,
        /// <summary>
        /// 只往db写
        /// </summary>
        OnlyWriteToDb = 21,
    }
    /// <summary>
    /// 实体表映射属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EntityTableAttribute : Attribute, IEntityTable
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

        /// <summary>
        /// 是否从不过期,模式True
        /// </summary>
        public bool NeverExpired { get; set; } = true;

        /// <summary>
        /// 存储方案，默认Redis和db都读写
        /// </summary>
        public EStorageType StorageType { get; set; } = EStorageType.ReadWrite;

        public EntityTableAttribute()
        {
            ConnectKey = Settings.GetDefaultConnectKey();
            TableName = null;
            TableNameFormat = "{0}";
        }

        public EntityTableAttribute(EStorageType storageType) : this()
        {
            StorageType = storageType;
        }

        public EntityTableAttribute(string connectKey) : this()
        {
            ConnectKey = connectKey;
        }

        public EntityTableAttribute(string connectKey, string tableNameFormat) : this(connectKey)
        {
            TableNameFormat = tableNameFormat;
        }

        public EntityTableAttribute(string connectKey, EStorageType storageType) : this(connectKey)
        {
            StorageType = storageType;
        }

        public EntityTableAttribute(string connectKey, string tableNameFormat, EStorageType storageType) : this(connectKey, tableNameFormat)
        {
            StorageType = storageType;
        }
    }
}
