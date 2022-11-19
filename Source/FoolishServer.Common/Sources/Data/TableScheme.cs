using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// 表结构
    /// </summary>
    public sealed class TableScheme : ITableScheme
    {
        /// <summary>
        /// 私有属性
        /// </summary>
        private EntityTableAttribute EntityTable { get; set; }
        /// <summary>
        /// Attribute信息
        /// </summary>
        public IEntityTable Attribute { get; private set; }
        /// <summary>
        /// 结构类型
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// 列信息
        /// </summary>
        public IReadOnlyDictionary<string, ITableFieldScheme> Fields { get; private set; }
        /// <summary>
        /// 主键列
        /// </summary>
        internal IReadOnlyList<ITableFieldScheme> KeyFields { get; private set; }

        internal TableScheme(Type type)
        {
            EntityTableAttribute entityTable = type.GetCustomAttribute<EntityTableAttribute>();
            Type = type;
            if (entityTable == null)
            {
                entityTable = new EntityTableAttribute();
            }
            if (string.IsNullOrEmpty(entityTable.TableName))
            {
                entityTable.TableName = type.Name.EndsWith("s") ? type.Name : (type.Name + "s"); ;
            }
            Attribute = entityTable;

            //读取列信息
            Dictionary<string, ITableFieldScheme> fields = new Dictionary<string, ITableFieldScheme>();
            Fields = fields;
            List<ITableFieldScheme> keys = new List<ITableFieldScheme>();
            KeyFields = keys;

            //遍历读取
            PropertyInfo[] properties = type.GetProperties();
            bool hasKey = false;
            EntityFieldAttribute firstAttribute = null;
            TableFieldScheme firstField = null;
            foreach (PropertyInfo property in properties)
            {
                EntityFieldAttribute attribute = property.GetCustomAttribute<EntityFieldAttribute>();
                if (attribute == null) continue;
                TableFieldScheme field = new TableFieldScheme(property, attribute);
                fields.Add(field.PropertyName, field);
                if (firstAttribute == null)
                {
                    firstAttribute = attribute;
                    firstField = field;
                }
                if (field.IsKey)
                {
                    //FConsole.Write(property.Name);
                    hasKey = true;
                    keys.Add(field);
                }
            }
            if (!hasKey)
            {
                firstAttribute.IsKey = true;
                keys.Add(firstField);
            }
        }

        /// <summary>
        /// 映射的数据库链接名称
        /// </summary>
        public string ConnectKey { get { return EntityTable.ConnectKey; } }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get { return EntityTable.TableName; } }
        /// <summary>
        /// 表名的格式
        /// </summary>
        public string TableNameFormat { get { return EntityTable.TableNameFormat; } }
        /// <summary>
        /// 是否从不过期,模式True
        /// </summary>
        public bool NeverExpired { get { return EntityTable.NeverExpired; } }
        /// <summary>
        /// 存储方案，默认Redis和db都读写
        /// </summary>
        public EStorageType StorageType { get { return EntityTable.StorageType; } }
    }
}
