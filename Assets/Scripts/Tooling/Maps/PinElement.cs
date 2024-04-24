using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;


// VisualElement container that represents Pins on a map 

public class PinElement : VisualElement
{
    public Pin pin;
    
    
    public string ID;
    
    private Vector2 initialSize;
    public bool temporary = false;
    private Vector2 position;
    public float  xCoord;
    public float  yCoord;
    public float relativeX;  // Between 0 and 1
    public float relativeY;  // Between 0 and 1
    
    
    private Vector3 latestZoomScale;
    private Vector3 effectiveScale;

    private List<VisualElement> components = new List<VisualElement>();
    private bool interactable = true; 

    private VisualElement baseElement; 
    private VisualElement colorElement;
    private VisualElement iconElement;
    private NameFlag nameFlag;

    
    
    
    
    
    
   
    /// <summary>
    /// called from temp pin creation
    /// </summary>
    /// <param name="position"></param>
    /// <param name="sprite"></param>
    /// <param name="zoomScale"></param>
    public PinElement(Vector2 position, Sprite sprite,  Vector3 zoomScale, Vector2 effectiveScale )
    {
        latestZoomScale = zoomScale;
        this.effectiveScale = effectiveScale;
        
        ID = System.Guid.NewGuid().ToString();
        this.temporary = true;
        interactable = true;
        this.style.width = RootMap.GetPinSize() ;   
        this.style.height = RootMap.GetPinSize() ;
        style.backgroundImage = new StyleBackground(sprite.texture);
        SetPositionForTempPin(position);
        RootMap.StoreTempPinElement(this);
        initialSize = new Vector2(RootMap.GetPinSize(), RootMap.GetPinSize());
        ApplyScaleFromZoom(latestZoomScale, effectiveScale); 
    }
    
    /// <summary>
    /// called from existing pin creation
    /// </summary>
    /// <param name="pin"></param>
    public PinElement(Pin pin)
    {
        components = new List<VisualElement>();
        baseElement = new VisualElement();
        colorElement = new VisualElement();
        interactable = true; 
        baseElement.style.width = RootMap.GetPinSize();
        baseElement.style.height = RootMap.GetPinSize();
        StyleAndPositionColorElement(colorElement); 
        
        this.pin = pin;
        ID = pin.ID;
        temporary = false; 
   
     //   colorElement.style.backgroundImage = new StyleBackground(RootControl.dropdownArrow.texture);  // todo enable proper inner sprite
    

        this.RegisterCallback<MouseEnterEvent>(OnMouseEnterPin);
        this.RegisterCallback<MouseLeaveEvent>(OnMouseLeavePin);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp); 
        this.RegisterCallback<MouseDownEvent>(OnMouseDown);

        components.Add(baseElement); 
        components.Add(colorElement); 

        EditorCoroutineUtility.StartCoroutineOwnerless(SelectColorAfterDelay());

        ToggleNameFlag(true);
        CreatePinCenterIcon();

        ApplyPinToggles();
        
        this.Add(baseElement);
    }

private IEnumerator   SelectColorAfterDelay()
{
    yield return new EditorWaitForSeconds(0.02f); 
    if (pin.category == Pin.Category.Element)
        if (RootControl.Element != null)
            if (RootControl.Pin != null)
               if (RootControl.Pin == pin)
                      if (RootControl.Element.ID == pin.Element)
                        SetColor(RootMap.pinSelectedColor); //todo-minor now again the color disappears after rebuildelements, so after temp pin create.. 

}
    private void CreatePinCenterIcon()
    {
        iconElement = new VisualElement();

        iconElement.style.position = new StyleEnum<Position>(Position.Absolute); 
        iconElement.style.width = RootMap.pinCenterIconSize ;   
        iconElement.style.height = RootMap.pinCenterIconSize ;   
        float leftOffset = (RootMap.GetPinSize() - RootMap.pinCenterIconSize) / 2;
        float topOffset = (RootMap.GetPinSize() - RootMap.pinCenterIconSize) / 2;
        iconElement.style.left = leftOffset;
        iconElement.style.top = topOffset;
        
        if (pin.category == Pin.Category.Element)
        { 
            //    iconElement.style.backgroundImage = new StyleBackground(RootMap.pinRowIconElement);  // todo proper toggling and sprites etc 
         iconElement.style.backgroundImage = new StyleBackground(RootMap.GetPinCenterIconSprite(RootMap.GetElementForPin(pin).category).texture);
        } 
        else if (pin.category == Pin.Category.Map)
            iconElement.style.backgroundImage = new StyleBackground(RootMap.pinRowIconMap);
        else if (pin.category == Pin.Category.Descriptive)
            iconElement.style.backgroundImage = new StyleBackground(RootMap.pinRowIconDescriptive);

        

        this.Add(iconElement);
    }

    private void SetColor(Color color)
    {
        colorElement.style.backgroundColor = new Color(color.r, color.g, color.b, 0.5f);  
    }
    private void StyleAndPositionColorElement(VisualElement visualElement)
    { 
        
        visualElement.style.borderBottomLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent)); // Makes the element circular
        visualElement.style.borderBottomRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));  
        visualElement.style.borderTopLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent));  
        visualElement.style.borderTopRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));   
        float leftOffset = (RootMap.GetPinSize() - RootMap.pinColoringSize) / 2;
        float topOffset = (RootMap.GetPinSize() - RootMap.pinColoringSize) / 2;
        visualElement.style.width = RootMap.pinColoringSize ;   
        visualElement.style.height = RootMap.pinColoringSize ;
     
        visualElement.style.position = new StyleEnum<Position>(Position.Absolute);
        visualElement.style.left = leftOffset; 
        visualElement.style.top = topOffset;
        SetColor(RootMap.pinBaseColor);
     
        this.Add(visualElement);
    }

    private void OnMouseEnterPin(MouseEnterEvent evt)
    { 
        if (interactable == false)
            return;
        if (RootMap.hoverPinColor)
            SetColor(RootMap.pinHoverColor);
        
        ApplyBaseElementScale(Vector3.one / RootMap.pinHoverScaleMultiplier );
        RootMap.hoveredPinElement = this;
    }
    private void OnMouseLeavePin(MouseLeaveEvent evt)
    { 
        if (interactable == false)
            return;
        if (RootControl.Pin != null)
        {
            if (RootControl.Pin != pin)
                SetColor(RootMap.pinBaseColor); 
        }
        else
            SetColor(RootMap.pinBaseColor); 

          
        ApplyBaseElementScale(Vector3.one );
        RootMap.hoveredPinElement = null;
    }
    private void OnMouseUp(MouseUpEvent evt)
    {  
        if (interactable == false)
            return; 
    }
    private void OnMouseDown(MouseDownEvent evt)
    {  
        if (interactable == false)
            return;
    } 
    public void ClickPinElement()
    {
     //   ToggleNameFlag(true);
    }
    public void DragPinElement(Vector2 mousePos, Vector2 delta)
    { 
        xCoord += delta.x / latestZoomScale.x;
        yCoord += delta.y / latestZoomScale.y;

        style.left = Mathf.RoundToInt(xCoord);
        style.top = Mathf.RoundToInt(yCoord); 

        relativeX = xCoord / (RootMap.GetActiveMapTexture().width * effectiveScale.x);
        relativeY = yCoord / (RootMap.GetActiveMapTexture().height * effectiveScale.y);
    }
    

    public void EndPinDrag()
    {
        Pin associatedPin = RootMap.GetPinForPinElement(this);

        associatedPin.CoordX = relativeX;
        associatedPin.CoordY = relativeY; 
    }
    private void SetPositionForTempPin(Vector2 givenPosition)
    {
        this.style.position = Position.Absolute;
        xCoord = (int) (givenPosition.x - RootMap.GetPinSize()/2);
        yCoord = (int) (givenPosition.y - RootMap.GetPinSize()/2);
 
        relativeX = xCoord / (RootMap.GetActiveMapTexture().width * effectiveScale.x);
        relativeY = yCoord / (RootMap.GetActiveMapTexture().height * effectiveScale.y);

        this.style.left = xCoord;
        this.style.top = yCoord;

        //   Debug.Log($"temp pin has xCoord: {xCoord}, relativeX: {relativeX}");
    }
    #region CalledFromIntegratePinElements
    public void SetPositionForExistingPin(Vector2 effectiveScale)
    {
        this.effectiveScale = effectiveScale;  
        this.style.position = Position.Absolute;

        // Assuming pin.CoordX and pin.CoordY are relative coordinates between 0 and 1
        xCoord = RootMap.GetActiveMapTexture().width * pin.CoordX;
        yCoord = RootMap.GetActiveMapTexture().height * pin.CoordY;
 
        // Adjusting for effective scale
        xCoord *= effectiveScale.x;
        yCoord *= effectiveScale.y;
 
        this.style.left = xCoord;
        this.style.top = yCoord;

        // Set the relative position for consistency
        relativeX = pin.CoordX;
        relativeY = pin.CoordY;
    }

    /*public void SetPositionForExistingPin(float zoomScale)
    {
        this.style.position = Position.Absolute;

        float adjustedWidth = RootMap.GetActiveMapTexture().width * zoomScale;
        float adjustedHeight = RootMap.GetActiveMapTexture().height * zoomScale;

        xCoord = adjustedWidth * pin.CoordX;
        yCoord = adjustedHeight * pin.CoordY;

        this.style.left = xCoord;
        this.style.top = yCoord;

        // Set the relative position for consistency
        relativeX = pin.CoordX;
        relativeY = pin.CoordY;
        
        Debug.Log( zoomScale + "" + pin.Name + "  x: "+ xCoord + "relX:" + relativeX + "  width: " +RootMap.GetActiveMapTexture().width + " pin.CoordX: " + pin.CoordX);

    }*/

    public void SetSprite(Sprite sprite)
    { 
        baseElement.style.backgroundImage = new StyleBackground(sprite.texture);
    }

    private Vector3 globalScale = Vector3.one;
    private Vector3 zoomScale = Vector3.one;
    public void ApplyGlobalScale()
    {
        globalScale = new Vector3(RootMap.pinScaling, RootMap.pinScaling, RootMap.pinScaling);
        UpdateTransformScale();
    }

    public void ApplyScaleFromZoom(Vector3 zoomScaleValue, Vector2 effectiveScale)
    {
        this.effectiveScale = effectiveScale;
            latestZoomScale = zoomScaleValue;

        if (zoomScaleValue == Vector3.zero)
            zoomScale = Vector3.one;
        else
            zoomScale = new Vector3(1 / zoomScaleValue.x, 1 / zoomScaleValue.y, 1 / zoomScaleValue.z);

        UpdateTransformScale();
    
        if (RootMap.highlightMode == false && RootMap.useZoomscales)
            if (RootMap.filterMode == RootMap.FilterMode.None)
                 ResolveZoomscale();
    }

    private void UpdateTransformScale()
    {
        this.transform.scale = Vector3.Scale(globalScale, zoomScale);
        MarkDirtyRepaint();
    }
    private void ResolveZoomscale()
    { 
        if (pin == null) 
            return;

        if (RootControl.Pin != null)
            if (RootControl.Pin == pin)
            {
                SetTransparency(1);
                return;
            }  
        float threshold1 = (RootMap.zoomMax - RootMap.zoomMin) / RootMap.zoomScaleThresholdOne;
        float threshold2  = (RootMap.zoomMax - RootMap.zoomMin) / RootMap.zoomScaleThresholdTwo;  

       
        switch (pin.Zoomscale)
        {
            case 1:
                if (latestZoomScale.x < threshold1)
                    SetTransparencyHidden();
                else if (latestZoomScale.x < threshold2)
                    SetTransparencyLowest();
                else
                    SetTransparencyFull();
                break;
            case 2:
                if (latestZoomScale.x < threshold1)
                    SetTransparencyLow();
                else if (latestZoomScale.x < threshold2)
                    SetTransparencyFull();
                else
                    SetTransparencyFull();
                break;
            case 3:
                SetTransparencyFull();
                break;
        }
    }
 
    private void SetTransparency(float alphaValue)
    { 

        if (alphaValue <= 0)
        {
            this.style.display = DisplayStyle.None;
            this.interactable = false;
        }
        else
        {
            this.style.display = DisplayStyle.Flex;
            this.interactable = true;
        }
        foreach (var visualElement in components)
            visualElement.style.opacity = alphaValue;
    }

    private void ApplyBaseElementScale(Vector3 zoomScale )
    {  
        if (zoomScale == Vector3.zero)
            baseElement.transform.scale = Vector3.one;
        else
            baseElement.transform.scale = new Vector3(1 / zoomScale.x, 1 / zoomScale.y, 1 / zoomScale.z); 
    }


    // set base sprite, color, icon, name flag according to pin settings
    public void ApplyPinToggles()
    { 
        baseElement.style.display = pin.ToggleBase ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        colorElement.style.display = pin.ToggleColor ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        iconElement.style.display = pin.ToggleIcon ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        nameFlag.style.display = pin.ToggleName ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    public void SetTransparencyHidden()
    {
        SetTransparency(0);
    }
    public void SetTransparencyLowest()
    {
        SetTransparency(RootMap.zoomScaleOneAlpha);
    }
    public void SetTransparencyLow()
    {
        SetTransparency(RootMap.zoomScaleTwoAlpha);
    }
    public void SetTransparencyFull()
    {
        SetTransparency(1);
    }
    public void ToggleNameFlag(bool enable)
    { 
        if (nameFlag != null)
            nameFlag.RemoveFromHierarchy();
        if (enable)
        {
            nameFlag = new NameFlag(RootMap, pin.Name); 
            AddFlag(nameFlag);
            this.Add(nameFlag);
        }

        if (pin.ToggleName == false)
            nameFlag.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }

    private void AddFlag(NameFlag flag)
    {
        components.Add(flag);
    }

    #endregion
    public void SelectHighlight()
    {  

       EditorCoroutineUtility.StartCoroutineOwnerless( SetColorAfterDelay());
        ToggleNameFlag(true);
    }

    private IEnumerator   SetColorAfterDelay()
    {
        yield return new EditorWaitForSeconds(0.1f);
        SetColor(RootMap.pinSelectedColor);

    }
    public void SecondaryHighlight()
    {  
        SetColor(RootMap.pinHighlightColor); 
        ToggleNameFlag(true);  
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


public class NameFlag : VisualElement
{
    private RootMap RootMap;
    private BlackLabel nameLabel;

    public NameFlag(RootMap rootMap, string labelText)
    {
        RootMap = rootMap;
         
        var sprite = RootMap.GetNameFlagSprite();
        this.style.width = sprite.texture.width;
        this.style.height = sprite.texture.height;
        this.style.backgroundImage = sprite.texture;
 
        this.style.position = Position.Absolute;

        this.style.display = DisplayStyle.Flex;
        this.style.justifyContent = Justify.Center;
        this.style.alignItems = Align.Center; 
        float horizontalOffset = (-sprite.texture.width / 4f) - RootMap.GetPinSize() / 8f;
        this.style.marginLeft = horizontalOffset;
        float verticalOffset = RootMap.GetPinSize()-2f;
        this.style.marginTop = verticalOffset;


        nameLabel = new BlackLabel( );
        nameLabel.text = labelText;
        nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        nameLabel.style.marginTop = -4f;
        nameLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        this.Add(nameLabel);
    }
}
