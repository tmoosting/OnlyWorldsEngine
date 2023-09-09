using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Provides monobehaviour functionality and tool settings

public class MonoLoader : MonoBehaviour
{
    // hardcoded in: RootControl's VerifyEnvironment() and its WorldLoader getter; Launcher's MonoLoader getter; WorldLoader's GetWorldsFileNames(); EditorSessionManager's LoadRootControl()
    [Tooltip("Base folder for toolset. Use full path eg: Assets/ or Assets/OnlyWorlds/")]  
    public string projectPath = "Assets/"; 
    public static string staticProjectPath = "Assets/"; 
    [Tooltip("Path to Root ScriptableObjects. Use projectPath + rootPath for complete path")]  
    public string rootPath = "Resources/Root Files/";
    public static string staticRootPath = "Resources/Root Files/";
    [Tooltip("Path to .db Worlds subfolder")]  
    public string worldPath = "Resources/Worlds/";
    [Tooltip("Enable automatic saving active World to database file")]
    public bool intervalSaveEnabled = false;
    [Tooltip("Interval time in seconds")] 
    public int intervalTime = 300;

    public static MonoLoader Instance { get; private set; }

    private void Awake()
    {
        Instance = this; 
    }
}
