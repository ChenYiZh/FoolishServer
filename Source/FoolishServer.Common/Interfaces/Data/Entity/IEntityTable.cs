using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 表信息
    /// </summary>
    public interface IEntityTable
    {
        /// <summary>
        /// 映射的数据库链接名称
        /// </summary>
        string ConnectKey { get; }
        /// <summary>
        /// 表名
        /// </summary>
        string TableName { get; }
        /// <summary>
        /// 表名的格式
        /// {0}为数据库名称，{1:MMdd}为之间名称
        /// </summary>
        string TableNameFormat { get; }
        /// <summary>
        /// 是否从不过期,判断是否产生冷数据，默认false
        /// </summary>
        bool NeverExpired { get; }
        /// <summary>
        /// 存储方案(位运算)
        /// <para>默认 StorageType.WriteToRedis | EStorageType.ReadFromRedis | EStorageType.WriteToDb | EStorageType.ReadFromDb</para>
        /// </summary>
        EStorageType StorageType { get; }
    }
}
