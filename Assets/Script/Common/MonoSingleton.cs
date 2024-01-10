using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T: MonoSingleton<T>
{
    const string SingletonContainerName = "Singletons GameObject";
    private static readonly object padlock = new object();
    private static bool ApplicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (ApplicationIsQuitting)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogWarning("[Singleton] " + typeof(T) +
                                            " already destroyed on application quit." +
                                            " Won't create again - returning null.");
                }

                return null;
            }
            lock (padlock)
            {
                if (_instance == null) {
                    // 主要针对Editor需要调用的场景
                    var ins = GameObject.FindObjectOfType<T>();
                    if (ins == null)
                    {
                        ins = CreateToScene();
                    }
                    return ins;
                }
                return _instance;
            }
        }
    }

    private static T _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            Init();
        }
    }
    
    public virtual void Init()
    {

    }

    public void OnApplicationQuit()
    {
        ApplicationIsQuitting = true;
    }
    private static T CreateToScene()
    {
        var go = GameObject.Find(SingletonContainerName);
        if (go == null)
        {
            go = new GameObject(SingletonContainerName);
        }
        T t = go.AddComponent<T>();
        DontDestroyOnLoad(t);
        return t;
    }
}
