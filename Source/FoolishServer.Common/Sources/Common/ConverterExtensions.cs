using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Common
{
    internal static class ConverterExtensions
    {
        /// <summary>
        /// 自动判断空的ToString
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetString(this object obj)
        {
            if (obj == null) { return null; }
            return obj.ToString();
        }
    }
}
