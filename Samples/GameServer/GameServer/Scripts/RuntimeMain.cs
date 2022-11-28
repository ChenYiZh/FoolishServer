using FoolishGames.Common;
using FoolishGames.Log;
using FoolishServer.Collections;
using FoolishServer.Common;
using FoolishServer.Config;
using FoolishServer.Data;
using FoolishServer.Data.Entity;
using FoolishServer.Model;
using FoolishServer.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            FConsole.Write(FType<EntityDictionary<int, Test2>>.Type.IsSubInterfaceOf(typeof(IDictionary<,>)));
            return;
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var set = DataContext.GetEntity<Test>();
            stopwatch.Stop();
            FConsole.Write(stopwatch.Elapsed);

            //var result = set.Find(2, "Hello World!");
            //FConsole.Write("result: " + result.TestID);
            //result.Password = "Good Morning";
            //FConsole.Write("set count: " + set.Count());
            ////set.LoadAll();
            //set = DataContext.GetEntity<Test>();
            //FConsole.Write("set count: " + set.Find(t => t.Password.Equals("Good Morning")).Count);

            return;
            stopwatch.Restart();
            FConsole.WriteWarn(DateTime.Now);
            int number = 1000000;
            Parallel.For(0, number / 5000, (i) =>
             {
                 //var set = DataContext.GetEntity<Test>();
                 int startIndex = i * 5000;
                 for (int index = startIndex; index < startIndex + 5000; index++)
                 {
                     //var set = DataContext.GetEntity<Test>();
                     NewTest(set, index);
                     //if (index % 1000 == 0)
                     //{
                     //    Thread.Sleep(100);
                     //}
                 }
             });
            stopwatch.Stop();
            FConsole.Write(number + ": " + stopwatch.Elapsed);
            return;
            //Parallel.For(0, number, (index) =>
            //{
            //    NewTest(index);
            //});
            //return;
            //for (int i = 0; i < number; i++)
            //{
            //    int index = i;
            //    new Thread(() =>
            //    {
            //        var set = DataContext.GetEntity<Test>();
            //        long key = new Random((int)(DateTime.Now.Ticks % int.MaxValue)).Next(0, number - 1);
            //        Test a = set.Find(key);//, "Hello World!");
            //                               //FConsole.Write(JsonUtility.ToJson(a));
            //                               //a.UserName = "Hello World!";
            //                               //Thread.Sleep(10000);
            //        a.Tests = null;
            //        //a.Tests.Remove(2);
            //        //a.Tests[1].TestId = "Fuck2";

            //        FConsole.Write(Interlocked.Decrement(ref count));
            //    }).Start();
            //}

            //for (int i = 0; i < number; i++)
            //{
            //    int index = i;
            //    new Thread(() =>
            //    {
            //        NewTest(null, index);
            //    }).Start();
            //}
        }

        private void NewTest(IEntitySet<Test> set, long id)
        {
            if (set == null)
            {
                set = DataContext.GetEntity<Test>();
            }
            //var set = DataContext.GetEntity<Test>();
            Test a = new Test();
            //a.OnPropertyModified += PropertyModified;
            a.UserId = id;
            a.UserName = "Hello World!";
            a.Tests = new Collections.EntityDictionary<int, Test2>();
            a.Tests.Add(1, new Test2() { TestId = "TestTest" });
            set.AddOrUpdate(a);
            a.Tests.Add(2, new Test2() { TestId = "Test2" });
            //string json = JsonUtility.ToJson(a);
            a.Password = "asdsad";

            //FConsole.Write(Interlocked.Increment(ref count));
            //FConsole.Write("Time:" + b.ModifiedTime);
        }

        private void PropertyModified(MajorEntity sender, string propertyName, object oldValue, object value)
        {
            FConsole.WriteFormat("{0}.{1}[{4}]: from {2}, to {3}.", sender.GetType().Name, propertyName, oldValue, value, sender.ModifiedTime);
        }
    }
}
