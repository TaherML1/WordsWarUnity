using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance;
    private Queue<Action> actionQueue = new Queue<Action>();

    public static MainThreadDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("MainThreadDispatcher");
                instance = go.AddComponent<MainThreadDispatcher>();
            }
            return instance;
        }
    }

    private void Update()
    {
        while (actionQueue.Count > 0)
        {
            Action action = null;
            lock (actionQueue)
            {
                action = actionQueue.Dequeue();
            }
            action?.Invoke();
        }
    }

    public void Enqueue(Action action)
    {
        lock (actionQueue)
        {
            actionQueue.Enqueue(action);
        }
    }
}
