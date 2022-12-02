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
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishServer.Action;
using FoolishServer.Data;
using FoolishServer.Data.Entity;
using FoolishServer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishServer.Actions
{
    public class Action1000 : ServerAction
    {
        public override bool Check()
        {
            return true;
        }

        public override void TakeAction(IMessageReader reader)
        {
            FConsole.Write(reader.ReadString());
            IEntitySet<Test> set = DataContext.GetEntity<Test>();
            Test entity = set.Find(1000, "Hello World!");
            MessageWriter msg = new MessageWriter();
            msg.WriteString("Server Response: " + entity.UserName);
            Session.Send(1000, msg);
        }
    }
}
