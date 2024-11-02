using FoolishGames.Collections;
using FoolishGames.Proxy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateProxy : MonoBehaviour, IBoss
{
    ThreadSafeQueue<IWorker> workers = new ThreadSafeQueue<IWorker>();

    public void CheckIn(IWorker worker)
    {
        workers.Enqueue(worker);
    }

    void Update()
    {
        while (workers.Count > 0)
        {
            IWorker worker = workers.Dequeue();
            worker.Work();
        }
    }
}
