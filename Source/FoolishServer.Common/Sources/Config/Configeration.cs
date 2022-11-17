using FoolishGames.Compiler.CSharp;
using FoolishGames.IO;
using FoolishGames.Log;
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
                FConsole.WriteInfoWithCategory(Categories.FOOLISH_SERVER, "Compiling scripts...");
                if (!ScriptEngine.CompileByRoslyn(Settings.CSScriptsPath, Settings.IsDebug, Settings.AssemblyName))
                {
                    throw new Exception("There is some errors on compiling the scripts.");
                }
            }

            //IL代码注入
            FConsole.WriteInfoWithCategory(Categories.FOOLISH_SERVER, "Checking models...");
            if (ILInjection.InjectEntityChangeEvent())
            {
                FConsole.WriteInfoWithCategory(Categories.FOOLISH_SERVER, "Some models have been modified automatically.");
                //if (string.Equals(Path.GetFileName(Assembly.GetEntryAssembly().GetName().Name + ".dll"), Settings.AssemblyName, StringComparison.OrdinalIgnoreCase))
                //{
                //    RuntimeHost.Reboot();
                //}
            }

            //加载程序集
            Assembly assembly;
            if ((assembly = LoadAssembly()) == null)
            {
                throw new Exception("Failed to load the assembly: " + Settings.AssemblyName);
            }

            FConsole.WriteInfoWithCategory(Categories.FOOLISH_SERVER, "Ready to start servers...");
        }

        private static Assembly LoadAssembly()
        {
            string dllPath = FPath.GetFullPath(Settings.AssemblyName);
            return Assembly.LoadFrom(dllPath);
            string pdbFile = dllPath.Substring(0, dllPath.Length - 4) + ".pdb";
            if (!File.Exists(dllPath))
            {
                return null;
            }
            if (Settings.IsRelease)
            {
                return Assembly.LoadFile(dllPath);
            }
            byte[] dll = File.ReadAllBytes(dllPath);
            byte[] pdb = null;
            if (File.Exists(pdbFile))
            {
                pdb = File.ReadAllBytes(pdbFile);
            }
            if (pdb == null)
            {
                return Assembly.LoadFile(dllPath);
            }
            return Assembly.Load(dll, pdb);
        }
    }
}
