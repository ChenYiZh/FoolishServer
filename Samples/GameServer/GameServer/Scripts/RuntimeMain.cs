using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Data;
using FoolishServer.Data.Entity;
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
            var set = DataContext.GetEntity<Test>();
            Test a = new Test();
            a.OnPropertyModified += PropertyModified;
            a.UserId = 12345678;
            a.Tests = new Collections.EntityList<Test2>();
            set.AddOrUpdate(a);
            FConsole.Write(a.GetEntityId().ToString());
        }

        private void PropertyModified(MajorEntity sender, string propertyName, object oldValue, object value)
        {
            FConsole.Write("{0}.{1}: from {2}, to {3}.", sender.GetType().Name, propertyName, oldValue, value);
        }
    }
}
