using FoolishGames.Log;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Data;
using FoolishServer.Data.Entity;
using FoolishServer.Model;
using FoolishServer.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        }

        public override void OnDatebaseInitialized()
        {
            base.OnDatebaseInitialized();
            DataContext.RawDatabase.Converter = new EntityJsonConverter();
        }

        public override void ReadyToStartServers()
        {
            base.ReadyToStartServers();
            //DataContext.RawDatabase.Converter = new EntityJsonConverter();
        }
        static int count = 0;
        public override void OnBegun()
        {
            base.OnBegun();
            FConsole.Write("RuntimeMain OnBegun.");

            //DataContext.PushAllRawData();
            //return;
            int number = 1000;
            Parallel.For(1, number, (index) =>
            {
                NewTest(index);
            });
            return;
            Parallel.For(1, number, (index) =>
            {
                var set = DataContext.GetEntity<Test>();
                Test a = set.Find(new Random((int)(DateTime.Now.Ticks % int.MaxValue)).Next(1, number), "Hello World!");
                //FConsole.Write(JsonUtility.ToJson(a));
                //a.UserName = "Hello World!";
                //Thread.Sleep(10000);
                a.Tests = null;
                //a.Tests.Remove(2);
                //a.Tests[1].TestId = "Fuck2";

                FConsole.Write(Interlocked.Decrement(ref count));
            });

        }

        private void NewTest(long id)
        {
            var set = DataContext.GetEntity<Test>();
            Test a = new Test();
            //a.OnPropertyModified += PropertyModified;
            a.UserId = id;
            a.UserName = "Hello World!";
            a.Tests = new Collections.EntityDictionary<int, Test2>();
            a.Tests.Add(1, new Test2() { TestId = "TestTest" });
            set.AddOrUpdate(a);
            //a.Tests.Add(2, new Test2() { TestId = "Test2" });
            //string json = JsonUtility.ToJson(a);
            //a.Password = "asdsad";

            FConsole.Write(Interlocked.Increment(ref count));
            //FConsole.Write("Time:" + b.ModifiedTime);
        }

        private void PropertyModified(MajorEntity sender, string propertyName, object oldValue, object value)
        {
            //FConsole.WriteFormat("{0}.{1}[{4}]: from {2}, to {3}.", sender.GetType().Name, propertyName, oldValue, value, sender.ModifiedTime);
        }
    }
}
