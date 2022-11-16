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
            Settings.LoadFromFile();
            if (ILInjection.InjectEntityChangeEvent())
            {
                if (string.Equals(Path.GetFileName(Assembly.GetEntryAssembly().GetName().Name + ".dll"), Settings.AssemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    FConsole.WriteWarnWithCategory(Categories.FOOLISH_SERVER, "The models' scripts have been auto injected!");
                    RuntimeHost.Reboot();
                }
            }
        }
    }
}
