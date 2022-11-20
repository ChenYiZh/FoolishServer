﻿using FoolishGames.Collections;
using FoolishGames.Common;
using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoolishGames.Reflection
{
    /// <summary>
    /// 反射创建对象
    /// </summary>
    public static class ObjectFactory
    {
        /// <summary>
        /// 缓存池
        /// </summary>
        private static IDictionary<string, Type> types = new ThreadSafeDictionary<string, Type>();

        /// <summary>
        /// 自动通过程序集搜索创建
        /// </summary>
        public static T Create<T>(string typeName, params object[] args) where T : class
        {
            try
            {
                Type type;
                if (!types.TryGetValue(typeName, out type))
                {
                    type = Type.GetType(typeName);
                    if (type != null)
                    {
                        types[typeName] = type;
                    }
                    else
                    {
                        foreach (Assembly assembly in AssemblyService.Assemblies)
                        {
                            type = assembly.GetType(typeName);
                            if (type != null)
                            {
                                types[typeName] = type;
                                break;
                            }
                        }
                        if (type == null)
                        {
                            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                            foreach (Assembly assembly in assemblies)
                            {
                                type = assembly.GetType(typeName);
                                if (type != null)
                                {
                                    types[typeName] = type;
                                    break;
                                }
                            }
                        }
                    }
                }
                return Activator.CreateInstance(type, args) as T;
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.REFLECTION, e);
                return null;
            }
        }

        /// <summary>
        /// 直接反射创建对象
        /// </summary>
        public static T CreateSimply<T>(string typeName, params object[] args) where T : class
        {
            try
            {
                Type type;
                if (!types.TryGetValue(typeName, out type))
                {
                    type = Type.GetType(typeName);
                    if (type != null)
                    {
                        types[typeName] = type;
                    }
                }
                return Activator.CreateInstance(type, args) as T;
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.REFLECTION, e);
                return null;
            }
        }

        /// <summary>
        /// 通过程序集创建对象
        /// </summary>
        public static T CreateForce<T>(Assembly assembly, string typeName, params object[] args) where T : class
        {
            if (assembly == null)
            {
                FConsole.WriteErrorFormatWithCategory(Categories.REFLECTION, "Create object failed, because assembly is null.");
                return null;
            }
            try
            {
                Type type;
                if (!types.TryGetValue(typeName, out type))
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        types[typeName] = type;
                    }
                }
                return Activator.CreateInstance(type, args) as T;
            }
            catch (Exception e)
            {
                FConsole.WriteExceptionWithCategory(Categories.REFLECTION, e);
                return null;
            }
        }
    }
}