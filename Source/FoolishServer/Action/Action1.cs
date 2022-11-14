using System;
using System.Collections.Generic;
using System.Text;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishServer.Action;

namespace FoolishServer
{
    public class Action1 : ServerAction
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
