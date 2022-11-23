using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using FoolishServer.Log;

namespace FoolishServer.Data
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum EDatabase
    {
        Unknow,
        MySQL,
        SQLServer,
        Redis,
    }

    /// <summary>
    /// 数据库操作
    /// </summary>
    public abstract class Database : ISQLDatabase
    {
        /// <summary>
        /// 是什么类型的数据库
        /// </summary>
        public EDatabase Kind { get { return Setting.Kind; } }
        /// <summary>
        /// 配置信息
        /// </summary>
        public IDatabaseSetting Setting { get; private set; }

        /// <summary>
        /// 判断连接状态
        /// </summary>
        public bool Connected { get { return Connection.State == System.Data.ConnectionState.Connecting || Connection.State == System.Data.ConnectionState.Executing; } }

        /// <summary>
        /// 连接器
        /// </summary>
        public DbConnection Connection { get; private set; }

        /// <summary>
        /// 先建立连接
        /// </summary>
        public void CreateConnection(IDatabaseSetting setting)
        {
            if (setting == null)
            {
                throw new ArgumentNullException("Can not create connection, because the setting is null.");
            }
            Setting = setting;
            Connection = CreateDbConnection(setting);
        }

        /// <summary>
        /// 创建连接对象
        /// </summary>
        protected abstract DbConnection CreateDbConnection(IDatabaseSetting setting);

        /// <summary>
        /// 对表结构进行调整或创建
        /// </summary>
        public abstract void GenerateOrUpdateTableScheme(ITableScheme table);

        /// <summary>
        /// 建立连接
        /// </summary>
        public void Close()
        {
            if (Connection != null)
            {
                try
                {
                    Connection.Close();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Kind.ToString(), e);
                }
                Connection = null;
            }
        }

        /// <summary>
        /// 开始连接
        /// </summary>
        public bool Connect()
        {
            FConsole.WriteInfoFormatWithCategory(Kind.ToString(), "{0}[{1}] is connecting...", Kind.ToString(), Setting.ConnectKey);
            Connection.Open();
            FConsole.WriteInfoFormatWithCategory(Kind.ToString(), "{0}[{1}] connected.", Kind.ToString(), Setting.ConnectKey);
            return true;
        }

        public bool CommitModifiedEntitys(IEnumerable<DbCommition> commitions)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> LoadAll<T>() where T : MajorEntity, new()
        {
            throw new NotImplementedException();
        }

        public T Find<T>(EntityKey key) where T : MajorEntity, new()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static ISQLDatabase Make(IDatabaseSetting setting)
        {
            switch (setting.Kind)
            {
                case EDatabase.MySQL: return new MySQLDatabase();
                default:
                    {
                        throw new NotSupportedException("The database of '" + setting.Kind.ToString() + "' is not realized.");
                    }
            }
        }
    }
}
