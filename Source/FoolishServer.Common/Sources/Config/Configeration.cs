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
