using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 列数据
    /// </summary>
    public interface ITableFieldScheme : IEntityField
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        string PropertyName { get; }
        /// <summary>
        /// Attribute信息
        /// </summary>
        IEntityField Attribute { get; }
        /// <summary>
        /// 列类型
        /// </summary>
        PropertyInfo Type { get; }
    }
}
