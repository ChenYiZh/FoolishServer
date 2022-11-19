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
        /// </summary>
        string TableNameFormat { get; }
        /// <summary>
        /// 数据是否从不过期，默认true
        /// </summary>
        bool NeverExpired { get; }
        /// <summary>
        /// 存储方案
        /// </summary>
        EStorageType StorageType { get; }
    }
}
