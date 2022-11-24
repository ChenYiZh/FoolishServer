using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Common
{
    /// <summary>
    /// 字符串操作
    /// </summary>
    public static class StringConverter
    {
        /// <summary>
        /// 变小写，并且大写字母前加下划线
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToLowerWithDownLine(string str)
        {
            if (str == null) return null;
            if (str.Length == 1) return str.ToLower();
            string oriStr = str;
            string lowStr = str.ToLower();
            string result = "" + lowStr[0];
            for (int i = 1; i < lowStr.Length; i++)
            {
                char lc = lowStr[i];
                char oc = oriStr[i];
                //if (lc == oc)
                //{
                //    result += lc;
                //}
                //else 
                if (lc != oc && (lowStr[i - 1] == oriStr[i - 1] || (i < lowStr.Length - 1 && lowStr[i + 1] == oriStr[i + 1])))
                {
                    result += "_" + lc;
                }
                else
                {
                    result += lc;
                }
            }
            return result;
        }
    }
}
