using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Common
{
    /// <summary>
    /// Type缓存类，防止多次调用GetType或者typeof
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class FType<T>
    {
        /// <summary>
        /// 获取泛型的Type
        /// </summary>
        public static Type Type { get; private set; } = typeof(T);
    }
    /// <summary>
    /// Type的一些操作
    /// </summary>
    public static class FType
    {
        /// <summary>
        /// 根据Type，获取默认值
        /// </summary>
        public static object GetDefaultValueFromType(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
