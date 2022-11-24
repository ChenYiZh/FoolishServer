using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FoolishGames.Common;
using FoolishServer.Common;
using FoolishServer.Data.Entity;

namespace FoolishServer.Data
{
    /// <summary>
    /// 表中的列结构
    /// </summary>
    public sealed class TableFieldScheme : ITableFieldScheme
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; internal set; }
        /// <summary>
        /// Attribute信息
        /// </summary>
        public IEntityField Attribute { get; private set; }
        /// <summary>
        /// 列类型
        /// </summary>
        public PropertyInfo Type { get; private set; }

        internal TableFieldScheme(PropertyInfo property, EntityFieldAttribute attribute)
        {
            Attribute = attribute;
            Type = property;
            PropertyName = property.Name;
            Name = string.IsNullOrWhiteSpace(attribute.Name) ? property.Name : attribute.Name;
            Name = StringConverter.ToLowerWithDownLine(Name);
            if (Attribute.DefaultValue == null)
            {
                if (!Nullable)
                {
                    DefaultValue = FType.GetDefaultValueFromType(property.PropertyType);
                }
            }
            else
            {
                DefaultValue = Attribute.DefaultValue;
            }
            FieldType = attribute.FieldType != ETableFieldType.Auto ? attribute.FieldType : TableFieldConverter.ConvertFromType(property.PropertyType); ;
        }
        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool IsKey { get { return Attribute.IsKey; } }
        /// <summary>
        /// 在数据库中列名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 是否可为空
        /// </summary>
        public bool Nullable { get { return IsKey ? false : Attribute.Nullable; } }
        /// <summary>
        /// 是否建立索引，只在主表下有用
        /// </summary>
        public bool IsIndex { get { return IsKey || Attribute.IsIndex; } }
        /// <summary>
        /// 默认补全值
        /// </summary>
        public object DefaultValue { get; private set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public ETableFieldType FieldType { get; private set; }
    }
}
