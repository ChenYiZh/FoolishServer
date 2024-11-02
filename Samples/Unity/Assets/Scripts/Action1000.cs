using FoolishClient.Action;
using FoolishGames.IO;
using FoolishGames.Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action1000 : ClientAction
{
    public override void TakeAction(IMessageReader reader)
    {
        FConsole.Write("Message - " + reader.ReadString());
    }
}