using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FoolishGames.IO
{
    /// <summary>
    /// 路径管理类
    /// </summary>
    public static class FPath
    {
        /// <summary>
        /// 路径补全
        /// </summary>
        public static string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.CurrentDirectory;
            }
            else if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Environment.CurrentDirectory, path);
            }
            return path;
        }
    }
}
