using FoolishClient.Action;
using FoolishGames.IO;
using FoolishGames.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Actions
{
    public class Action1000 : ClientAction
    {
        public override void TakeAction(IMessageReader reader)
        {
            FConsole.Write(reader.ReadString());
        }
    }
}
