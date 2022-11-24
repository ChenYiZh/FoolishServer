using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.Log;
using FoolishGames.Proxy;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using FoolishServer.Struct;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;

namespace FoolishServer.Data
{
    /// <summary>
    /// mysql连接管理
    /// </summary>
    public sealed class MySQLDatabase : Database<MySqlConnection>
    {
        /// <summary>
        /// 事务队列
        /// </summary>
        private ThreadSafeQueue<DbCommition>[] CommitionPool = new ThreadSafeQueue<DbCommition>[3];

        /// <summary>
        /// 读写时用的互斥锁
        /// </summary>
        private object SyncRoot = new object();

        /// <summary>
        /// 事务锁
        /// </summary>
        private int CommitionPoolIndex = 0;

        /// <summary>
        /// 推送线程
        /// </summary>
        private Thread PushThread;

        /// <summary>
        /// 创建连接对象
        /// </summary>
        protected override MySqlConnection CreateDbConnection(IDatabaseSetting setting)
        {
            for (int i = 0; i < CommitionPool.Length; i++)
            {
                CommitionPool[i] = new ThreadSafeQueue<DbCommition>();
            }
            //如果有二进制保存 + ";Allow User Variables=True"
            MySqlConnection connection = new MySqlConnection(setting.ConnectionString);
            PushThread = new Thread(PushingCommitions);
            PushThread.Start();
            return connection;
        }

        /// <summary>
        /// 业务处理线程
        /// </summary>
        /// <param name="state"></param>
        private void PushingCommitions(object state)
        {
            while (true)
            {
                bool lockToken = false;
                try
                {
                    Monitor.TryEnter(SyncRoot, 100, ref lockToken);
                    if (lockToken)
                    {
                        int nextIndex = CommitionPoolIndex + 1;
                        if (nextIndex >= CommitionPool.Length)
                        {
                            nextIndex = 0;
                        }
                        int pushIndex = CommitionPoolIndex - 1;
                        if (pushIndex < 0)
                        {
                            pushIndex = CommitionPool.Length - 1;
                        }
                        Interlocked.Exchange(ref CommitionPoolIndex, nextIndex);
                        ThreadSafeQueue<DbCommition> commitions = CommitionPool[pushIndex];
                        while (commitions.Count > 0)
                        {
                            DbCommition commition = commitions.Dequeue();
                            try
                            {
                                GenerateModifySql(commition);
                            }
                            catch (Exception e)
                            {
                                FConsole.WriteExceptionWithCategory(Kind.ToString(), e);
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
                finally
                {
                    if (lockToken)
                    {
                        Monitor.Exit(SyncRoot);
                    }
                }
            }
        }

        /// <summary>
        /// 关闭时执行
        /// </summary>
        public override void Close()
        {
            if (PushThread != null)
            {
                PushThread.Abort();
            }
            PushThread = null;
            base.Close();
        }

        #region CheckTableSchema
        /// <summary>
        /// 解析数据库属性字段
        /// </summary>
        private class FieldInfo : IEntityField
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
            string sql = "SHOW TABLES;";
            HashSet<string> tableNames = Query<HashSet<string>, string>(sql, (reader) => { return reader.GetString(0); });//加载所有表

            #region 通过imformation_schema查询列信息，权限问题

            //string returnColumns = string.Join(", ", ColumnReadInfo);
            //string sql = $"SELECT {returnColumns} " +
            //    $"FROM `{INFORMATION_SCHEMA}`.`COLUMNS` AS IC " +
            //    $"LEFT JOIN `{INFORMATION_SCHEMA}`.`KEY_COLUMN_USAGE` AS KCU " +
            //    $"ON IC.`TABLE_SCHEMA` = KCU.`TABLE_SCHEMA` AND IC.`TABLE_NAME` = KCU.`TABLE_NAME` AND IC.`COLUMN_NAME` = KCU.`COLUMN_NAME` " +
            //    $"LEFT JOIN `{INFORMATION_SCHEMA}`.`STATISTICS` AS S " +
            //    $"ON IC.`TABLE_SCHEMA` = S.`TABLE_SCHEMA` AND IC.`TABLE_NAME` = S.`TABLE_NAME` AND IC.`COLUMN_NAME` = S.`COLUMN_NAME` " +
            //    $"WHERE IC.`TABLE_SCHEMA` = '{Setting.Database.ToLower()}' AND IC.`TABLE_NAME` = '{tableName}';";

            //List<FieldInfo> tableFields = Query(sql, (reader) =>
            //{
            //    string fieldType = reader.GetString("COLUMN_TYPE", null);
            //    return new FieldInfo()
            //    {
            //        IsKey = reader.GetInt32("IS_KEY") == 1,
            //        Name = reader.GetString("COLUMN_NAME"),
            //        Nullable = reader.GetInt32("NULLABLE", 0) == 1,
            //        IsIndex = reader.GetInt32("IS_INDEX", 0) == 1,
            //        DefaultValue = reader.GetString("COLUMN_DEFAULT", null),
            //        DbFieldType = fieldType,
            //        FieldType = ConvertFromString(fieldType),
            //    };
            //});

            //sql = null;
            //if (tableFields.Count == 0)
            #endregion

            //表不存在，就创建表
            if (!tableNames.Contains(tableName))
            {
                //创建表
                string fieldSqls = string.Join(", ", table.Fields.Values.Select(f => CreateGenerateSQL(f)));
                string keyFieldSqls = string.Join(", ", table.Fields.Values.Where(f => f.IsKey).Select(f => $"`{f.Name}`"));
                ITableFieldScheme[] indexs = table.Fields.Values.Where(f => f.IsIndex && !f.IsKey).ToArray();
                string indexSqls = indexs.Length > 0 ? $", {string.Join(", ", indexs.Select(f => $"INDEX `INDEX_{f.Name}` (`{f.Name}`)"))}" : "";
                sql = $"CREATE TABLE IF NOT EXISTS `{tableName}`({fieldSqls}, PRIMARY KEY ({keyFieldSqls}){indexSqls} )ENGINE=InnoDB DEFAULT CHARSET=utf8;";
                if (Query(sql) != 0)
                {
                    FConsole.WriteTo(LOG_LEVEL, Kind.ToString(), "[Query Error] There are some errors happened on command the sql -> \r\n{0}", sql);
                }
            }
            //表存在，判断列信息是否一致
            else
            {
                sql = $"DESC {tableName};";//读取表信息
                List<FieldInfo> tableFields = Query<List<FieldInfo>, FieldInfo>(sql, (reader) =>
                {
                    string fieldType = reader.GetString("Type", null);
                    return new FieldInfo()
                    {
                        IsKey = reader.GetString("Key", null) == "PRI",
                        Name = reader.GetString("Field"),
                        Nullable = reader.GetString("Null", "").ToLower() == "yes",
                        IsIndex = !string.IsNullOrWhiteSpace(reader.GetString("Key", null)),
                        DefaultValue = reader.GetString("Default", null),
                        DbFieldType = fieldType,
                        FieldType = ConvertFromString(fieldType),
                    };
                });

                //进行判断
                Dictionary<string, FieldInfo> dbFields = tableFields.ToDictionary(f => f.Name.ToLower(), f => f);
                List<TableFieldComparor> comparors = new List<TableFieldComparor>();
                foreach (ITableFieldScheme field in table.Fields.Values)
                {
                    FieldInfo fieldInfo = null;
                    if (dbFields.ContainsKey(field.Name.ToLower()))
                    {
                        fieldInfo = dbFields[field.Name.ToLower()];
                        dbFields.Remove(field.Name.ToLower());
                    }
                    comparors.Add(new TableFieldComparor(field, fieldInfo));
                }
                foreach (FieldInfo fieldInfo in dbFields.Values)
                {
                    comparors.Add(new TableFieldComparor(null, fieldInfo));
                }
                dbFields.Clear();

                //判断结构处理
                StringBuilder sqls = new StringBuilder();
                bool keyChanged = false;
                foreach (TableFieldComparor comparor in comparors)
                {
                    if (comparor.Operation != TableFieldComparor.EOperation.UnModified)
                    {
                        if (comparor.Operation == TableFieldComparor.EOperation.Deleted)
                        {
                            continue;
                        }
                        if (comparor.IsError)
                        {
                            throw new TypeAccessException($"Can't alter the table `{tableName}`, because the field['{comparor.FieldName}' fail to convert {comparor.DbFieldInfo.FieldType.ToString()} to {comparor.TableField.FieldType.ToString()}]");
                        }
                        if (comparor.Operation == TableFieldComparor.EOperation.ToInsert)
                        {
                            sqls.AppendLine($"ALTER TABLE `{tableName}` ADD {CreateGenerateSQL((ITableFieldScheme)comparor.TableField)};");
                        }
                        else if (!comparor.OnlyIsIndexChanged && !comparor.OnlyIsKeyChanged)
                        {
                            sqls.AppendLine($"ALTER TABLE `{tableName}` MODIFY {CreateGenerateSQL((ITableFieldScheme)comparor.TableField)}");
                        }
                        if ((comparor.TableField.IsIndex && comparor.Operation == TableFieldComparor.EOperation.ToInsert)
                            || (comparor.DbFieldInfo != null && comparor.TableField.IsIndex != comparor.DbFieldInfo.IsIndex))
                        {
                            if (comparor.TableField.IsIndex)
                            {
                                sqls.AppendLine($"ALTER TABLE `{tableName}` ADD INDEX `INDEX_{comparor.TableField.Name}`(`{comparor.TableField.Name}`)");
                            }
                        }
                        if ((comparor.TableField.IsKey && comparor.Operation == TableFieldComparor.EOperation.ToInsert)
                            || (comparor.DbFieldInfo != null && comparor.TableField.IsKey != comparor.DbFieldInfo.IsKey))
                        {
                            keyChanged = true;
                        }
                    }
                }

                if (keyChanged)
                {
                    sqls.AppendLine($"ALTER TABLE `{tableName}` DROP PRIMARY KEY;");
                    sqls.AppendLine($"ALTER TABLE `{tableName}` ADD PRIMARY KEY ({string.Join(", ", table.Fields.Values.Where(f => f.IsKey).Select(f => $"`{f.Name}`"))});");
                }

                if (sqls.Length > 0)
                {
                    sql = sqls.ToString();// string.Join("; ", sqls);
                    if (Query(sql) != 0)
                    {
                        FConsole.WriteTo(LOG_LEVEL, Kind.ToString(), "[Query Error] There are some errors happened on command the sql -> \r\n{0}", sql);
                    }
                }
            }
        }
        #endregion
        /// <summary>
        /// 通过MySqlDataReader解析数据
        /// </summary>
        private TSet Query<TSet, T>(string sql, Func<MySqlDataReader, T> deserialize, IEnumerable<KeyValuePair<string, object>> parameters = null) where TSet : ICollection<T>, new()
        {
            FConsole.WriteTo(LOG_LEVEL, Kind.ToString(), sql);
            TSet items = new TSet();
            using (MySqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        MySqlParameter parameter = command.CreateParameter();
                        parameter.ParameterName = param.Key;
                        parameter.Value = param.Value;
                    }
                }
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
        private int Query(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            FConsole.WriteTo(LOG_LEVEL, Kind.ToString(), @sql);
            using (MySqlCommand command = Connection.CreateCommand())
            {
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        MySqlParameter parameter = command.CreateParameter();
                        parameter.ParameterName = param.Key;
                        parameter.Value = param.Value;
                    }
                }
                return command.ExecuteNonQuery();
            }
        }

        private static string CreateGenerateSQL(ITableFieldScheme tableField)
        {
            ETableFieldType type = tableField.FieldType;//== ?TableFieldConverter.ConvertFromType(tableField.Type.PropertyType);
            string sql = $"`{tableField.Name}` {ConvertToString(tableField, type)}";
            if (tableField.IsKey || !tableField.Nullable)
            {
                sql += " NOT NULL";
            }
            if (type == ETableFieldType.String || type == ETableFieldType.Text || type == ETableFieldType.LongText)
            {
                if (tableField.DefaultValue != null)
                {
                    sql += $" DEFAULT '{tableField.DefaultValue}'";
                }
            }
            //else if (!tableField.DefaultValue.Equals(FType.GetDefaultValueFromType(tableField.Type.PropertyType)))
            //{
            //    sql += $" default {tableField.DefaultValue}";
            //}
            else if (tableField.DefaultValue != null)
            {
                sql += $" DEFAULT {tableField.DefaultValue}";
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
            foreach (DbCommition commition in commitions)
            {
                CommitionPool[CommitionPoolIndex].Enqueue(commition);
            }
            return true;
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
        /// <summary>
        /// 执行修改操作
        /// </summary>
        public bool GenerateModifySql(DbCommition commition)
        {
            TableScheme tableScheme = (TableScheme)DataContext.TableSchemes[commition.Key.Type];
            Dictionary<string, string> keys = new Dictionary<string, string>(tableScheme.KeyFields.Count);
            IReadOnlyList<ITableFieldScheme> keyFields = tableScheme.KeyFields;
            for (int i = 0; i < keyFields.Count; i++)
            {
                keys.Add(keyFields[i].Name, commition.Key.Keys[i].GetString());
            }
            //Where判断sql
            string keySql = string.Join(" AND ", keys.Select(kv => $"`{kv.Key}` = '{kv.Value}'"));
            //删除逻辑
            if (commition.ModifyType == EModifyType.Remove || commition.Entity == null)
            {
                Query($"DELETE FROM `{tableScheme.TableName}` WHERE {keySql};");
                return true;
            }
            //生成修改逻辑有二进制会动到@
            Dictionary<string, object> parameters = new Dictionary<string, object>(tableScheme.Fields.Count);
            Dictionary<string, string> values = new Dictionary<string, string>(tableScheme.Fields.Count);
            foreach (ITableFieldScheme field in tableScheme.Fields.Values)
            {
                //获取当前的值
                object value = field.Type.GetValue(commition.Entity);
                if (field.Type.PropertyType.IsSubclassOf(FType<PropertyEntity>.Type))
                {
                    values.Add(field.Name, $"'{JsonUtility.ToJson(value)}'");
                }
                else if (field.FieldType == ETableFieldType.Blob || field.FieldType == ETableFieldType.LongBlob)
                {
                    values.Add(field.Name, $"@{field.PropertyName}");
                    parameters.Add(field.Name, field.PropertyName);
                }
                else if (field.FieldType == ETableFieldType.String || field.FieldType == ETableFieldType.Text || field.FieldType == ETableFieldType.LongText
                    || field.FieldType == ETableFieldType.DateTime)
                {
                    values.Add(field.Name, $"'{value.ToString()}'");
                }
                else
                {
                    values.Add(field.Name, $"{value.ToString()}");
                }
            }
            string keysSql = string.Join(", ", values.Keys.Select(n => $"`{n}`"));
            string valuesSql = string.Join(", ", values.Values);
            string equalsSql = string.Join(", ", values.Where(kv => !keys.ContainsKey(kv.Key)).Select(kv => $"`{kv.Key}` = {kv.Value}"));

            Query($"INSERT INTO `{tableScheme.TableName}` ({keysSql}) VALUES ({valuesSql}) ON DUPLICATE KEY UPDATE {equalsSql};", parameters);

            return true;
        }
    }
}
