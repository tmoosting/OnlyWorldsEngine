using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "RootTimeline", menuName = "ScriptableObjects/RootTimeline", order = 0)] 
public class RootTimeline : ScriptableObject
{



    [Header("Settings")] 
    public int defaultStart = 0;
    public int defaultEnd = 1000;
    public int defaultDividers = 50;
    public int defaultWorldTime = 0;
    public float defaultZoomLevel = 1f; // between 0 and 1, percentage of total timeSteps shown
    public float currentZoomLevel;
    public float zoomSpeed = 0.05f;
    public float zoomMin = 0.5f;
    public float zoomMax = 20f;
    public float timeBarHeight = 10f;
    [Header("Colors")] 
    public Color backgroundColor;
    public Color controlBarColor;


    [HideInInspector] public int activeStart;
    [HideInInspector] public int activeEnd;
    [HideInInspector] public int activeDividers;
    [HideInInspector] public int activeWorldTime;


    /// <summary>
    /// called when a new World is selected from TimelineWindow
    /// </summary>
    public void UpdateTimelineData()
    {
        // todo adjust to world db data
        activeStart = defaultStart;
        activeEnd = defaultEnd;
        activeDividers = defaultDividers;
        activeWorldTime = defaultWorldTime;
    }
    
    
    public int GetTimeSteps()
    {
        return activeEnd - activeStart;
    }
    
    
        
    private MonoLoader _monoLoader;
    private MonoLoader MonoLoader
    {
        get
        {
            if (_monoLoader == null)
                _monoLoader = FindObjectOfType<MonoLoader>(true);
            if (_monoLoader == null)
                Debug.LogWarning("! No MonoLoader GameObject found. Please re-load the tool from Launcher.");
            return _monoLoader;
        }
    }   
    private RootControl _rootControl;
    private RootControl RootControl
    {
        get
        {
            if (_rootControl == null)
                _rootControl = LoadRootControl();
            return _rootControl;
        }
    }
    private RootControl LoadRootControl()
    {
        RootControl rootControl =  AssetDatabase.LoadAssetAtPath<RootControl>(MonoLoader.projectPath + MonoLoader.rootPath + "RootControl.asset");
        if (rootControl == null)
            Debug.LogWarning("! No RootControl found. Please re-load the tool from Launcher.");
        return rootControl;
    }

}
