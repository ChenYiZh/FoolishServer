using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Framework.Log
{
    public interface IGConsole
    {
        /// <summary>
        /// 已注册的Logger
        /// </summary>
        IReadOnlyCollection<ILogger> LoggerList { get; }

        /// <summary>
        /// 注册Logger
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        bool RegistLoggerImpl(ILogger logger);

        /// <summary>
        /// 移除Logger
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        bool RemoveLoggerImpl(ILogger logger);

        /// <summary>
        /// 输出默认目录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteImpl(string message, params object[] args);

        /// <summary>
        /// 指定类别输出
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteWithCategoryImpl(string category, string message, params object[] args);

        /// <summary>
        /// 输出到Info目录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteInfoImpl(string message, params object[] args);

        /// <summary>
        /// 指定类别输出到Info目录
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteInfoWithCategoryImpl(string category, string message, params object[] args);

        /// <summary>
        /// 输出到Warn目录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteWarnImpl(string message, params object[] args);

        /// <summary>
        /// 指定类别输出到Warn目录
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteWarnWithCategoryImpl(string category, string message, params object[] args);

        /// <summary>
        /// 输出到Exception目录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteErrorImpl(string message, params object[] args);

        /// <summary>
        /// 指定类别输出到Exception目录
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void WriteErrorWithCategoryImpl(string category, string message, params object[] args);

        /// <summary>
        /// 输出到Exception目录
        /// </summary>
        /// <param name="exception"></param>
        void WriteExceptionImpl(Exception exception);

        /// <summary>
        /// 指定类别输出到Exception目录
        /// </summary>
        /// <param name="category"></param>
        /// <param name="exception"></param>
        void WriteExceptionWithCategoryImpl(string category, Exception exception);

        /// <summary>
        /// 输出到指定类别
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="category">日志类别</param>
        /// <param name="message">日志内容</param>
        /// <param name="args"></param>
        void WriteToImpl(string level, string category, string message, params object[] args);
    }
}
