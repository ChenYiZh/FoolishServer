using FoolishServer.Log;
using System;
using System.Collections.Generic;
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
        }
    }
}
