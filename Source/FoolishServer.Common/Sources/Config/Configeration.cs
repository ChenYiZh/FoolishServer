﻿/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2030 ChenYiZh
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
using FoolishGames.Common;
using FoolishGames.Compiler.CSharp;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishServer.Data;
using FoolishServer.Log;
using FoolishServer.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FoolishServer.Config
{
    internal class Configeration
    {
        private static bool initialized = false;
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;

            //加载配置
            Settings.LoadFromFile();

            //打印启动信息
            RuntimeHost.PrintStartInfo();
            //return;
            //编译脚本
            if (Directory.Exists(FPath.GetFullPath(Settings.CSScriptsPath)))
            {
                FConsole.WriteInfoFormatWithCategory(Categories.FOOLISH_SERVER, "Compiling scripts...");
                if (!ScriptEngine.CompileByRoslyn(Settings.CSScriptsPath, Settings.IsDebug, Settings.AssemblyName))
                {
                    throw new Exception("There is some errors on compiling the scripts.");
                }
            }

            //IL代码注入
            FConsole.WriteInfoFormatWithCategory(Categories.FOOLISH_SERVER, "Checking models...");
            if (ILInjection.InjectEntityChangeEvent())
            {
                FConsole.WriteInfoFormatWithCategory(Categories.FOOLISH_SERVER, "Some models have been modified automatically.");
                //if (string.Equals(Path.GetFileName(Assembly.GetEntryAssembly().GetName().Name + ".dll"), Settings.AssemblyName, StringComparison.OrdinalIgnoreCase))
                //{
                //    RuntimeHost.Reboot();
                //}
            }

            //加载程序集
            Assembly assembly;
            if ((assembly = AssemblyService.Load(Settings.AssemblyName)) == null)
            {
                throw new Exception("Failed to load the assembly: " + Settings.AssemblyName);
            }

            FConsole.WriteInfoFormatWithCategory(Categories.FOOLISH_SERVER, "Ready to start servers...");
        }
    }
}
