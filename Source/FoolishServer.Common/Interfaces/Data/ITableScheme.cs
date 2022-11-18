using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 表结构
    /// </summary>
    public interface ITableScheme
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
    }
}
