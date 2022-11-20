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
}
