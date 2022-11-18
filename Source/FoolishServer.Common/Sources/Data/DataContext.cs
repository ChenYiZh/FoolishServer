using FoolishGames.Common;
using FoolishServer.Common;
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

        private static Dictionary<Type, ITableScheme> tableSchemes = new Dictionary<Type, ITableScheme>();
        /// <summary>
        /// 表结构映射
        /// </summary>
        internal static IReadOnlyDictionary<Type, ITableScheme> TableSchemes { get; private set; }
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
            return TableSchemes[type];
        }

        internal static void Initialize()
        {
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
        }
    }
}
