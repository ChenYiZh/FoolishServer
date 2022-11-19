using FoolishGames.Common;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
        /// 读取数据
        /// </summary>
        public static IEntitySet<T> GetEntity<T>() where T : MajorEntity, new()
        {
            return new EntitySet<T>();
        }
        /// <summary>
        /// 获取表名
        /// </summary>
        public static ITableScheme GetTableScheme(Type type)
        {
            return tableSchemes[type];
        }
        /// <summary>
        /// 初始化
        /// </summary>
        internal static void Initialize()
        {
            //生成表的映射关系
            Assembly assembly = AssemblyService.Assemblies.FirstOrDefault();
            Type majorType = Types.MajorEntity;
            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(majorType))
                    {
                        tableSchemes.Add(type, new TableScheme(type));
                    }
                }
            }

            //建立Redis连接
            Redis = new RedisDatabase(Settings.RedisSetting);
            Redis.Connect();
        }
        /// <summary>
        /// 退出时调用
        /// </summary>
        internal static void Shutdown()
        {
            if (Redis != null)
            {
                Redis.Close();
            }
        }
    }
}
