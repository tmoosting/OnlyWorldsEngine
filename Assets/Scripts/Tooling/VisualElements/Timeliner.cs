using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

// Draws Timeline, places and handles EventIcons, handles Zoom and other interactions

 
public class Timeliner : VisualElement
{

    private Action<int> mouseMoveAction;
    
    private Timeline timeline;

    private List<VisualElement> verticalLines;
    
    // Timeline elements
    public VisualElement timelineElements; 
    public VisualElement verticalLinesHolder;

    
    public Timeliner(Action<int> mouseMoveAction)
    {
        this.mouseMoveAction = mouseMoveAction;
        Initialize(); 
    }

    private void Initialize()
    {
        this.style.backgroundColor = RootTimeline.backgroundColor;
 
        this.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
 
        this.style.alignItems = new StyleEnum<Align>(Align.Center); 
        this.style.justifyContent = new StyleEnum<Justify>(Justify.Center);  

        style.width = Length.Percent(100);
        style.height = Length.Percent(100);
         Utilities.ExecuteWithDelay(CreateTimeline);
         Utilities.ExecuteWithDelay(CreateTimelineElements);
     //   EditorApplication.delayCall += DelayedInitialization;
    
        RegisterCallback<MouseMoveEvent>(evt =>
        {
            mouseMoveAction.Invoke(GetTimestepForXCoord(evt.mousePosition.x));
        });
    }
 
    
    
    /*private float lastScaleX = 1f;
    public void ApplyScaling(Vector3 zoomScale)
    {
        lastScaleX = zoomScale.x;
        timelineElements.transform.scale = new Vector3(zoomScale.x, timelineElements.transform.scale.y, timelineElements.transform.scale.z);
  
        float newWidth = this.resolvedStyle.width * zoomScale.x;
        timelineElements.style.width = newWidth;

        foreach (var verticalLine in verticalLines)
            verticalLine.transform.scale  = new Vector3(  1 / zoomScale.x,   verticalLine.transform.scale.y,   verticalLine.transform.scale.z);
    }*/

    
    
    
    private void CreateTimeline()
    {
        if (timeline != null)
        {
            timeline.Clear();
            timeline.RemoveFromHierarchy();
        }
        timeline = new Timeline(RootControl,this);
        timeline.name = "timeline";
        timeline.style.width = Length.Percent(100);
        timeline.style.height = Length.Percent(100);
        timeline.style.position = Position.Absolute;

        this.Add(timeline);
    }

    private void CreateTimelineElements()
    {
        if (timelineElements != null)
        {
            timelineElements.Clear();
            timelineElements.RemoveFromHierarchy();
        }

        timelineElements = new VisualElement();
        timelineElements.name = "timelineElements"; 
        timelineElements.style.position = Position.Absolute; 
        timelineElements.style.width = Length.Percent(100);
        timelineElements.style.height = Length.Percent(100);
   //     timelineElements.transform.scale =  new Vector3( lastScaleX,  timelineElements.transform.scale.y,  timelineElements.transform.scale.z);

        CreateVerticalLines();
        
        verticalLinesHolder.style.marginTop = this.resolvedStyle.height / 2f;  
 

        this.Add(timelineElements); 

    }
    private void CreateVerticalLines()
    {
        if (verticalLinesHolder != null)
        {
            verticalLinesHolder.Clear();
            verticalLinesHolder.RemoveFromHierarchy();
        }

        verticalLines = new List<VisualElement>();
        verticalLinesHolder = new VisualElement();
        verticalLinesHolder.style.position = Position.Absolute;
        verticalLinesHolder.style.width = Length.Percent(100);
        verticalLinesHolder.style.height = Length.Percent(100);   

        int numLines = RootControl.RootTimeline.activeDividers;
        float totalWidth = this.resolvedStyle.width;

        for (int i = 0; i < numLines; i++)
        {
            var verticalLine = new VisualElement();
            verticalLine.style.width = new StyleLength(1f); // 1 pixel wide
            verticalLine.style.height = new StyleLength(RootTimeline.timeBarHeight); // 10 pixels tall, adjust as necessary
            verticalLine.style.backgroundColor = Color.black; // or your desired color
            verticalLine.style.position = Position.Absolute;

            // Calculate position for the line based on number of dividers
            float position = (i + 1) * (totalWidth / (numLines + 1));

            verticalLine.style.left = new StyleLength(position);
            verticalLine.style.top = new StyleLength(1f); // 1 pixel from the top to align with the center of the horizontal line
            verticalLines.Add(verticalLine);
            verticalLinesHolder.Add(verticalLine);
        }
        timelineElements.Add(verticalLinesHolder); 
    }


  

    public int GetTimestepForXCoord(float xCoord)
    {
        float stepsPerX =   (RootTimeline.GetTimeSteps() / this.resolvedStyle.width) ;
        
        return  (int)  (xCoord * stepsPerX) + RootTimeline.activeStart ;
    }
 
    #region Getters
    private MonoLoader _monoLoader;
    private MonoLoader MonoLoader
    {
        get
        {
            if (_monoLoader == null)
                _monoLoader = Object.FindObjectOfType<MonoLoader>(true);
            if (_monoLoader == null)
                Debug.LogWarning("! No MonoLoader GameObject found. Please re-load the tool from Launcher.");
            return _monoLoader;
        }
    }     
    private RootTimeline _rootTimeline;
    private RootTimeline RootTimeline
    {
        get
        {
            if (_rootTimeline == null)
                _rootTimeline = LoadRootTimeline();
            return _rootTimeline;
        }
    }
    private RootTimeline LoadRootTimeline()
    {
        RootTimeline rootTimeline =  AssetDatabase.LoadAssetAtPath<RootTimeline>(MonoLoader.projectPath + MonoLoader.rootPath + "RootTimeline.asset");
        if (rootTimeline == null)
            Debug.LogWarning("! No RootTimeline found. Please re-load the tool from Launcher.");
        return rootTimeline;
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
#endregion


}
