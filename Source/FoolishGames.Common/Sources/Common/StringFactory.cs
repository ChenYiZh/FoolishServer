using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Common
{
    /// <summary>
    /// string转换
    /// </summary>
    public class StringFactory
    {
        /// <summary>
        /// 从Exception转成字符串
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string Make(Exception exception)
        {
            return string.Format("{0}\r\n{1}", exception.Message, exception.StackTrace);
        }

        /// <summary>
        /// 从Exception转成字符串
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string Make(string message, Exception exception)
        {
            return string.Format("{0}\r\n{1}\r\n{2}", message, exception.Message, exception.StackTrace);
        }
    }
}
