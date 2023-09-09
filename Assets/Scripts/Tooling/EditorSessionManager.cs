using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EditorSessionManager  
{
    private static double nextSaveTime = 0;
    private static int saveTimeInterval = 300;

    static EditorSessionManager()
    {
        EditorApplication.update += Update;
        EditorApplication.quitting += OnEditorQuit;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        // Subscribe to other events as necessary
    }
    private static void Update()
    {
        if (EditorApplication.timeSinceStartup > nextSaveTime)
        {
            RootControl rootControl = LoadRootControl();
            if (rootControl != null)
                rootControl.IntervalBackup();
            
            nextSaveTime = EditorApplication.timeSinceStartup + saveTimeInterval;
        }
   
        
    }
    private static void OnBeforeAssemblyReload()
    {
        // Code to execute before a domain reload
    }

    private static void OnAfterAssemblyReload()
    {
        // Code to execute after a domain reload
    }
    private static void OnEditorQuit()
    {
        // Perform cleanup, backup, etc.
    }
    
    private static RootControl LoadRootControl()
    {
        RootControl rootControl = AssetDatabase.LoadAssetAtPath<RootControl>("Assets/Resources/Root Files/RootControl.asset");
        if (rootControl == null)
        {
            Debug.LogWarning("! No RootControl found. Please re-load the tool from Launcher.");
        }
        return rootControl;
    }
    private static WorldParser LoadWorldParser()
    {
        WorldParser worldParser = AssetDatabase.LoadAssetAtPath<WorldParser>("Assets/Resources/Root Files/WorldParser.asset");
        if (worldParser == null)
        {
            Debug.LogWarning("! No WorldParser found. Please re-load the tool from Launcher.");
        }
        return worldParser;
    }
    
}
