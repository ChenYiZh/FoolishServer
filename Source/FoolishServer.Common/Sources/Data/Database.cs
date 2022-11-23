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
        /// <summary>
        /// 不在识别范围的数据库
        /// </summary>
        Unknow,
        /// <summary>
        /// MySQL
        /// </summary>
        MySQL,
        /// <summary>
        /// SQLServer
        /// </summary>
        SQLServer,
        /// <summary>
        /// Redis
        /// </summary>
        Redis,
    }

    /// <summary>
    /// 数据库操作
    /// </summary>
    public abstract class Database : ISQLDatabase
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public const string LOG_LEVEL = "SQL";

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
        public virtual bool Connected
        {
            get
            {
                DbConnection connection = GetConnection();
                return connection.State == System.Data.ConnectionState.Connecting || connection.State == System.Data.ConnectionState.Executing;
            }
        }

        /// <summary>
        /// 连接器
        /// </summary>
        public abstract DbConnection GetConnection();

        /// <summary>
        /// 先建立连接
        /// </summary>
        public virtual void CreateConnection(IDatabaseSetting setting)
        {
            if (setting == null)
            {
                throw new ArgumentNullException("Can not create connection, because the setting is null.");
            }
            Setting = setting;
        }

        /// <summary>
        /// 对表结构进行调整或创建
        /// </summary>
        public abstract void GenerateOrUpdateTableScheme(ITableScheme table);

        /// <summary>
        /// 断开连接
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 开始连接
        /// </summary>
        public bool Connect()
        {
            FConsole.WriteInfoFormatWithCategory(Kind.ToString(), "{0}[{1}] is connecting...", Kind.ToString(), Setting.ConnectKey);
            GetConnection().Open();
            FConsole.WriteInfoFormatWithCategory(Kind.ToString(), "{0}[{1}] connected.", Kind.ToString(), Setting.ConnectKey);
            return true;
        }

        /// <summary>
        /// 操作一堆数据
        /// </summary>
        public abstract bool CommitModifiedEntitys(IEnumerable<DbCommition> commitions);

        /// <summary>
        /// 读取表中所有数据
        /// </summary>
        public abstract IEnumerable<T> LoadAll<T>() where T : MajorEntity, new();

        /// <summary>
        /// 通过EntityKey，查询某一条数据，没有就返回空
        /// </summary>
        public abstract T Find<T>(EntityKey key) where T : MajorEntity, new();

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

    /// <summary>
    /// 数据库操作
    /// </summary>
    public abstract class Database<T> : Database, ISQLDatabase<T> where T : DbConnection
    {
        /// <summary>
        /// 连接器
        /// </summary>
        public sealed override DbConnection GetConnection()
        {
            return Connection;
        }

        /// <summary>
        /// 连接器
        /// </summary>
        public T Connection { get; private set; }

        /// <summary>
        /// 先建立连接
        /// </summary>
        public sealed override void CreateConnection(IDatabaseSetting setting)
        {
            base.CreateConnection(setting);
            Connection = CreateDbConnection(setting);
        }

        /// <summary>
        /// 创建连接对象
        /// </summary>
        protected abstract T CreateDbConnection(IDatabaseSetting setting);

        /// <summary>
        /// 断开连接
        /// </summary>
        public override void Close()
        {
            if (GetConnection() != null)
            {
                try
                {
                    GetConnection().Close();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Kind.ToString(), e);
                }
                Connection = null;
            }
        }
    }
}
