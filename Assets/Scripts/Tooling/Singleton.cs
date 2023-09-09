using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Singleton<T> where T : new()
{
    private static readonly T instance = new T();
 
    static Singleton()
    {
    }

    protected Singleton()
    {
    }

    protected static T Instance
    {
        get
        {
            return instance;
        }
    }
}

public class SingletonComponent<T> : MonoBehaviour where T : UnityEngine.MonoBehaviour
{
    private static T s_Instance = null;
    public static T Instance
    {
        get
        {
            if( s_Instance != null )
            {
                return s_Instance;
            }

            T[] instances = Resources.FindObjectsOfTypeAll<T>();
            if( instances != null )
            {
                // find the one that is actually in the scene (and not the editor)
                for( int i = 0; i < instances.Length; ++i )
                {
                    T instance = instances[ i ];
                    if( instance == null )
                        continue;

                    if( instance.hideFlags != HideFlags.None )
                        continue;

                    // avoid selecting component attached to a prefab asset that isn't in scene
                    if( string.IsNullOrEmpty(instance.gameObject.scene.name) )
                        continue;

                    s_Instance = instance;
                    DontDestroyOnLoad( s_Instance );
                    break;
                }
            }


            if( s_Instance == null )
            {
                string name = string.Format( "__{0}__", typeof( T ).FullName );
                GameObject singletonGo = new GameObject( name );
                s_Instance = singletonGo.AddComponent<T>();
                DontDestroyOnLoad( s_Instance );
            }
            return s_Instance;
        }
    }

    internal static bool IsInstantiated => s_Instance != null;

    protected virtual void Awake()
    {
        if(s_Instance == null)
        {
            s_Instance = Instance;
        }
    }
}