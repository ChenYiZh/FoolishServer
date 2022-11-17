using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Model;
using FoolishServer.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer
{
    public class RuntimeMain : CustomRuntime
    {
        public override void OnStartup()
        {
            FConsole.LogStackTracker = true;
            //FConsole.LogStackLevels.Add(LogLevel.WARN);
            base.OnStartup();
            FConsole.Write("RuntimeMain OnStartup: " + Settings.IsDebug);
            Test a = new Test();
            a.OnPropertyModified += PropertyModified;
            a.UserId = 12345678;
        }

        private void PropertyModified(Entity sender, string propertyName, object oldValue, object value)
        {
            FConsole.WriteError("{0}.{1}: from {2}, to {3}.", sender.GetType().Name, propertyName, oldValue, value);
        }
    }
}
