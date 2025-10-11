using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    [SerializeField] bool isDontDestroyOnLoad = false;
    private static T inst;

    public static T Inst
    {
        get
        {
            if (inst == null)
            {
                inst = FindObjectOfType<T>();

                if (inst == null)
                {
                    GameObject instObj = new(typeof(T).ToString());
                    inst = instObj.AddComponent<T>();
                }
            }

            return inst;
        }
    }

    public virtual void Awake()
    {
        if (inst == null)
        {
            inst = this as T;
            if (isDontDestroyOnLoad)
                DontDestroyOnLoad(inst);
        }
        else
        {
            if (inst != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
