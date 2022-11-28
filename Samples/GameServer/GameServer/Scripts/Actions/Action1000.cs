using FoolishGames.IO;
using FoolishGames.Log;
using FoolishServer.Action;
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
        }
    }
}
