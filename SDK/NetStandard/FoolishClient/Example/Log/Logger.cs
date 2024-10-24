﻿/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/
using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Log
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
