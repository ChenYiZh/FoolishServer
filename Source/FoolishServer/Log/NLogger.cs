using FoolishServer.Framework.Log;
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
        }
    }
}
