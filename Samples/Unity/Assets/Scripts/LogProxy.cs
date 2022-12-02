using FoolishGames.Collections;
using FoolishGames.Log;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogProxy : MonoBehaviour, FoolishGames.Log.ILogger
{
    private struct Logger { public string Level; public string Message; }

    ThreadSafeQueue<Logger> loggers = new ThreadSafeQueue<Logger>();

    public void SaveLog(string level, string message)
    {
        loggers.Enqueue(new Logger { Level = level, Message = message });
    }

    // Update is called once per frame
    void Update()
    {
        while (loggers.Count > 0)
        {
            Logger logger = loggers.Dequeue();
            switch (logger.Level)
            {
                case LogLevel.ERROR: Debug.LogError(logger.Message); break;
                case LogLevel.WARN: Debug.LogWarning(logger.Message); break;
                default: Debug.Log(logger.Message); break;
            }
        }
    }
}
