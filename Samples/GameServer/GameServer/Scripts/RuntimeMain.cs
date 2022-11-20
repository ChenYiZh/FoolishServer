using FoolishGames.Log;
using FoolishServer.Config;
using FoolishServer.Data;
using FoolishServer.Data.Entity;
using FoolishServer.Model;
using FoolishServer.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
            a.UserName = "Hello_World!";
            a.Tests = new Collections.EntityDictionary<int, Test2>();
            a.Tests.Add(1, new Test2() { TestId = "TestTest" });
            set.AddOrUpdate(a);

            Thread.Sleep(3000);
            a.Tests.Add(2, new Test2() { TestId = "Test2" });
            Thread.Sleep(3000);
            a.Password = "asdsad";
        }

        private void PropertyModified(MajorEntity sender, string propertyName, object oldValue, object value)
        {
            FConsole.WriteFormat("{0}.{1}[{4}]: from {2}, to {3}.", sender.GetType().Name, propertyName, oldValue, value, sender.ModifiedTime);
        }
    }
}
