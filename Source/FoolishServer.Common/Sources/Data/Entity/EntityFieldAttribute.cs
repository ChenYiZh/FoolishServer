using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 数据库中字段类型
    /// </summary>
    public enum ETableFieldType
    {
        /// <summary>
        /// 默认参数，自动匹配
        /// </summary>
        Auto = 0,
        /// <summary>
        /// TINYTEXT/varchar(255)
        /// </summary>
        String = 1,
        /// <summary>
        /// TEXT/varchar(65,535)
        /// </summary>
        Text = 2,
        /// <summary>
        /// 数据库中最大可存入的数据长度
        /// </summary>
        LongText = 3,
        /// <summary>
        /// 二进制数据：65,535 字节的数据/varbinary
        /// </summary>
        Blob = 5,
        /// <summary>
        /// 二进制数据：LongBlob/varbinary(max)
        /// </summary>
        LongBlob = 6,
        /// <summary>
        /// bit: 字节0/1
        /// </summary>
        Bit = 10,
        /// <summary>
        /// TINYINT
        /// </summary>
        Byte = 11,
        /// <summary>
        /// SMALLINT
        /// </summary>
        Short = 12,
        /// <summary>
        /// INT
        /// </summary>
        Int = 13,
        /// <summary>
        /// BIGINT
        /// </summary>
        Long = 14,
        /// <summary>
        /// FLOAT/float(24)
        /// </summary>
        Float = 20,
        /// <summary>
        /// DOUBLE/float(53)
        /// </summary>
        Double = 21,
        /// <summary>
        /// DATETIME
        /// </summary>
        DateTime = 30,
    }
    /// <summary>
    /// 属性字段
    /// String类型默认varchar(255),列表默认LongText
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EntityFieldAttribute : Attribute, IEntityField
    {
        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool IsKey { get; set; } = false;
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否可为空
        /// </summary>
        public bool Nullable { get; set; } = true;
        /// <summary>
        /// 是否建立索引，只在主表下有用
        /// </summary>
        public bool IsIndex { get; set; } = false;
        /// <summary>
        /// 默认补全值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 数据类新
        /// </summary>
        public ETableFieldType FieldType { get; set; } = ETableFieldType.Auto;
    }
}
