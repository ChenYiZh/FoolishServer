using FoolishGames.Common;
using FoolishGames.Log;
using FoolishGames.Reflection;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using FoolishServer.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoolishServer.Data
{
    /// <summary>
    /// 数据读取
    /// </summary>
    public static class DataContext
    {
        /// <summary>
        /// Redis管理类
        /// </summary>
        public static IRawDatabase RawDatabase { get; set; }

        /// <summary>
        /// 连接着的数据库
        /// </summary>
        public static IReadOnlyDictionary<string, ISQLDatabase> Databases { get; private set; }

        /// <summary>
        /// 表结构
        /// </summary>
        private static Dictionary<Type, ITableScheme> tableSchemes = new Dictionary<Type, ITableScheme>();

        /// <summary>
        /// 表结构映射
        /// </summary>
        internal static IReadOnlyDictionary<Type, ITableScheme> TableSchemes { get { return tableSchemes; } }

        /// <summary>
        /// 缓存中的数据
        /// </summary>
        private static Dictionary<Type, IDbSet> entityPool = new Dictionary<Type, IDbSet>();

        /// <summary>
        /// 缓存中的数据
        /// </summary>
        internal static IReadOnlyDictionary<Type, IDbSet> EntityPool { get { return entityPool; } }

        /// <summary>
        /// 计时器间隔设为100ms
        /// </summary>
        private const int TIMER_INTERVAL = 100;

        /// <summary>
        /// 计时器
        /// </summary>
        private static Timer Timer;

        /// <summary>
        /// 读取数据
        /// </summary>
        public static IEntitySet<T> GetEntity<T>() where T : MajorEntity, new()
        {
            Type type = FType<T>.Type;
            if (!EntityPool.ContainsKey(type))
            {
                FConsole.WriteErrorFormatWithCategory(Categories.ENTITY, "There is no EntitySet type of {0}.", type.FullName);
                return null;
            }
            IDbSet<T> dbSet = (IDbSet<T>)EntityPool[type];
            return new EntitySet<T>(dbSet);
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        public static ITableScheme GetTableScheme(Type type)
        {
            if (tableSchemes.ContainsKey(type))
            {
                return tableSchemes[type];
            }
            return null;
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        public static ITableScheme GetTableScheme<T>() where T : MajorEntity, new()
        {
            return GetTableScheme(FType<T>.Type);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal static void Initialize()
        {
            //生成表的映射关系
            Assembly assembly = AssemblyService.Assemblies.FirstOrDefault();
            Type majorType = FType<MajorEntity>.Type;
            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(majorType) && type.GetCustomAttribute<EntityTableAttribute>() != null)
                    {
                        InitializeDataContainer(type);
                    }
                }
            }

            //建立Redis连接
            if (RawDatabase == null)
            {
                RawDatabase = new RedisDatabase(Settings.RedisSetting);
            }

            //建立数据库连接
            if (Databases == null)
            {
                Databases = new Dictionary<string, ISQLDatabase>();
            }
            IDictionary<string, ISQLDatabase> batabases = (IDictionary<string, ISQLDatabase>)Databases;
            foreach (KeyValuePair<string, IDatabaseSetting> setting in Settings.DatabaseSettings)
            {
                if (batabases.ContainsKey(setting.Key))
                {
                    FConsole.WriteWarnFormatWithCategory(Categories.FOOLISH_SERVER, "The connectkey: '{0}' is exists, and database connection will not create.", setting.Key);
                    continue;
                }
                batabases[setting.Key] = Database.Make(setting.Value);
                batabases[setting.Key].SetSettings(setting.Value);
            }
        }

        /// <summary>
        /// 检查数据库结构是否变更
        /// </summary>
        public static void CheckTableSchemes()
        {
            FConsole.WriteInfoFormatWithCategory(Categories.ENTITY, "Check whether tables in the dbs have been changed...");
            foreach (ITableScheme tableScheme in tableSchemes.Values)
            {
                ISQLDatabase database;
                if (Databases.TryGetValue(tableScheme.ConnectKey, out database))
                {
                    database.GenerateOrUpdateTableScheme(tableScheme);
                }
                else
                {
                    FConsole.WriteErrorFormat("No database of '{0}' exists.", tableScheme.ConnectKey);
                }
            }
        }

        /// <summary>
        /// 开始读取数据
        /// </summary>
        internal static void Start()
        {
            // Redis 连接
            RawDatabase.Connect();

            // 数据库连接
            foreach (KeyValuePair<string, ISQLDatabase> database in Databases)
            {
                database.Value.Connect();
            }

            //检查数据库结构是否变更
            CheckTableSchemes();

            //加载所有热数据
            FConsole.WriteInfoFormatWithCategory(Categories.ENTITY, "Start loading hot data...");
            Parallel.ForEach(entityPool.Values, (IDbSet dbSet) =>
            {
               dbSet.PullAllRawData();
            });

            //开启计时器
            Timer = new Timer(Tick, null, TIMER_INTERVAL, TIMER_INTERVAL);
        }

        /// <summary>
        /// 反射用的
        /// </summary>
        private static Type DbSetType = typeof(DbSet<>);
        /// <summary>
        /// 初始化数据容器
        /// </summary>
        /// <param name="type"></param>
        private static void InitializeDataContainer(Type type)
        {
            tableSchemes.Add(type, new TableScheme(type));
            Type dbSetType = DbSetType.MakeGenericType(type);
            IDbSet dbSet = (IDbSet)Activator.CreateInstance(dbSetType);
            entityPool.Add(type, dbSet);
        }

        /// <summary>
        /// 计时器事件
        /// </summary>
        private static void Tick(object sender)
        {
            //Parallel.ForEach(EntityPool.Values, (IDbSet dbSet) =>
            foreach (IDbSet dbSet in EntityPool.Values)
            {
                //FConsole.WriteWarn("Tick: ");
                try
                {
                    bool commit = false;
                    bool release = false;
                    lock (dbSet.SyncRoot)
                    {
                        dbSet.CommitCountdown -= TIMER_INTERVAL;
                        //是否需要提交修改
                        commit = dbSet.CommitCountdown <= 0;
                        if (commit)
                        {
                            dbSet.CommitCountdown = Settings.DataCommitInterval;
                        }
                        //是否需要释放冷数据
                        if (dbSet.ReleaseCountdown > 0)
                        {
                            dbSet.ReleaseCountdown -= TIMER_INTERVAL;
                            release = dbSet.ReleaseCountdown <= 0;
                        }
                    }
                    if (commit)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem((obj) =>
                        {
                            Thread.CurrentThread.Priority = ThreadPriority.Highest;
                            dbSet.CommitModifiedData();
                        }, null);
                    }
                    if (release)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem((obj) => { dbSet.ReleaseColdEntities(); }, null);
                    }
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
                }
            }
            //});
        }

        /// <summary>
        /// 退出时调用
        /// </summary>
        internal static void Shutdown()
        {
            //关闭计时器
            if (Timer != null)
            {
                try
                {
                    Timer.Dispose();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
                }
                Timer = null;
            }
            //推送数据
            Parallel.ForEach(EntityPool.Values, (IDbSet dbSet) =>
            {
                try
                {
                    dbSet.ForceCommitAllModifiedData();
                    dbSet.Release();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
                }
            });
            //强制Redis落地
            if (RawDatabase != null)
            {
                RawDatabase.Close();
            }
            foreach (ISQLDatabase database in Databases.Values)
            {
                database.Close();
            }
        }

        /// <summary>
        /// 将缓存的数据全部提交
        /// </summary>
        public static void PushAllRawData()
        {
            Parallel.ForEach(EntityPool.Values, (IDbSet dbSet) =>
            {
                try
                {
                    dbSet.PushAllRawData();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
                }
            });
        }
    }
}
