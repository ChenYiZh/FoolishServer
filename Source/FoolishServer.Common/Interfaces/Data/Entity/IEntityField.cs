using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 列信息
    /// </summary>
    public interface IEntityField
    {
        /// <summary>
        /// 是否是主键
        /// </summary>
        bool IsKey { get; }
        /// <summary>
        /// 在数据库中列名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 是否可为空
        /// </summary>
        bool Nullable { get; }
        /// <summary>
        /// 是否建立索引，只在主表下有用
        /// </summary>
        bool IsIndex { get; }
        /// <summary>
        /// 默认补全值
        /// </summary>
        object DefaultValue { get; }
        /// <summary>
        /// 数据类型
        /// </summary>
        ETableFieldType FieldType { get; }
    }
}
