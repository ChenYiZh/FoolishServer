using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Log
{
    internal class NLogger : ILogger
    {
        private NLog.Logger logger;

        public NLogger()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void SaveLog(string level, string message)
        {
            SetColor(GetColor(level));
            switch (level)
            {
                case LogLevel.DEBUG: logger.Debug(message); break;
                case LogLevel.INFO: logger.Info(message); break;
                case LogLevel.WARN: logger.Warn(message); break;
                case LogLevel.ERROR: logger.Error(message); break;
                default:
                    {
                        NLog.Logger customLogger = NLog.LogManager.GetLogger(level);
                        if (customLogger != null)
                        {
                            customLogger.Log(NLog.LogLevel.Trace, message);
                        }
                    }
                    break;
            }
            ResetColor();
        }

        private ConsoleColor GetColor(string level)
        {
            switch (level)
            {
                case LogLevel.ERROR: return ConsoleColor.Red;
                case LogLevel.WARN: return ConsoleColor.Yellow;
                case LogLevel.INFO: return ConsoleColor.Cyan;
                default: return ConsoleColor.White;
            }
        }

        private ConsoleColor lastConsoleColor;

        private void SetColor(ConsoleColor color)
        {
            try
            {
                lastConsoleColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
            }
            catch { }
        }

        private void ResetColor()
        {
            try
            {
                Console.ForegroundColor = lastConsoleColor;
            }
            catch { }
        }
    }
}
