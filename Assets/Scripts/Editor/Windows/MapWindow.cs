using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using World_Model;

// Controls
// left click to select pin, or (on empty) create temp pin (and deselect any pin)
// right click to toggle pinwindow, to cancel temp pin, to cancel any pin list highlight
// mid mouse to pan
// scroll to zoom



// Structure:
// - rootVisualElement
//   - _mapRoot (general root)
//      - _mapScaleContainer (preserves scaling of the map it contains)

public class MapWindow : EditorWindow
{

    //MapWindow Elements
    private VisualElement _mapRoot;
    private MapScaleContainer _mapScaleContainer;

    // ControlWindow Elements
    private VisualElement controlWindow;
    private VisualElement controlWindowContent;
    private WhiteButton buttonAddMap;
    private SquareDropdown dropdownFieldChooseMap;
    private Label mapDropdownLabel;
    
    private VisualElement switchContent;
    private VisualElement addMapContent;
    private VisualElement existingMapContent;
    private BlueTextField mapPathField;
    private BlueTextField mapNameField;
    private WhiteButton mapSaveButton;
    private WhiteButton mapStoreButton;
    private Button expandButton;
    private SquareDropdown mapTypeDropdown;
    private SquareDropdown mapParentDropdown;
    private ColorField colorPicker;
    private TextField descriptionField;

 
    // Pins
    private PinSubWindow pinSubWindow;

    private Sprite pinSprite => RootMap.GetPinBaseSprite();


    #region Instancing
    private static MapWindow _instance;
    public static MapWindow Instance
    {
        get
        {
            if (_instance == null)
                _instance = GetWindow<MapWindow>("Map");
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
    [MenuItem("OnlyWorlds/Map Window")]
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
        BuildWindow();
    }
    private void OnGUI()
    {
       
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

    #region Refreshing
    private void RefreshSubscribes(bool subscribing)
    {
        if (subscribing)
        {
            RootControl.OnWorldChanged += RefreshWorld;
            RootControl.OnWindowRefresh += RefreshBlank;
            RootControl.OnTableChanged += RefreshTable;
            RootControl.OnElementChanged += RefreshElement;
       //     RootControl.OnFieldChanged += RefreshField;
            RootControl.OnPinChanged += RefreshPin;
            RootControl.OnMapChanged += RefreshMap;
            RootControl.OnTimestateChanged += RefreshTimestate;
            RootMap.OnPinRefresh += IntegratePinElements;
            RootMap.OnPinCentering += CenterOnPinElement;
        }
        else
        {
            RootControl.OnWorldChanged -= RefreshWorld;
            RootControl.OnWindowRefresh -= RefreshBlank;
            RootControl.OnTableChanged -= RefreshTable;
            RootControl.OnElementChanged -= RefreshElement;
        //    RootControl.OnFieldChanged -= RefreshField;
            RootControl.OnPinChanged -= RefreshPin;
            RootControl.OnMapChanged -= RefreshMap;
            RootControl.OnTimestateChanged -= RefreshTimestate;
            RootMap.OnPinRefresh -= IntegratePinElements;
            RootMap.OnPinCentering -= CenterOnPinElement;

        }
    }

  

    private void RefreshBlank()
    {
        BuildWindow();
    }  
    private void RefreshWorld(World newWorld)
    {
        if (newWorld == null)
            return;
        BuildWindow();
    }
    private void RefreshTable(Element.Table newTable)
    { 

    }
    private void RefreshElement(Element newElement)
    {
        if (newElement == null)
        {
            RefreshLossElement();
            return;
        }

        if (RootMap.selectPinOnElementSelect)
        {
            bool pinFound = RootMap.DoesElementHavePinOnMap(newElement);

            if (pinFound)
            {
                RootControl.SetPin(RootMap.GetPinForElement(newElement));
            }
            else
            {
                if (tempPin == null)
                { 
                    RootControl.SetPin(null, false); 
                    if (pinSubWindow.isExtended)
                       pinSubWindow.SetMapScreenContainer(true);
                }
                else
                { 
                }
            }

        }
        
        if (pinSubWindow.tempPinElement == null)
               RootMap.RebuildPinElements();  
        
        if (pinSubWindow != null)
           pinSubWindow.NewElementSelected();
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
        BuildWindow();
    }
    private void RefreshTimestate(Timestate newTimestate)
    {
        if (newTimestate == null)
        {
            RefreshLossTimestate();
            return;
        }
    }
    
    private void RefreshPin(Pin newPin)
    {
        if (newPin == null)
        {
            RefreshLossPin();
            return;
        }
        RootMap.ResetHighlightMode();
        BuildPinWindow(true);
    }

    private void RefreshLossPin()
    { 
        pinSubWindow.RefreshWindow();
    }

    private void RefreshLossElement()
    { 
        if (RootControl.Pin != null)
            RootControl.SetPin(null, true);
        if (pinSubWindow != null) 
            pinSubWindow.ElementDeselected();
    }
    private void RefreshLossField ()
    { 
        
    }   
    private void RefreshLossMap ()
    {
        BuildWindow();
    }  
    private void RefreshLossTimestate ()
    { 
        
    }
    #endregion
    
    #region Window Building
 
    
    /// <summary>
    ///  rebuilds the entire map window. 
    /// </summary>
private void BuildWindow()
{
    rootVisualElement.Clear();
    _isPanning = false;
    justClickedPinElement = false;
    isDragging = false;
    RootMap.ResetSettings();
    RootControl.SetPin(null, true);
    
    _mapRoot = new VisualElement
    {
        style =
        {
            width = Length.Percent(100),
            height = Length.Percent(100)
        }
    };

    _zoomScale = Vector3.one;
    // Create the MapScaleContainer with the size of the map image

  
    Texture2D mapTexture =  RootMap.GetActiveMapTexture();
    _mapScaleContainer = new MapScaleContainer(new Vector2(mapTexture.width, mapTexture.height));
    _mapRoot.Add(_mapScaleContainer);
    rootVisualElement.Add(_mapRoot);
    LoadMapImage(); 
    BuildPinWindow();
    BuildControlWindow();
    UpdateMapWindowCallbacks();
    EditorCoroutineUtility.StartCoroutineOwnerless(LoadPinElementsAfterDelay());
}

    private IEnumerator LoadPinElementsAfterDelay()
    {
        yield return new EditorWaitForSeconds(0.02f);
        LoadPinElements(); 
    }

    private void LoadPinElements()
{
    if (RootControl.World == null)
        return;
    RootMap.RebuildPinElements();
}

private void IntegratePinElements()
{ 
    if (tempPin != null)
    {
        tempPin.RemoveFromHierarchy();
        tempPin = null;
    }
    foreach (var pinElement in RootMap.activePinElements)
    {
        pinElement.SetPositionForExistingPin(GetEffectiveScale());
        pinElement.SetSprite(pinSprite);
        pinElement.ApplyScaleFromZoom(_zoomScale, GetEffectiveScale()); 
        _mapScaleContainer.Add(pinElement);
    } 
}
 
private void BuildPinWindow(bool buildForActivePin = false)
{
    if (RootControl.World == null)
        return;
    if (pinSubWindow == null)
         pinSubWindow = new PinSubWindow();
    if (buildForActivePin)
        pinSubWindow.RefreshWindow(true, null, RootControl.Pin);
    else
        pinSubWindow.RefreshWindow();
    rootVisualElement.Add(pinSubWindow);
}

private Vector2 GetEffectiveScale()
{
    float aspectRatio = _mapScaleContainer.CurrentSize.x / _mapScaleContainer.CurrentSize.y;
    float containerWidth = _mapRoot.contentRect.width;
    float containerHeight = _mapRoot.contentRect.height;
    float width, height;  

    if (aspectRatio > (containerWidth / containerHeight))
    {
        width = containerWidth;
        height = width / aspectRatio;
    }
    else
    {
        height = containerHeight;
        width = height * aspectRatio;
    }

    float resizeScaleX = width / _mapScaleContainer.CurrentSize.x;
    float resizeScaleY = height / _mapScaleContainer.CurrentSize.y;
 
  //  return new Vector2(_zoomScale.x * resizeScaleX, _zoomScale.y * resizeScaleY);
    return new Vector2(  resizeScaleX,   resizeScaleY);
}

private void ResizeMapContainer()
{
    if (_isPanning)
        return;
    float aspectRatio = _mapScaleContainer.CurrentSize.x / _mapScaleContainer.CurrentSize.y;
    float containerWidth = _mapRoot.contentRect.width;
    float containerHeight = _mapRoot.contentRect.height;
    
    float width, height;

    // Compare aspect ratio of the map and the container
    if (aspectRatio > (containerWidth / containerHeight))
    {
        // Width is the limiting factor
        width = containerWidth;
        height = width / aspectRatio;
    }
    else
    {
        // Height is the limiting factor
        height = containerHeight;
        width = height * aspectRatio;
    }

    _mapScaleContainer.style.width = new Length(width, LengthUnit.Pixel);
    _mapScaleContainer.style.height = new Length(height, LengthUnit.Pixel); 
    
    // Calculate the position for centering the map.
    float xPosition = (containerWidth - width) / 2;
    float yPosition = (containerHeight - height) / 2;

    _mapScaleContainer.style.left = xPosition;
    _mapScaleContainer.style.top = yPosition;
    
    UpdatePinPositions();
}
public void UpdatePinPositions()
{
    float newMapWidth = _mapScaleContainer.contentRect.width;
    float newMapHeight = _mapScaleContainer.contentRect.height;

    foreach (var pinElement in RootMap.activePinElements)
    {
        pinElement.xCoord = pinElement.relativeX * newMapWidth;
        pinElement.yCoord = pinElement.relativeY * newMapHeight;
        pinElement.style.left = pinElement.xCoord;
        pinElement.style.top = pinElement.yCoord;
    }
}
private void UpdateMapScaleContainerSize()
{
    // Update the base size of the MapScaleContainer according to the size of the map texture
    Texture2D mapTexture = RootMap.GetActiveMapTexture();
    _mapScaleContainer._baseSize = new Vector2(mapTexture.width, mapTexture.height);

    // After the base size change, it's necessary to resize the container again to fit it into the parent
    ResizeMapContainer();
}

private EventCallback<KeyDownEvent> _keyPressCallback;
private EventCallback<MouseDownEvent> _mouseDownClickCallback; 
private EventCallback<MouseUpEvent> _mouseUpClickCallback; 
//private EventCallback<MouseDownEvent> _rightClickCallback;
private EventCallback<GeometryChangedEvent> _resizeCallback;
private EventCallback<WheelEvent> _zoomCallback;
private EventCallback<MouseDownEvent> _startPanCallback;
private EventCallback<MouseMoveEvent> _panCallback;
private EventCallback<MouseUpEvent> _endPanCallback;
private EventCallback<GeometryChangedEvent> _geometryChangedCallback;

private Vector2 _startMousePosition;
private Vector2 _startMapPosition;
private bool _isPanning;

private void UpdateMapWindowCallbacks()
{
    if (_keyPressCallback != null)
        rootVisualElement.UnregisterCallback(_keyPressCallback);  
    
    if (_mouseDownClickCallback != null)
        _mapScaleContainer.UnregisterCallback(_mouseDownClickCallback);   
    
    if (_mouseUpClickCallback != null)
        _mapScaleContainer.UnregisterCallback(_mouseUpClickCallback); 
    
   // if (_rightClickCallback != null)
    //    _mapScaleContainer.UnregisterCallback(_rightClickCallback);
    
    if (_resizeCallback != null)
        _mapRoot.UnregisterCallback(_resizeCallback);

    if (_zoomCallback != null)
        _mapScaleContainer.UnregisterCallback(_zoomCallback);
    
    if (_startPanCallback != null)
        _mapScaleContainer.UnregisterCallback(_startPanCallback);

    if (_panCallback != null)
        _mapScaleContainer.UnregisterCallback(_panCallback);
    
    if (_endPanCallback != null)
        _mapScaleContainer.UnregisterCallback(_endPanCallback);
    
    if (_geometryChangedCallback != null)
        _mapRoot.UnregisterCallback(_geometryChangedCallback);

    rootVisualElement.focusable = true;
    _keyPressCallback = (e) => PressKey(e);
    rootVisualElement.RegisterCallback(_keyPressCallback);
    
    _resizeCallback = (e) => ResizeMapContainer();
    _mapScaleContainer.RegisterCallback(_resizeCallback);

    _geometryChangedCallback = (e) => UpdateMapScaleContainerSize();
    _mapRoot.RegisterCallback(_geometryChangedCallback);

    _mouseUpClickCallback = (e) => ClickMapMouseUp(e);
    _mapScaleContainer.RegisterCallback(_mouseUpClickCallback,TrickleDown.TrickleDown);   
    
    _mouseDownClickCallback = (e) => ClickMapMouseDown(e);
    _mapScaleContainer.RegisterCallback(_mouseDownClickCallback,TrickleDown.TrickleDown);  
    
    _zoomCallback = (e) => ZoomMap(e);
    _mapScaleContainer.RegisterCallback(_zoomCallback);

    _startPanCallback = (e) => StartPan(e);
    _mapScaleContainer.RegisterCallback(_startPanCallback);

    _panCallback = (e) => MoveMouse(e);
    _mapScaleContainer.RegisterCallback(_panCallback);

    _endPanCallback = (e) => EndPan(e);
    _mapScaleContainer.RegisterCallback(_endPanCallback);
}
     
private void PressKey(KeyDownEvent evt)
{
    if (evt.keyCode == KeyCode.Escape)
    {
        pinSubWindow.ReceiveEscapeKey();
    }
}

private void RightClickMap()
{
    // Close Add Map process if it's open
    if (addMapContainerOpen)
        ClickAddMap();
    
    if (RootMap.highlightMode)
        RootMap.ResetHighlightMode(true);
    else
        pinSubWindow.ReceiveRightClick();
    
    // Remove temp pin if one exists
    if (tempPin != null)
        if (_mapScaleContainer.Contains(tempPin))
            _mapScaleContainer.Remove(tempPin);  
}

private bool justClickedPinElement = false;
private bool isDragging = false;    
private PinElement draggedElement;
private Vector2 lastMousePosition;
private void ClickMapMouseUp(MouseUpEvent e)
{ 
    if (dragEnableCoroutine != null)
    {
        EditorCoroutineUtility.StopCoroutine(dragEnableCoroutine);
        dragEnableCoroutine = null;
    }

    justClickedPinElement = false;
    if (RootControl.Map == null)
        return;
    
    if (isDragging)
    {
        draggedElement.EndPinDrag();
        isDragging = false;
        return;
    }
    
    if (e.button == (int)MouseButton.LeftMouse)
    {  
        Vector2 mousePosition = _mapScaleContainer.WorldToLocal(e.mousePosition);
        if (ClickWasOnPin())
            MouseUpOnPinElement(RootMap.hoveredPinElement);
        else
            CreateTempPin(mousePosition);
    }
    else if (e.button == (int)MouseButton.RightMouse)
        RightClickMap();
}

private void ClickMapMouseDown(MouseDownEvent e)
{ 
    if (ClickWasOnPin())
    { 
        dragEnableCoroutine =   EditorCoroutineUtility.StartCoroutineOwnerless(EnableDragAfterDelay());
        draggedElement = RootMap.hoveredPinElement;
        lastMousePosition = e.mousePosition;
    }
    else
    {
        justClickedPinElement = false;
    }
}
private EditorCoroutine dragEnableCoroutine;
private IEnumerator EnableDragAfterDelay()
{
    yield return new EditorWaitForSeconds(0.05f); 
    justClickedPinElement = true;
}

private void MouseUpOnPinElement(PinElement pinElement)
{ 
    if (isDragging)
    {
        isDragging = false; 
        draggedElement.EndPinDrag();
        return;
    }
    ClickPin(pinElement);
    pinSubWindow.ClickToggleExtendButton(true); 
}
private bool ClickWasOnPin( )
{
    if (RootMap.hoveredPinElement == null) // todo-check: use if (e.target is PinElement) instead?
        return false;
    else
        return true;
}


private PinElement tempPin;
private void CreateTempPin(Vector2 mousePosition)
{ 
    RootControl.SetPin(null, true); 
    RootMap.ResetHighlightMode(true);
    if (tempPin != null)
        if (_mapScaleContainer.Contains(tempPin))
             _mapScaleContainer.Remove(tempPin);
    tempPin = new PinElement(mousePosition, pinSprite,  _zoomScale, GetEffectiveScale());
    _mapScaleContainer.Add(tempPin);
    pinSubWindow.ClickToggleExtendButton(true);
    pinSubWindow.RefreshWindow(true, tempPin);
}

 
    private void ClickPin(PinElement clickedPinElement)
    { 
        clickedPinElement.ClickPinElement();
        RootMap.LeftClickPin(RootMap.GetPinForPinElement(clickedPinElement));
    }

 

private void StartPan(MouseDownEvent e)
{
   
        

    if (e.button == (int)MouseButton.MiddleMouse)
    {
        _isPanning = true;
        _startMousePosition = e.mousePosition;
        _startMapPosition = _mapRoot.transform.position;
    }
}

//todo-fix: combine threshold with delay for optimal results
private const float DRAG_THRESHOLD = 5.0f; // Threshold in pixels
private Vector2? initialDragPosition = null; // Nullable Vector2 to store the start position

private void MoveMouse(MouseMoveEvent e)
{
    Vector2 mousePosition = e.mousePosition;

    if (justClickedPinElement)
    { 
        if (initialDragPosition == null)
        {
            initialDragPosition = mousePosition; // Store the initial position of the mouse when the click occurred
        }

        float distanceMoved = Vector2.Distance((Vector2)initialDragPosition, mousePosition);

        if (distanceMoved >= DRAG_THRESHOLD)
        {
            isDragging = true;
            initialDragPosition = null; // Reset the initial position since dragging has started
        }
    }

    if (isDragging)
    {
        Vector2 delta = mousePosition - lastMousePosition; 

        draggedElement.DragPinElement(e.mousePosition, delta);
        lastMousePosition = mousePosition;
        return;
    }
    if (_isPanning)
    {
        Vector2 diff = e.mousePosition - _startMousePosition;
        _startMousePosition = e.mousePosition;
        _mapScaleContainer.Pan(diff);
    }
}


private void EndPan(MouseUpEvent e)
{
    if (e.button == (int)MouseButton.MiddleMouse)
        _isPanning = false;
}

    private Vector3 _zoomScale;
    private void ZoomMap(WheelEvent e)
    { 
        float zoomSpeed = RootMap.zoomSpeed;
        float minScaleLog = Mathf.Log(RootMap.zoomMin);  // logarithmic scale min
        float maxScaleLog = Mathf.Log(RootMap.zoomMax);  // logarithmic scale max

        // Get the current scale in logarithmic form
        float currentScaleLog = Mathf.Log(_mapRoot.transform.scale.x);

        // Calculate the new scale in logarithmic form
        float newScaleLog = currentScaleLog - e.delta.y * zoomSpeed;

        // Clamp the new scale
        newScaleLog = Mathf.Clamp(newScaleLog, minScaleLog, maxScaleLog);

        // Convert the new scale back to linear form and apply it
          _zoomScale = new Vector3(Mathf.Exp(newScaleLog), Mathf.Exp(newScaleLog), Mathf.Exp(newScaleLog)); 
        _mapRoot.transform.scale = _zoomScale;
        
        if (RootMap.constantPinScale)
            if (RootMap.activePinElements != null)
                foreach (var rootMapActivePinElement in RootMap.activePinElements)
                    rootMapActivePinElement.ApplyScaleFromZoom(_zoomScale, GetEffectiveScale());

    }
 
    private void LoadMapImage()
    {
        Texture2D mapTexture = RootMap.GetActiveMapTexture();
        if (mapTexture != null)
            _mapScaleContainer.style.backgroundImage = new StyleBackground(mapTexture);
        else
            Debug.LogError("Failed to load map texture.");
    }
  
    #endregion

    #region ControlWindow

    private bool existingMapsPresent = false;
    private bool addMapContainerOpen = false;
    private bool expandToggled = false;
    /// <summary>
    /// creates a floating window for some essential map controls
    /// </summary>
    private void BuildControlWindow()
    {
        if (RootControl.World == null)
            return;
        addMapContainerOpen = false;
        
        BuildControlBase();
        RefreshControlWindow();
        
        rootVisualElement.Add(controlWindow);
        RegisterControlWindowCallbacks();
    }

    private void RefreshControlWindow()
    {
//        Debug.Log("Refresh Control Window");
        float normalSize = 152;
        float expandSize = 290;
        addMapContainerOpen = false;
        existingMapsPresent = false;
        controlWindow.Clear(); 
        
        if (expandToggled == false)
            controlWindow.style.height = normalSize;
        else
            controlWindow.style.height = expandSize;
        
        BuildControlContent();
        BuildControlHeaderSection();
        BuildControlMapDropdown(); // set existingMapsPresent value here
        
        if (existingMapsPresent)
            BuildExistingMapsContent();

        controlWindowContent.Add(switchContent);
        controlWindow.Add(controlWindowContent);


        if (RootControl.Map != null)
            rootVisualElement.style.backgroundColor =  RootMap.HexStringToColor( RootControl.Map.BackgroundColor);
    }
    
    private Vector2 controlWindowPosition = Vector2.zero;
    private void BuildControlBase()
    {
        
        controlWindow = new VisualElement();
        controlWindow.style.backgroundColor = new StyleColor(new Color(0.25f, 0.3f, 0.4f));
        controlWindow.style.width = 200;
        controlWindow.style.position = Position.Absolute;
        controlWindow.style.left = RootMap.pinWindowWidth - 1f; ;
        controlWindow.style.top = 0;
        controlWindow.style.borderTopColor = Color.black;
        controlWindow.style.borderTopWidth = 1f;
        controlWindow.style.borderBottomColor = Color.black;
        controlWindow.style.borderBottomWidth = 1f;
        controlWindow.style.borderLeftColor = Color.black;
        controlWindow.style.borderLeftWidth = 1f;
        controlWindow.style.borderRightColor = Color.black;
        controlWindow.style.borderRightWidth = 1f;
    } 
    private void BuildControlContent()
    {
        controlWindowContent = new VisualElement();
        controlWindow.style.borderTopLeftRadius = 5;
        controlWindow.style.borderTopRightRadius = 5;
        controlWindow.style.borderBottomLeftRadius = 5;
        controlWindow.style.borderBottomRightRadius = 5;
        switchContent = new VisualElement();
        switchContent.style.marginLeft = new StyleLength(3f); 
        switchContent.style.marginRight = new StyleLength(3f); 
        switchContent.style.marginTop = new StyleLength(4f); 
    }
    private void BuildControlHeaderSection()
    {
        var headerCombo = new VisualElement();
        headerCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        headerCombo.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        headerCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        headerCombo.style.marginBottom = new StyleLength(3f); 

        var headerLabel = new Label("Maps");
        headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        headerLabel.style.fontSize = new StyleLength(14f);
        headerLabel.style.paddingLeft = new StyleLength(4);

        buttonAddMap = new WhiteButton(RootControl, "Add Map..", ClickAddMap);
        buttonAddMap.style.marginRight = new StyleLength(4f); 
        buttonAddMap.style.marginTop = new StyleLength(4f);
        buttonAddMap.style.width = 75f;
        buttonAddMap.text = "Add Map..";
        buttonAddMap.style.fontSize = new StyleLength(12f);
        buttonAddMap.clicked -= ClickAddMap;
        buttonAddMap.clicked += ClickAddMap;
    
        headerCombo.Add(headerLabel);
        headerCombo.Add(buttonAddMap);
        controlWindowContent.Add(headerCombo);
    }
    private void BuildControlMapDropdown()
    {
        var dropdownCombo = new VisualElement();
        dropdownCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        dropdownCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        
        mapDropdownLabel = new Label("Map");
        mapDropdownLabel.style.marginTop = 3f;
        mapDropdownLabel.style.marginRight = 3f;
        mapDropdownLabel.style.marginLeft = 3f;
        mapDropdownLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        dropdownFieldChooseMap = new SquareDropdown(RootControl, "dropdownFieldChooseMap",OnMapSelectDropdownChange );
        dropdownFieldChooseMap.style.width = 144f;
        dropdownFieldChooseMap.style.flexShrink = 1;
        dropdownFieldChooseMap.style.height = 21f;
        dropdownFieldChooseMap.style.marginLeft = 13;
        SetMapDropdownChoices();
        dropdownCombo.Add(mapDropdownLabel);
        dropdownCombo.Add(dropdownFieldChooseMap);
        controlWindowContent.Add(dropdownCombo);
    }
    private void SetMapDropdownChoices()
    {
        List<string> mapNames = new List<string>();
        foreach (var map in RootMap.OrderMapList( RootControl.World.Maps))
            if (map != null)
                mapNames.Add(map.Name);
        if (mapNames.Count == 0)
        {
            existingMapsPresent = false;
            mapNames.Add("0 Maps. Please add one..");
            dropdownFieldChooseMap.choices = mapNames;
            dropdownFieldChooseMap.value = mapNames[0];
            dropdownFieldChooseMap.SetEnabled(false);
        }
        else
        {
            existingMapsPresent = true;
          
            dropdownFieldChooseMap.choices = mapNames;
            if (RootControl.Map != null)
                dropdownFieldChooseMap.value = RootControl.Map.Name;
            else
                dropdownFieldChooseMap.value = mapNames[0];
        }
    }

    private void OnMapSelectDropdownChange(string value)
    {
        RootMap.SelectMapFromDropdown(value);
    }

   

    private void BuildExistingMapsContent()
  {
      float bottomSpacing = 4f;
        if (RootControl.Map == null)
            return; //todo-cleanup: avoid this getting triggered by RootControl setting World in LoadWorld() before Map

        Map map = RootControl.Map;
        existingMapContent = new VisualElement();

        // Name field
        var mapNameCombo = new VisualElement();
        mapNameCombo.tooltip = "Set the Map name, stored as part of the World";
        mapNameCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        mapNameCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        mapNameCombo.style.marginBottom = bottomSpacing;
        var mapNameLabel = new Label("Name");
        mapNameLabel.style.marginRight = new StyleLength(5f);
        mapNameField = new BlueTextField(RootControl);
        mapNameField.style.flexShrink = 1;
        mapNameField.value = map.Name;
        mapNameField.RegisterValueChangedCallback(OnExistingMapNameFieldChange);
        mapNameCombo.Add(mapNameLabel);
        mapNameCombo.Add(mapNameField);

        // Type dropdown
        var typeCombo = new VisualElement();
        typeCombo.tooltip = "Set the Map Type, in descending order of domain hierarchy";
        typeCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        typeCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        typeCombo.style.marginBottom = bottomSpacing;
        var typeLabel = new Label("Type");
        typeLabel.style.marginRight = new StyleLength(10f);
        mapTypeDropdown = new SquareDropdown(RootControl, "mapTypeDropdown", null );
        mapTypeDropdown.RegisterValueChangedCallback((evt) =>
        {
            mapStoreButton.text = "Store";
        }); 
        mapTypeDropdown.style.width = Length.Percent(100);
        mapTypeDropdown.style.flexShrink = 1;
        List<string> mapTypeNames = Enum.GetNames(typeof(Map.Type)).ToList();
        mapTypeDropdown.choices = mapTypeNames;
        mapTypeDropdown.value = map.type.ToString();
        typeCombo.Add(typeLabel);
        typeCombo.Add(mapTypeDropdown);
        
        
        // Parent Map  
        var parentCombo = new VisualElement();
        parentCombo.tooltip = "Set the parent Map that this Map is contained in or part of";
        if (RootControl.World.Maps.Count > 1) // only create content if there are other maps
        {
            parentCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
            parentCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
            parentCombo.style.marginBottom = bottomSpacing;
            var parentLabel = new Label("Parent");
            parentLabel.style.marginRight = new StyleLength(1f);
            mapParentDropdown = new SquareDropdown(RootControl, "mapParentDropdown", null );
            mapParentDropdown.style.flexShrink = 1;
            mapParentDropdown.style.width = Length.Percent(100);
            List<string> parentMapNames = new List<string>(); 
            parentMapNames.Add("None");
            foreach (var otherMap in RootControl.World.Maps)
                if (otherMap != null)
                    if (otherMap != map)
                        parentMapNames.Add(otherMap.Name);
            mapParentDropdown.choices = parentMapNames;
            if (string.IsNullOrEmpty( map.ParentMap ) == false)
                mapParentDropdown.value = map.ParentMap;
            else
                mapParentDropdown.value = mapParentDropdown.choices[0];

            parentCombo.Add(parentLabel);
            parentCombo.Add(mapParentDropdown);
        }

        // Background color picker
        var colorCombo = new VisualElement();
        colorCombo.tooltip = "Set the window background color for the Map";
        colorCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        colorCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        colorCombo.style.marginBottom = bottomSpacing+2f ;
        var colorLabel = new Label("Color");
        colorLabel.style.marginRight = new StyleLength(8f);
        colorPicker = new ColorField();
        colorPicker.style.flexShrink = 1; 
        colorPicker.value = RootMap.HexStringToColor(map.BackgroundColor); // Starting color (this is red)
        colorPicker.RegisterValueChangedCallback((evt) =>
        {
            map.BackgroundColor = RootMap.ColorToHexString(evt.newValue);
            RefreshControlWindow();
        });
        colorCombo.Add(colorLabel);
        colorCombo.Add(colorPicker);
        
        // Description 
        var descriptionCombo = new VisualElement();
        descriptionCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        descriptionCombo.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        descriptionCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        var descriptionLabel = new Label("Description");
        expandButton = new WhiteButton(RootControl,"Shorten", ClickDescriptionExpandButton);
        expandButton.style.marginTop = 2f;
        if (expandToggled)
            expandButton.text = "Shorten";
        else
            expandButton.text = "Expand"; 
        descriptionCombo.Add(descriptionLabel);
        descriptionCombo.Add(expandButton);
        descriptionCombo.style.marginBottom = 2f;
        
        
        float normalSize = 33;
        float expandSize = 123;
        descriptionField = new TextField();
        descriptionField.multiline = true; //todo-minor: also make this wordWrap, but how?
        if (expandToggled == false)
            descriptionField.style.height = new Length(normalSize, LengthUnit.Pixel);
        else
            descriptionField.style.height = new Length(expandSize, LengthUnit.Pixel);
        if (map.Description == "")
            descriptionField.value = "Add a description..";
        else
          descriptionField.value = map.Description;
        descriptionField.style.marginBottom = 2f;
        descriptionField.RegisterValueChangedCallback((evt) =>
        {
            mapStoreButton.text = "Save";
        });
        // Store Button

        var buttonRow = new SimpleRow();
        buttonRow.style.marginBottom = 2f;
        buttonRow.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);

        WhiteButton deleteButton = new WhiteButton(RootControl, "Delete", ClickDeleteMapButton);
        deleteButton.style.width = Length.Percent(48);
        
        mapStoreButton = new WhiteButton(RootControl, "Save", ClickMapStoreButton); 
        mapStoreButton.style.width = Length.Percent(48);
        
        buttonRow.Add(deleteButton);
        buttonRow.Add(mapStoreButton);
        
        existingMapContent.Add(typeCombo);
        existingMapContent.Add(mapNameCombo);
    //    existingMapContent.Add(parentCombo); // Leave parent option out of Map window - use only pins instead
        existingMapContent.Add(colorCombo);
    //    existingMapContent.Add(descriptionCombo);
    //    existingMapContent.Add(descriptionField);
        existingMapContent.Add(buttonRow);
        switchContent.Add(existingMapContent);
    }

    private void ClickDeleteMapButton()
    {
        RootMap.DeleteMap(dropdownFieldChooseMap.value);
        BuildWindow();
    }


    private void ClickMapStoreButton()
    {
        if (RootControl.Map == null)
            return;
        Map.Type mapType = (Map.Type)Enum.Parse(typeof(Map.Type), mapTypeDropdown.value); 
     //   RootMap.UpdateMapData(mapNameField.value, mapType, mapParentDropdown.value, colorPicker.value, descriptionField.value);
        RootMap.UpdateMapData(mapNameField.value, mapType,colorPicker.value, descriptionField.value);
        mapStoreButton.text = "Saved";
    }

    private void ClickDescriptionExpandButton()
    {
        expandToggled = !expandToggled;
        RefreshControlWindow(); 
    }
  private void OnExistingMapNameFieldChange(ChangeEvent<string> evt)
  {
       if (evt.newValue == "")
           mapStoreButton.SetEnabled(false);
       else
           mapStoreButton.SetEnabled(true);
       mapStoreButton.text = "Store";
  }


  private void BuildControlAddMapContent()
    {
        addMapContainerOpen = true;
        buttonAddMap.text = "Cancel";
        mapDropdownLabel.text = "Adding Map..";
        if (existingMapsPresent)
            existingMapContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        dropdownFieldChooseMap.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        
        addMapContent = new VisualElement();

        var filePickCombo = new VisualElement();
        filePickCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
    //    filePickCombo.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        filePickCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        filePickCombo.style.marginTop = new StyleLength(3f); 
        filePickCombo.style.marginBottom = new StyleLength(5f); 

        mapPathField = new BlueTextField(RootControl);
        mapPathField.value = "Path..";
        mapPathField.style.flexShrink = 1;

       // mapPathField.SetEnabled(false);
        mapPathField.RegisterValueChangedCallback(OnMapPathFieldChange);

        var mapPickButton = new WhiteButton(RootControl, "File..", ClickFileSelectButton); 
        mapPickButton.style.flexShrink = 1;
        mapPickButton.style.marginLeft = new StyleLength(1f);
        

        filePickCombo.Add(mapPickButton);
        filePickCombo.Add(mapPathField);
        
        var mapNameCombo = new VisualElement();
        mapNameCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        mapNameCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        mapNameCombo.style.marginBottom = 25f;
        var mapNameLabel = new Label("Name");
        mapNameLabel.style.marginLeft = new StyleLength(3f);
        mapNameLabel.style.marginRight = new StyleLength(5f);

       
        mapNameField = new BlueTextField(RootControl);
        mapNameField.RegisterValueChangedCallback(OnAddMapNameFieldChange);
        mapNameField.style.marginBottom = new StyleLength(-2f);
        mapNameField.style.flexShrink = 1;

        mapNameCombo.Add(mapNameLabel);
        mapNameCombo.Add(mapNameField);

        mapSaveButton = new WhiteButton(RootControl, "Copy and Save",ClickMapSaveButton );
        mapSaveButton.style.width = Length.Percent(100);
        mapSaveButton.SetEnabled(false);
      
        var descriptionField = new TextField();
        descriptionField.value = "Description..";
        descriptionField.multiline = true;
        descriptionField.style.height = new Length(66, LengthUnit.Pixel);
        descriptionField.style.marginBottom = 3f;
        
        addMapContent.Add(filePickCombo);
        addMapContent.Add(mapNameCombo);
       // addMapContent.Add(descriptionField);
        addMapContent.Add(mapSaveButton); 
        switchContent.Add(addMapContent);
    }
    private void ClickAddMap()
    {
        if (addMapContainerOpen)
            RefreshControlWindow();
        else
            BuildControlAddMapContent();
    }
     
    private void ClickMapSaveButton()
    {
        string sourcePath = mapPathField.value;
        if (string.IsNullOrEmpty(sourcePath) || !System.IO.File.Exists(sourcePath))
        {
            Debug.LogError("No valid file selected");
            return;
        }
        string targetPath = Application.dataPath + "/Resources/Images/Maps/" + System.IO.Path.GetFileName(sourcePath);
        string relativeTargetPath = "Assets/Resources/Images/Maps/" + System.IO.Path.GetFileName(sourcePath);
    
        // Check if sourcePath is not the same as targetPath
        if (!string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
            System.IO.File.Copy(sourcePath, targetPath, true);
        
       string filenameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
  //      string filenameWithExtension = System.IO.Path.GetFileName(sourcePath);


        AssetDatabase.ImportAsset(relativeTargetPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        RootControl.SetMap(RootMap.CreateMap(mapNameField.value, filenameWithoutExtension));
    }
    private void ClickFileSelectButton()
    {
        string path = EditorUtility.OpenFilePanel("Select an image file", "", "jpg,,jpeg,png");
        if (!string.IsNullOrEmpty(path))
        {
            mapPathField.value = path;
            mapNameField.value = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
    private void OnMapPathFieldChange(ChangeEvent<string> evt)
    {
        if (evt.newValue == "")
            mapSaveButton.SetEnabled(false);
        else if (mapNameField.value != "")
            mapSaveButton.SetEnabled(true);
    }
    private void OnAddMapNameFieldChange(ChangeEvent<string> evt)
    {
        if (evt.newValue == "")
            mapSaveButton.SetEnabled(false);
        else if (mapPathField.value != "" &&   mapPathField.value != "Path..")
            mapSaveButton.SetEnabled(true);
    }

    private void RegisterControlWindowCallbacks()
{
    Vector2 offset = Vector2.zero;
    controlWindow.RegisterCallback<MouseDownEvent>(evt =>
    {
        // This calculates where inside of the controlWindow the user clicked
        offset = evt.localMousePosition;
    }, TrickleDown.NoTrickleDown);

    controlWindow.RegisterCallback<MouseMoveEvent>(evt =>
    {
        if (evt.pressedButtons == (1 << (int)MouseButton.LeftMouse))
        {
            // Get the position of the mouse relative to the root element
            Vector2 mousePos = controlWindow.parent.WorldToLocal(evt.mousePosition);
            // Calculate new window position
            controlWindow.style.left = mousePos.x - offset.x;
            controlWindow.style.top = mousePos.y - offset.y;
            controlWindowPosition = new Vector2(controlWindow.layout.x, controlWindow.layout.y);
        }
    });
    controlWindow.RegisterCallback<MouseUpEvent>(evt =>
    {
        // Release the offset when the mouse is released
        offset = Vector2.zero;
    }, TrickleDown.NoTrickleDown);
}

    
    
    #endregion
    
    

//todo-functionality: make the 'camera' center on a pinelement. Below does strange storing on first attempt, then recalling that stored state
    private void CenterOnPinElement(PinElement pinElement)
    { 
        /*// Using xCoord and yCoord directly
        Vector2 pinPosition = new Vector2(pinElement.xCoord, pinElement.yCoord);

        Debug.Log($"PinElement Position: {pinPosition.x}, {pinPosition.y}");
        Debug.Log($"MapScaleContainer Width: {_mapScaleContainer.resolvedStyle.width}, Height: {_mapScaleContainer.resolvedStyle.height}");

        // Calculate the current viewport's center position
        Vector2 viewportCenter = new Vector2(_mapScaleContainer.resolvedStyle.width / 2, _mapScaleContainer.resolvedStyle.height / 2);

        // Difference between PinElement's position and the viewport's center
        Vector2 diff = pinPosition - viewportCenter;

        Debug.Log($"Viewport Center: {viewportCenter}, Pin Position: {pinPosition}, Difference: {diff}");

        // Delegate the panning to the MapScaleContainer
        _mapScaleContainer.Pan(-diff); */
    }

    #region Getters
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
    private RootMap _rootMap;
    private RootMap RootMap
    {
        get
        {
            if (_rootMap == null)
                _rootMap = LoadRootMap();
            return _rootMap;
        }
    }
    private RootMap LoadRootMap()
    {
        RootMap rootMap =  AssetDatabase.LoadAssetAtPath<RootMap>(MonoLoader.projectPath + MonoLoader.rootPath + "RootMap.asset");
        if (rootMap == null)
            Debug.LogWarning("! No RootMap found. Please re-load the tool from Launcher.");
        return rootMap;
    }
    #endregion
}
