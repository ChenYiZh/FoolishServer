using FoolishClient.RPC;
using FoolishGames.IO;
using FoolishGames.Log;
using FoolishGames.Proxy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UpdateProxy), typeof(LogProxy))]
public class Startup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UpdateProxy proxy = GetComponent<UpdateProxy>();
        LogProxy logger = GetComponent<LogProxy>();

        FConsole.RegistLogger(logger);

        FNetwork.MakeTcpSocket("default", "127.0.0.1", 9001, "Action{0}").MessageEventProcessor = proxy;

        MessageWriter message = new MessageWriter();
        message.WriteString("Client Message: Hello World!");
        FNetwork.Send(1000, message);
    }

    private void OnApplicationQuit()
    {
        FNetwork.Shutdown();
    }
}
