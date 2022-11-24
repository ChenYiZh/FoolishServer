using FoolishGames.Common;
using FoolishGames.Log;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace FoolishServer.Data
{
    /// <summary>
    /// mysql连接管理
    /// </summary>
    public sealed class MySQLDatabase : Database<MySqlConnection>
    {
        /// <summary>
        /// 创建连接对象
        /// </summary>
        protected override MySqlConnection CreateDbConnection(IDatabaseSetting setting)
        {
            return new MySqlConnection(setting.ConnectionString);
        }

        /// <summary>
        /// 解析数据库属性字段
        /// </summary>
        private struct FieldInfo : IEntityField
        {
            /// <summary>
            /// 是否是主键
            /// </summary>
            public bool IsKey { get; set; }
            /// <summary>
            /// 在数据库中列名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 是否可为空
            /// </summary>
            public bool Nullable { get; set; }
            /// <summary>
            /// 是否建立索引，只在主表下有用
            /// </summary>
            public bool IsIndex { get; set; }
            /// <summary>
            /// 默认补全值
            /// </summary>
            public object DefaultValue { get; set; }
            /// <summary>
            /// 数据库中的类型
            /// </summary>
            public string DbFieldType { get; set; }
            /// <summary>
            /// 数据类型
            /// </summary>
            public ETableFieldType FieldType { get; set; }

            public override string ToString()
            {
                return $"{Name} [{DbFieldType}, {FieldType.ToString()}]: IsKey: {IsKey}, IsIndex: {IsIndex}, Nullable: {Nullable}, DefaultValue: {DefaultValue}";
            }
        }

        /// <summary>
        /// MySQL管理表的数据库
        /// </summary>
        private const string INFORMATION_SCHEMA = "information_schema";

        /// <summary>
        /// 需要读取的列表信息
        /// </summary>
        private readonly static string[] ColumnReadInfo
            = new string[] {"IC.`COLUMN_NAME`", "IC.`COLUMN_DEFAULT`", "IF(IC.`IS_NULLABLE` = 'YES', 1, 0) AS NULLABLE", "IC.`COLUMN_TYPE`",
                "IF(IC.`COLUMN_NAME` = KCU.`COLUMN_NAME`, 1, 0) AS IS_KEY",//判断是否是主键， >0 是主键
                "IF(IC.`COLUMN_NAME` = S.`COLUMN_NAME`, 1, 0) AS IS_INDEX" //判断是否是索引
            };

        /// <summary>
        /// 对表结构进行调整或创建
        /// </summary>
        public override void GenerateOrUpdateTableScheme(ITableScheme table)
        {
            string tableName = table.TableName;
            FConsole.WriteFormat("TableName: {0}", tableName);
            string returnColumns = string.Join(", ", ColumnReadInfo);
            string sql = $"SELECT {returnColumns} " +
                $"FROM `{INFORMATION_SCHEMA}`.`COLUMNS` AS IC " +
                $"LEFT JOIN `{INFORMATION_SCHEMA}`.`KEY_COLUMN_USAGE` AS KCU " +
                $"ON IC.`TABLE_SCHEMA` = KCU.`TABLE_SCHEMA` AND IC.`TABLE_NAME` = KCU.`TABLE_NAME` AND IC.`COLUMN_NAME` = KCU.`COLUMN_NAME` " +
                $"LEFT JOIN `{INFORMATION_SCHEMA}`.`STATISTICS` AS S " +
                $"ON IC.`TABLE_SCHEMA` = S.`TABLE_SCHEMA` AND IC.`TABLE_NAME` = S.`TABLE_NAME` AND IC.`COLUMN_NAME` = S.`COLUMN_NAME` " +
                $"WHERE IC.`TABLE_SCHEMA` = '{Setting.Database.ToLower()}' AND IC.`TABLE_NAME` = '{tableName}';";

            FConsole.WriteTo(LOG_LEVEL, Kind.ToString(), sql);

            List<FieldInfo> tableFields = Query(sql, (reader) =>
            {
                string fieldType = reader.GetString("COLUMN_TYPE", null);
                return new FieldInfo()
                {
                    IsKey = reader.GetInt32("IS_KEY") == 1,
                    Name = reader.GetString("COLUMN_NAME"),
                    Nullable = reader.GetInt32("NULLABLE", 0) == 1,
                    IsIndex = reader.GetInt32("IS_INDEX", 0) == 1,
                    DefaultValue = reader.GetString("COLUMN_DEFAULT", null),
                    DbFieldType = fieldType,
                    FieldType = ConvertFromString(fieldType),
                };
            });

            sql = null;
            if (tableFields.Count == 0)
            {
                //创建表
                string fieldSqls = string.Join(", ", table.Fields.Values.Select(f => CreateGenerateSQL(f)));
                string keyFieldSqls = string.Join(", ", table.Fields.Values.Where(f => f.IsKey).Select(f => $"`{f.Name}`"));
                ITableFieldScheme[] indexs = table.Fields.Values.Where(f => f.IsIndex && !f.IsKey).ToArray();
                string indexSqls = indexs.Length > 0 ? $", INDEX({string.Join(", ", indexs.Select(f => $"`{f.Name}`"))})" : "";
                sql = $"CREATE TABLE IF NOT EXISTS `{tableName}`({fieldSqls}, PRIMARY KEY ({keyFieldSqls}){indexSqls} )ENGINE=InnoDB DEFAULT CHARSET=utf8;";
                FConsole.WriteTo(LOG_LEVEL, Kind.ToString(), sql);
            }
            else
            {
                FConsole.Write(string.Join("\r\n", tableFields.Select(f => f.ToString())));
            }
            if (!string.IsNullOrEmpty(sql))
            {
                if (Query(sql) != 0)
                {
                    FConsole.WriteTo(LOG_LEVEL, Kind.ToString(), "[Query Error] There are some errors happened on command the sql -> \r\n{0}", sql);
                }
            }
        }

        /// <summary>
        /// 通过MySqlDataReader解析数据
        /// </summary>
        private List<T> Query<T>(string sql, Func<MySqlDataReader, T> deserialize)
        {
            List<T> items = new List<T>();
            using (MySqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = sql;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(deserialize(reader));
                }
                reader.Close();
            }
            return items;
        }

        /// <summary>
        /// 只执行SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private int Query(string sql)
        {
            using (MySqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = sql;
                return command.ExecuteNonQuery();
            }
        }

        private static string CreateGenerateSQL(ITableFieldScheme tableField)
        {
            ETableFieldType type = TableFieldConverter.ConvertFromType(tableField.Type.PropertyType);
            string sql = $"`{tableField.Name}` {ConvertToString(tableField, type)}";
            if (tableField.IsKey || !tableField.Nullable)
            {
                sql += " NOT NULL";
            }
            if (type == ETableFieldType.String || type == ETableFieldType.Text || type == ETableFieldType.LongText)
            {
                if (tableField.DefaultValue != null)
                {
                    sql += $" default '{tableField.DefaultValue}'";
                }
            }
            else if (!tableField.DefaultValue.Equals(FType.GetDefaultValueFromType(tableField.Type.PropertyType)))
            {
                sql += $" default {tableField.DefaultValue}";
            }
            return sql;
        }

        /// <summary>
        /// 从数据库读取的字符串转类型
        /// </summary>
        private static ETableFieldType ConvertFromString(string sqlType)
        {
            if (sqlType.StartsWith("varchar"))
            {
                return ETableFieldType.String;
            }
            else if (sqlType.StartsWith("text"))
            {
                return ETableFieldType.Text;
            }
            else if (sqlType.StartsWith("longtext"))
            {
                return ETableFieldType.LongText;
            }
            else if (sqlType.StartsWith("blob"))
            {
                return ETableFieldType.Blob;
            }
            else if (sqlType.StartsWith("longblob"))
            {
                return ETableFieldType.LongBlob;
            }
            else if (sqlType.StartsWith("bit"))
            {
                return ETableFieldType.Bit;
            }
            else if (sqlType.StartsWith("tinyint"))
            {
                if (sqlType.Contains("unsigned"))
                {
                    return ETableFieldType.Byte;
                }
                else
                {
                    return ETableFieldType.SByte;
                }
            }
            else if (sqlType.StartsWith("smallint"))
            {
                if (sqlType.Contains("unsigned"))
                {
                    return ETableFieldType.UShort;
                }
                else
                {
                    return ETableFieldType.Short;
                }
            }
            else if (sqlType.StartsWith("int"))
            {
                if (sqlType.Contains("unsigned"))
                {
                    return ETableFieldType.UInt;
                }
                else
                {
                    return ETableFieldType.Int;
                }
            }
            else if (sqlType.StartsWith("bigint"))
            {
                if (sqlType.Contains("unsigned"))
                {
                    return ETableFieldType.ULong;
                }
                else
                {
                    return ETableFieldType.Long;
                }
            }
            else if (sqlType.StartsWith("float"))
            {
                return ETableFieldType.Float;
            }
            else if (sqlType.StartsWith("double"))
            {
                return ETableFieldType.Double;
            }
            else if (sqlType.StartsWith("datetime"))
            {
                return ETableFieldType.DateTime;
            }
            return ETableFieldType.Error;
        }

        /// <summary>
        /// 从数据库读取的字符串转类型
        /// </summary>
        public static string ConvertToString(ITableFieldScheme fieldInfo, ETableFieldType type)
        {
            if (type == ETableFieldType.Auto || type == ETableFieldType.Error)
            {
                type = TableFieldConverter.ConvertFromType(fieldInfo.Type.PropertyType);
            }
            if (type == ETableFieldType.Error)
            {
                return null;
            }
            switch (type)
            {
                case ETableFieldType.String: return "varchar(255)";
                case ETableFieldType.Text: return "text";
                case ETableFieldType.LongText: return "longtext";
                case ETableFieldType.Blob: return "blob";
                case ETableFieldType.LongBlob: return "longblob";
                case ETableFieldType.Bit: return "bit";
                case ETableFieldType.Byte: return "tinyint unsigned";
                case ETableFieldType.Short: return "smallint";
                case ETableFieldType.Int: return "int";
                case ETableFieldType.Long: return "bigint";
                case ETableFieldType.SByte: return "tinyint";
                case ETableFieldType.UShort: return "smallint unsigned";
                case ETableFieldType.UInt: return "int unsigned";
                case ETableFieldType.ULong: return "long unsigned";
                case ETableFieldType.Float: return "float";
                case ETableFieldType.Double: return "double";
                case ETableFieldType.DateTime: return "datetime";
            }
            return null;
        }

        /// <summary>
        /// 操作一堆数据
        /// </summary>
        public override bool CommitModifiedEntitys(IEnumerable<DbCommition> commitions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读取表中所有数据
        /// </summary>
        public override IEnumerable<T> LoadAll<T>()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 通过EntityKey，查询某一条数据，没有就返回空
        /// </summary>
        public override T Find<T>(EntityKey key)
        {
            throw new NotImplementedException();
        }
    }
}
