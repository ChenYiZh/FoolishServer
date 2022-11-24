using FoolishGames.Log;
using FoolishGames.Timer;
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
        public IEntityTable EntityTable { get; private set; }
        /// <summary>
        /// 结构类型
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// 列信息，Key: PropertyName
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
            EntityTable = entityTable;

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
        public string TableName { get { return GetTableName(); } }
        /// <summary>
        /// {0}为数据库名称，{1:MMdd}为之间名称
        /// </summary>
        public string TableNameFormat { get { return EntityTable.TableNameFormat; } }
        /// <summary>
        ///  是否从不过期,判断是否产生冷数据，默认false
        /// </summary>
        public bool NeverExpired { get { return EntityTable.NeverExpired; } }
        /// <summary>
        /// 存储方案(位运算)
        /// <para>默认 StorageType.WriteToRedis | EStorageType.ReadFromRedis | EStorageType.WriteToDb | EStorageType.ReadFromDb</para>
        /// </summary>
        public EStorageType StorageType { get { return EntityTable.StorageType; } }
        /// <summary>
        /// 缓存的临时表名
        /// </summary>
        private string PrivateTempTableName;
        /// <summary>
        /// 缓存全局表名
        /// </summary>
        private string PrivateGlobalTableName;
        /// <summary>
        /// 用于存数据库的表名
        /// </summary>
        public string GetTableName()
        {
            string tableName = Type.Name.EndsWith("s") ? Type.Name : Type.Name + "s";
            if (!string.IsNullOrWhiteSpace(EntityTable.TableName))
            {
                tableName = EntityTable.TableName;
            }
            string format = EntityTable.TableNameFormat;
            if (string.IsNullOrEmpty(EntityTable.TableNameFormat) || !EntityTable.TableNameFormat.Contains("{0}"))
            {
                format = "{0}";
            }
            tableName = string.Format(format, tableName, TimeLord.Now);
            return tableName;
            if (PrivateTempTableName != tableName)
            {
                PrivateTempTableName = tableName;
                string lowTableName = tableName.ToLower();
                PrivateGlobalTableName = "";
                for (int i = 0; i < lowTableName.Length; i++)
                {
                    char lc = lowTableName[i];
                    char c = PrivateTempTableName[i];
                    if (lc == c)
                    {
                        PrivateGlobalTableName += lc;
                    }
                    else
                    {
                        PrivateGlobalTableName += "_" + lc;
                    }
                }
            }
            return PrivateGlobalTableName;
        }
    }
}
