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
        public static IDatabase Redis { get; private set; }

        /// <summary>
        /// 连接着的数据库
        /// </summary>
        public static IReadOnlyDictionary<string, IDatabase> Databases { get; private set; }

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
            IDbSet<T> dbSet = (IDbSet<T>)EntityPool[FType<T>.Type];
            return new EntitySet<T>(dbSet);
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        public static ITableScheme GetTableScheme(Type type)
        {
            return tableSchemes[type];
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
                    if (type.IsSubclassOf(majorType))
                    {
                        InitializeDataContainer(type);
                    }
                }
            }

            //建立Redis连接
            Redis = new RedisDatabase(Settings.RedisSetting);
            Redis.Connect();

            //开启计时器
            Timer = new Timer(Tick, null, TIMER_INTERVAL, TIMER_INTERVAL);
        }

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
        /// 读取热数据
        /// </summary>
        private static void ReadRawData()
        {

        }

        /// <summary>
        /// 计时器事件
        /// </summary>
        private static void Tick(object sender)
        {
            Parallel.ForEach(EntityPool.Values, (IDbSet dbSet) =>
            {
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
                        ThreadPool.QueueUserWorkItem((obj) => { dbSet.CommitModifiedData(); });
                    }
                    if (release)
                    {
                        ThreadPool.QueueUserWorkItem((obj) => { dbSet.ReleaseColdEntities(); });
                    }
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
                }
            });
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
                    dbSet.CommitModifiedData();
                }
                catch (Exception e)
                {
                    FConsole.WriteExceptionWithCategory(Categories.ENTITY, e);
                }
            });
            //强制Redis落地
            if (Redis != null)
            {
                Redis.Close();
            }
        }
    }
}
