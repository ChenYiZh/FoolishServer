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
        /// <summary>
        /// 判断是否实现某个泛型接口，少用
        /// </summary>
        public static bool IsSubInterfaceOf(this Type type, Type interfaceType)
        {
            if (type == null)
            {
                return false;
            }
            Type[] types = type.GetInterfaces();
            foreach (Type t in types)
            {
                if (t.IsGenericType ? t.GetGenericTypeDefinition() == interfaceType : t == interfaceType)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
