/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2025 ChenYiZh
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
            //FConsole.LogStackLevels.Add(LogLevel.INFO);
            //FConsole.LogStackLevels.Add(LogLevel.DEBUG);
            base.OnStartup();
            FConsole.WriteWarn("RuntimeMain OnStartup: " + Settings.IsDebug);
            return;
        }

        public override void OnDatebaseInitialized()
        {
            base.OnDatebaseInitialized();
            DataContext.RawDatabase.Converter = new EntityJsonConverter();
            FConsole.Write("RuntimeMain OnDatebaseInitialized.");
        }

        public override void ReadyToStartServers()
        {
            base.ReadyToStartServers();
            FConsole.Write("RuntimeMain ReadyToStartServers.");
        }

        public override void OnBegun()
        {
            base.OnBegun();
            FConsole.Write("RuntimeMain OnBegun.");
            IEntitySet<Test> set = DataContext.GetEntity<Test>();
            Test a = new Test();
            a.UserId = 1000;
            a.UserName = "Hello World!";
            a.Tests = new EntityDictionary<int, Test2>();
            a.Tests.Add(1, new Test2() { TestId = "TestTest" });
            set.AddOrUpdate(a);
            a.Tests.Add(2, new Test2() { TestId = "Test2" });
            a.Password = "asdsad";
            set.AddOrUpdate(a);            
        }

        public override void OnShutdown()
        {
            base.OnShutdown();
            FConsole.Write("RuntimeMain OnShutdown.");
            DataContext.PushAllRawData();
        }

        public override void OnKilled()
        {
            base.OnKilled();
            FConsole.Write("RuntimeMain OnKilled.");
        }
    }
}
