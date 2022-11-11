using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Log
{
    /// <summary>
    /// 日志保存接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="message"></param>
        void SaveLog(string level, string message);
    }
}
