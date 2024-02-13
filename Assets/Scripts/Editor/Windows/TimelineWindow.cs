using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using World_Model;


// What is an Event? --> a Timestate, and one or more Elements, each with a list of properties and changes thereon (either new value, or both old and new)
// What is a Timestate?  --> an int value between 0 and ..100k? to customize as necessary into years/days/eons? 
// What is in TimelineWindow?
    // a zone that gets split into timeSteps zones from left to right
    // can be zoomed in/out. Max zoomout shows max RootTimeline.timeSteps extent
    //  click to add EventIcon, click events to drag them, like pins
    // Has a 'Current' or 'default' or such marker: the time that the world db is 'snaphotted' at

// Features:
   // Settings for min and max EventIcon size, depending on overall zoom sate 
   // A context window on EventIcon click

// To do:
//  create timelineElements in Timeliner; move vertical stripes there; zoom to rescale





public class TimelineWindow : EditorWindow
{ 

    
    #region Instancing
    private static TimelineWindow _instance;
    public static TimelineWindow Instance
    {
        get
        {
            if (_instance == null)
                _instance = GetWindow<TimelineWindow>("Timeline");
            return _instance;
        }
    }
    private void VerifyInstance()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Close();
    }
    [MenuItem("OnlyWorlds/Timeline Window")]
    public static void ShowWindow()
    {
        Instance.Show();
    }
    #endregion
    #region WindowFunctions
    
 
    private void OnEnable()
    {
        VerifyInstance();
        RefreshSubscribes(true);
      //  if (RootControl.World != null)
         BuildWindow();
    }

 

    private void RefreshSubscribes(bool subscribing)
    {
        if (subscribing)
        {
            RootControl.OnWorldChanged += RefreshWorld;
            RootControl.OnWindowRefresh += RefreshBlank;
            RootControl.OnTableChanged += RefreshTable;
            RootControl.OnElementChanged += RefreshElement;
         //   RootControl.OnFieldChanged += RefreshField;
            RootControl.OnPinChanged += RefreshPin;
            RootControl.OnMapChanged += RefreshMap;
            RootControl.OnTimestateChanged += RefreshTimestate;
        }
        else
        {
            RootControl.OnWorldChanged -= RefreshWorld;
            RootControl.OnWindowRefresh -= RefreshBlank;
            RootControl.OnTableChanged -= RefreshTable;
            RootControl.OnElementChanged -= RefreshElement;
       //     RootControl.OnFieldChanged -= RefreshField;
            RootControl.OnPinChanged -= RefreshPin;
            RootControl.OnMapChanged -= RefreshMap;
            RootControl.OnTimestateChanged -= RefreshTimestate;
        }
    }

    private void OnGUI()
    {
        /*
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            GUI.FocusControl(null);  
            Repaint();
        }
        */

    }
    private void OnFocus()
    {
        
    } 
    private void OnLostFocus()
    {
        
    }

    private void OnDisable()
    {
        RefreshSubscribes(false); 

    }

    private void OnDestroy()
    {
        RefreshSubscribes(false); 

    }
    #endregion 
    #region RefreshFunctions
    
    private void RefreshBlank()
    { 
        
    }  
    private void RefreshWorld(World newWorld)
    {
        RootTimeline.UpdateTimelineData();
        BuildWindow();
    }
    private void RefreshTable(Element.Category newCategory)
    { 

    }
    private void RefreshElement(Element newElement)
    {
        if (newElement == null)
        {
            RefreshLossElement();
            return;
        }

    } 
    private void RefreshField(Field newField)
    {
        if (newField == null)
        {
            RefreshLossField();
            return;
        }
    }
    private void RefreshMap(Map newMap)
    {
        if (newMap == null)
        {
            RefreshLossMap();
            return;
        }
    }
    private void RefreshTimestate(Timestate newTimestate)
    {
        if (newTimestate == null)
        {
            RefreshLossTimestate();
            return;
        }

        RefreshWindow();
    }
    private void RefreshPin(Pin newPin)
    {
        if (newPin == null)
        {
            RefreshLossPin();
            return;
        }
    }

    private void RefreshLossPin()
    {
         
    }

    private void RefreshLossElement()
    { 
        
    }
    private void RefreshLossField ()
    { 
        
    }   
    private void RefreshLossMap ()
    { 
        
    }  
    private void RefreshLossTimestate ()
    {
        RefreshWindow();
    }
    #endregion
     
    
    #region Window Building

    private VisualElement _timelineRoot;

    // ControlBar
    private VisualElement controlBar;
    private Label mousePosLabel;
    private Label worldTimeLabel;
    private int controlBarPartitionAmount = 8;
    private List<VisualElement> controlBarPartitions;
    private BlueTextField startTextField;
    private BlueTextField endTextField;
    private BlueTextField dividersTextField;
    
    // DragBar
    private VisualElement dragBar;

    // Timeliner
    private Timeliner Timeliner;

     


    private EventCallback<WheelEvent> _zoomCallback;

    
    private void BuildWindow()
    {
        rootVisualElement.Clear();
        
        CreateControlBar();
        CreateDragBar();
        _timelineRoot = new VisualElement
        {
            style =
            {
                width = Length.Percent(100),
                height = Length.Percent(100),
                justifyContent = new StyleEnum<Justify>(Justify.Center)
            }
        };
        _timelineRoot.name = "_timelineRoot";
        RefreshWindow();
        RegisterCallbacks();

        rootVisualElement.Add(controlBar);
        rootVisualElement.Add(dragBar);
        rootVisualElement.Add(_timelineRoot);
        

    }
    private void RefreshWindow()
    {
        isDragging = false;
        CreateTimeliner();
    }
    
    bool isDragging = false;
    float initialMouseX;
    float initialHandleX;

    private VisualElement dragHandle;
    private void CreateDragBar()
    {
        dragBar = new VisualElement();
        dragBar.style.width = Length.Percent(100);
        dragBar.style.height = 10;
        dragBar.style.backgroundColor = RootTimeline.backgroundColor;
        dragBar.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        dragBar.style.overflow = Overflow.Visible;
        dragHandle = new VisualElement();
        dragHandle.name = "dragHandle";
        dragHandle.style.width = 10f;
        dragHandle.style.height = 10f;
        dragHandle.style.backgroundColor = Color.black; 
        dragHandle.RegisterCallback<MouseDownEvent>(OnDragHandleMouseDown);
        rootVisualElement.RegisterCallback<MouseUpEvent>(OnDragHandleMouseUp);
        rootVisualElement.RegisterCallback<MouseMoveEvent>(OnDragBarMouseMove);
        dragBar.Add(dragHandle);
    }
    private void OnDragHandleMouseDown(MouseDownEvent evt)
    {
        isDragging = true;
        initialMouseX = evt.mousePosition.x;
        initialHandleX = dragHandle.resolvedStyle.left;
        evt.StopPropagation();
    }

    private void OnDragHandleMouseUp(MouseUpEvent evt)
    {
        isDragging = false;
        evt.StopPropagation();
    }

    private void OnDragBarMouseMove(MouseMoveEvent evt)
    {
        if (!isDragging) return;

        float deltaX = evt.mousePosition.x - initialMouseX;
        float newHandleX = initialHandleX + deltaX;

        // Ensure dragHandle does not move out of dragBar boundaries
        float maxHandleX = dragBar.resolvedStyle.width - dragHandle.resolvedStyle.width;
        newHandleX = Mathf.Clamp(newHandleX, 0, maxHandleX);

        dragHandle.style.left = newHandleX;
        worldTimeLabel.text = "World: " + Timeliner.GetTimestepForXCoord(newHandleX).ToString("0");
        RootTimeline.activeWorldTime = Timeliner.GetTimestepForXCoord(newHandleX);
        evt.StopPropagation();
    }


    /// <summary>
    ///  horizontal bar stretching the top of TimelineWindow
    /// </summary>
    private void CreateControlBar()
    {
        controlBar = new VisualElement();
        controlBar.style.width = Length.Percent(100);
        controlBar.style.height = Length.Percent(15);
        controlBar.style.backgroundColor = RootTimeline.controlBarColor;
        controlBar.style.borderBottomWidth = 2f;
        controlBar.style.borderBottomColor = Color.black;
        controlBar.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

        Utilities.ExecuteWithDelay(CreateControlBarContent); 
      
    }

  
    private void CreateControlBarContent()
    {
        // Create Partitions
        controlBarPartitions = new List<VisualElement>();
        for (int i = 0; i < controlBarPartitionAmount; i++)
        {
            VisualElement partition = new VisualElement();
            partition.style.height = Length.Percent(100);
            float width = controlBar.resolvedStyle.width / controlBarPartitionAmount;
            partition.style.width = width;
            controlBarPartitions.Add(partition);
        }
        foreach (var controlBarPartition in controlBarPartitions)
            controlBar.Add(controlBarPartition);
        
        // Start Input 
        int startValue = 0;
        Label startLabel = new Label("Start");
        startLabel.style.color = Color.white;
        startTextField = new BlueTextField(RootControl);
        startTextField.style.width = Length.Percent(35);
        startTextField.SetValueWithoutNotify(RootTimeline.activeStart.ToString());
        startTextField.RegisterValueChangedCallback(evt =>
        {
            if (string.IsNullOrEmpty(startTextField.value))
                return;
            if (int.TryParse(startTextField.value, out startValue) == false)
            {
                startTextField.SetValueWithoutNotify("0");
            }
            else
            {
                RootTimeline.activeStart = startValue; 
                RefreshWindow();
            } 
        }); 
        controlBarPartitions[0].Add(startLabel);
        controlBarPartitions[0].Add(startTextField);//todo add proper delay and check to avoid index error, or make custom number textfield altogether

        int dividersValue = 0;
        Label dividerLabel = new Label("Markers");
        dividerLabel.style.color = Color.white;
        dividersTextField = new BlueTextField(RootControl);
        dividersTextField.style.width = Length.Percent(35);
        dividersTextField.SetValueWithoutNotify(RootTimeline.activeDividers.ToString());
        dividersTextField.RegisterValueChangedCallback(evt =>
        {
            if (string.IsNullOrEmpty(dividersTextField.value))
                return;
            if (int.TryParse(dividersTextField.value, out dividersValue) == false)
            {
                dividersTextField.SetValueWithoutNotify("0");
            }
            else
            {
                RootTimeline.activeDividers = dividersValue; 
                RefreshWindow();
            } 
        }); 
        controlBarPartitions[1].Add(dividerLabel);
        controlBarPartitions[1].Add(dividersTextField); 
        
        // Mouse Feedback
        mousePosLabel = new Label("Mouse: ");
        worldTimeLabel = new Label("World: ");
        worldTimeLabel.text = "World: " + RootTimeline.activeWorldTime;
        mousePosLabel.style.color = Color.white;
        worldTimeLabel.style.color = Color.white;
        controlBarPartitions[2].Add(mousePosLabel);
        controlBarPartitions[2].Add(worldTimeLabel);
        
        // Start Input 
        int endValue = 0;
        Label endLabel = new Label("End");
        endLabel.style.color = Color.white;
        endTextField = new BlueTextField(RootControl);
        endTextField.style.width = Length.Percent(35);
        endTextField.SetValueWithoutNotify(RootTimeline.activeEnd.ToString());
        endTextField.RegisterValueChangedCallback(evt =>
        {
            if (string.IsNullOrEmpty(endTextField.value))
                return;
            if (int.TryParse(endTextField.value, out endValue) == false)
            {
                endTextField.SetValueWithoutNotify("0");
            }
            else
            {
                RootTimeline.activeEnd = endValue;
                RefreshWindow();
            } 
        });
        controlBarPartitions[controlBarPartitionAmount - 1].style.alignItems = new StyleEnum<Align>(Align.FlexEnd);
        controlBarPartitions[controlBarPartitionAmount-1].Add(endLabel);
        controlBarPartitions[controlBarPartitionAmount-1].Add(endTextField);

    }

    private void CreateTimeliner()
    { 
        if (Timeliner != null)
        {
            Timeliner.Clear();
            Timeliner.RemoveFromHierarchy();
            Timeliner = null;
        }
        Timeliner = new Timeliner(OnMouseMoveInTimeliner);
        Timeliner.name = "Timeliner";

        _timelineRoot.Add(Timeliner);
    }
    private void RegisterCallbacks()
    {
        
        /*if (_zoomCallback != null)
            _timelineRoot.UnregisterCallback(_zoomCallback);
        _zoomCallback = (e) => MouseScrollInTimeliner(e);
        _timelineRoot.RegisterCallback(_zoomCallback);*/
        
        
    }
  

    private void OnMouseMoveInTimeliner(int xValue)
    {
        if (mousePosLabel != null)
        {
            mousePosLabel.text = "Mouse: " + xValue.ToString("0");
        }
    }

     
    /*
    private Vector3 _zoomScale;
    private void MouseScrollInTimeliner(WheelEvent e)
    { 
        float zoomSpeed = RootTimeline.zoomSpeed;
        float minScaleLog = Mathf.Log(RootTimeline.zoomMin);  // logarithmic scale min
        float maxScaleLog = Mathf.Log(RootTimeline.zoomMax);  // logarithmic scale max

        // Get the current scale in logarithmic form
        float currentScaleLog = Mathf.Log( Timeliner.timelineElements.transform.scale.x );

        // Calculate the new scale in logarithmic form
        float newScaleLog = currentScaleLog - e.delta.y * zoomSpeed;

        // Clamp the new scale
        newScaleLog = Mathf.Clamp(newScaleLog, minScaleLog, maxScaleLog);

        _zoomScale = new Vector3(Mathf.Exp(newScaleLog), Mathf.Exp(newScaleLog), Mathf.Exp(newScaleLog)); 
          
        Timeliner.ApplyScaling(_zoomScale);
        e.StopPropagation();
    }
    */
     
    
    
    
    #endregion
    
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
}
