using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Data.Entity
{
    /// <summary>
    /// 属性字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EntityFieldAttribute : Attribute, IEntityField
    {
        public string FieldName { get; set; }
    }
}
