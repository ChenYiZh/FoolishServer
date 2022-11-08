﻿using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Framework.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace FoolishServer.Log
{
    public class GConsole : IGConsole
    {
        private HashSet<ILogger> loggers = null;

        public IReadOnlyCollection<ILogger> LoggerList { get { return loggers; } }
        /// <summary>
        /// 已注册的Logger
        /// </summary>
        public static IReadOnlyCollection<ILogger> Loggers { get { return Instance.LoggerList; } }

        private Object SyncRoot = new Object();

        private static GConsole instance = null;

        private static GConsole Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GConsole();
                }
                return instance;
            }
        }

        private GConsole()
        {
            SyncRoot = new Object();
            loggers = new HashSet<ILogger>();
        }
        ///// <summary>
        ///// 调用一下可以执行Instance
        ///// </summary>
        //internal static void Initialize()
        //{
        //    Instance.WriteInfoWithCategoryImpl(Categories.GCONSOLE, "Console initialized");
        //}

        /// <summary>
        /// 注册Logger
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static bool RegistLogger(ILogger logger)
        {
            return Instance.RegistLoggerImpl(logger);
        }

        public bool RegistLoggerImpl(ILogger logger)
        {
            bool result = false;
            lock (SyncRoot)
            {
                result = loggers.Add(logger);
            }
            return result;
        }

        /// <summary>
        /// 移除Logger
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static bool RemoveLogger(ILogger logger)
        {
            return Instance.RemoveLoggerImpl(logger);
        }

        public bool RemoveLoggerImpl(ILogger logger)
        {
            bool result = false;
            lock (SyncRoot)
            {
                result = loggers.Remove(logger);
            }
            return result;
        }
        /// <summary>
        /// 输出到Debug目录
        /// </summary>
        public static void Write(string message, params object[] args)
        {
            Instance.WriteImpl(message, args);
        }

        public void WriteImpl(string message, params object[] args)
        {
            WriteToImpl(LogLevel.DEBUG, Categories.LOG, message, args);
        }
        /// <summary>
        /// 输出到Debug目录
        /// </summary>
        public static void WriteWithCategory(string category, string message, params object[] args)
        {
            Instance.WriteWithCategoryImpl(category, message, args);
        }

        public void WriteWithCategoryImpl(string category, string message, params object[] args)
        {
            WriteToImpl(LogLevel.DEBUG, category, message, args);
        }
        /// <summary>
        /// 输出到Info目录
        /// </summary>
        public static void WriteInfo(string message, params object[] args)
        {
            instance.WriteInfoImpl(message, args);
        }

        public void WriteInfoImpl(string message, params object[] args)
        {
            WriteToImpl(LogLevel.INFO, Categories.LOG, message, args);
        }
        /// <summary>
        /// 输出到Info目录
        /// </summary>
        public static void WriteInfoWithCategory(string category, string message, params object[] args)
        {
            Instance.WriteInfoWithCategoryImpl(category, message, args);
        }

        public void WriteInfoWithCategoryImpl(string category, string message, params object[] args)
        {
            WriteToImpl(LogLevel.INFO, category, message, args);
        }
        /// <summary>
        /// 输出到Warn目录
        /// </summary>
        public static void WriteWarn(string message, params object[] args)
        {
            Instance.WriteWarnImpl(message, args);
        }

        public void WriteWarnImpl(string message, params object[] args)
        {
            WriteToImpl(LogLevel.WARN, Categories.LOG, message, args);
        }
        /// <summary>
        /// 指定类别输出到Warn目录
        /// </summary>
        public static void WriteWarnWithCategory(string category, string message, params object[] args)
        {
            Instance.WriteWarnWithCategoryImpl(category, message, args);
        }

        public void WriteWarnWithCategoryImpl(string category, string message, params object[] args)
        {
            WriteToImpl(LogLevel.WARN, category, message, args);
        }
        /// <summary>
        /// 输出到Exception目录
        /// </summary>
        public static void WriteError(string message, params object[] args)
        {
            Instance.WriteErrorImpl(message, args);
        }

        public void WriteErrorImpl(string message, params object[] args)
        {
            WriteToImpl(LogLevel.ERROR, Categories.LOG, message, args);
        }
        /// <summary>
        /// 输出到Exception目录
        /// </summary>
        public static void WriteErrorWithCategory(string category, string message, params object[] args)
        {
            Instance.WriteErrorWithCategoryImpl(category, message, args);
        }

        public void WriteErrorWithCategoryImpl(string category, string message, params object[] args)
        {
            WriteToImpl(LogLevel.ERROR, category, message, args);
        }
        /// <summary>
        /// 输出到Exception目录
        /// </summary>
        public static void WriteException(Exception exception)
        {
            Instance.WriteExceptionImpl(exception);
        }

        public void WriteExceptionImpl(Exception exception)
        {
            SendMessage(LogLevel.ERROR, Categories.LOG, StringFactory.Make(exception), false);
        }
        /// <summary>
        /// 输出到Exception目录
        /// </summary>
        public static void WriteExceptionWithCategory(string category, Exception exception)
        {
            Instance.WriteExceptionWithCategoryImpl(category, exception);
        }

        public void WriteExceptionWithCategoryImpl(string category, Exception exception)
        {
            SendMessage(LogLevel.ERROR, category, StringFactory.Make(exception), false);
        }
        /// <summary>
        /// 输出到指定类别
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="category">日志类别</param>
        /// <param name="message">日志内容</param>
        /// <param name="args"></param>
        public static void WriteTo(string level, string category, string message, params object[] args)
        {
            Instance.WriteToImpl(level, category, message, args);
        }

        public void WriteToImpl(string level, string category, string message, params object[] args)
        {
            SendMessage(level, category, string.Format(message, args), true);
        }

        private void SendMessage(string level, string category, string message, bool track)
        {
            message = $"{DateTime.Now.ToString()} [{category}] - " + message;
            if (Settings.IsDebug && track)
            {
                const string stackIndent = "  ";
                StackTrace stackTrace = new StackTrace(true);
                StringBuilder builder = new StringBuilder();
                int count = stackTrace.FrameCount;
                for (int i = 2; i < count; i++)
                {
                    StackFrame stackFrame = stackTrace.GetFrame(i);
                    MethodBase method = stackFrame.GetMethod();
                    string className = method.DeclaringType != null ? method.DeclaringType.FullName : "Unknow type";
                    string fileName = stackFrame.GetFileName();
                    builder.AppendFormat("{0} at {1}.{2}", stackIndent, className, method);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        builder.AppendFormat(" file {0}:line {1}", fileName, stackFrame.GetFileLineNumber());
                    }
                }
                message += "\r\n" + builder.ToString();
            }
            SendMessage(level, message);
        }
        /// <summary>
        /// 服务器内部输出
        /// </summary>
        internal static void WriteLine(string level, string category, string message)
        {
            message = $"{DateTime.Now.ToString()} [{category}] - " + message;
            WriteLine(level, message);
        }
        /// <summary>
        /// 服务器内部输出
        /// </summary>
        internal static void WriteLine(string level, string message)
        {
            Instance.SendMessage(level, message);
        }

        private void SendMessage(string level, string message)
        {
            message += "\r\n";
            foreach (ILogger logger in loggers)
            {
                logger.SaveLog(level, message);
            }
        }
    }
}
