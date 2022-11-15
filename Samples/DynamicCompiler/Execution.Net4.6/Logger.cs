using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution
{
    class Logger : ILogger
    {
        public void SaveLog(string level, string message)
        {
            SetColor(GetColor(level));
            Console.WriteLine(message);
            ResetColor();
        }

        private ConsoleColor GetColor(string level)
        {
            switch (level)
            {
                case LogLevel.ERROR: return ConsoleColor.Red;
                case LogLevel.WARN: return ConsoleColor.DarkYellow;
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
