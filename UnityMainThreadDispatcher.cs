using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Führt Aktionen sicher auf dem Unity-Mainthread aus.
/// Das ist wichtig, wenn du etwas in einem Hintergrundthread berechnest (z.B. Speech-Ergebnisse)
/// und danach GameObjects oder UI-Elemente aktualisieren willst.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();
    private static UnityMainThreadDispatcher instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("MainThreadDispatcher");
            instance = obj.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(obj); // bleibt zwischen Szenen bestehen
        }
    }

    /// <summary>
    /// Wird von einem beliebigen Thread aufgerufen,
    /// um eine Aktion auf dem Hauptthread auszuführen.
    /// </summary>
    public static void Enqueue(Action action)
    {
        lock (actions)
            actions.Enqueue(action);
    }

    void Update()
    {
        // Führe alle gesammelten Aktionen aus
        while (actions.Count > 0)
        {
            Action a;
            lock (actions)
                a = actions.Dequeue();
            a?.Invoke();
        }
    }
}
