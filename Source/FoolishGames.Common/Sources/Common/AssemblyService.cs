using FoolishGames.Collections;
using FoolishGames.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FoolishGames.Common
{
    /// <summary>
    /// 程序集管理类
    /// </summary>
    public static class AssemblyService
    {
        private static ThreadSafeDictionary<string, Assembly> assemblies = new ThreadSafeDictionary<string, Assembly>();
        /// <summary>
        /// 已加载的程序集
        /// </summary>
        public static IEnumerable<Assembly> Assemblies { get { return assemblies.Values; } }
        /// <summary>
        /// 读取程序集
        /// </summary>
        /// <param name="dllPath"></param>
        /// <returns></returns>
        public static Assembly Load(string dllPath)
        {
            if (!dllPath.EndsWith(".dll")) return null;
            string dllName = Path.GetFileNameWithoutExtension(dllPath);
            if (assemblies.ContainsKey(dllName))
            {
                return assemblies[dllName];
            }
            Assembly assembly = Assembly.LoadFile(FPath.GetFullPath(dllPath));
            if (assembly != null)
            {
                assemblies.Add(dllName, assembly);
            }
            return assembly;
        }
    }
}
