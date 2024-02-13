using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using World_Model;
using World_Model.Elements;
using Object = World_Model.Elements.Object;

// pin window content for when an existing pin is selected


public class PinScreen : VisualElement
{
    private Pin selectedPin;
    private PinSubWindow _pinSubWindow;
    
    

    // Pin Settings
    private InlayBox adaptiveInlayBox;
    private ClickLabel nameClickLabel;
    private SquareDropdown zoomscaleDropdown;
    
    // Pin Controls
    private InlayBox controlsInlayBox;

    // Visuals / Flags
    private InlayBox visualsInlayBox;

    // Box Elements
    private VisualElement adaptiveContent;
    private VisualElement mapImage;
    private VisualElement locationMapImage;
    private VisualElement controlsContent; 
    private VisualElement visualsContent; 

    public PinScreen(PinSubWindow pinSubWindow)
    {
        _pinSubWindow = pinSubWindow;
        BuildWindow();
    }

    
    /// <summary>
    ///  window is rebuilt each time another pin is selected
    /// </summary>
    private void BuildWindow()
    {
        float defaultBottomSpacing = 4f;
        if (RootControl.Pin == null)
            return;
        
        selectedPin = RootControl.Pin;

        adaptiveInlayBox = new InlayBox(250, RootMap.pinWindowColor);
        BuildAdaptiveBox();
        adaptiveInlayBox.style.marginBottom = defaultBottomSpacing *2f; 
        adaptiveInlayBox.style.height = adaptiveContent.resolvedStyle.height  ;
        this.Add(adaptiveInlayBox);

        controlsInlayBox = new InlayBox(250, RootMap.pinWindowColor);
        BuildControlsBox();
        controlsInlayBox.style.height = controlsContent.resolvedStyle.height  ;
        controlsInlayBox.style.marginBottom = defaultBottomSpacing *2f;  
        this.Add(controlsInlayBox);

        visualsInlayBox = new InlayBox(250, RootMap.pinWindowColor);
        BuildVisualsBox();
        visualsInlayBox.style.height = visualsContent.resolvedStyle.height  ;
        this.Add(visualsInlayBox); 
   
    }

    private void BuildVisualsBox()
    {
        float bottomSpacing = 3f;
        visualsContent = new VisualElement();
        visualsContent.style.marginTop = 2f;
        visualsContent.style.marginLeft = 2f;
        
        SimpleToggle baseSpriteToggle = new SimpleToggle(
            RootControl,
            "Base Sprite",
            () => selectedPin.ToggleBase,           
            newValue => selectedPin.ToggleBase = newValue,  
            newVal => OnPinVisualsChange()
        );
        baseSpriteToggle.style.marginBottom = bottomSpacing;  
        
        SimpleToggle colorToggle = new SimpleToggle(
            RootControl,
            "Color",
            () => selectedPin.ToggleColor,           
            newValue => selectedPin.ToggleColor = newValue,  
            newVal => OnPinVisualsChange()
        );
        colorToggle.style.marginBottom = bottomSpacing;  
        
        SimpleToggle iconToggle = new SimpleToggle(
            RootControl,
            "Icon",
            () => selectedPin.ToggleIcon,           
            newValue => selectedPin.ToggleIcon = newValue,  
            newVal => OnPinVisualsChange()
        );
        iconToggle.style.marginBottom = bottomSpacing;  
        
        SimpleToggle nameToggle = new SimpleToggle(
            RootControl,
            "Name",
            () => selectedPin.ToggleName,           
            newValue => selectedPin.ToggleName = newValue,  
            newVal => OnPinVisualsChange()
        );
        nameToggle.style.marginBottom = bottomSpacing+2f;  
        
        visualsContent.Add(baseSpriteToggle);
        visualsContent.Add(colorToggle);
        visualsContent.Add(iconToggle);
        visualsContent.Add(nameToggle);

        WhiteButton applyAllButton = new WhiteButton(RootControl, "Apply to All", ClickApplyAll);
        applyAllButton.style.marginBottom = bottomSpacing;
        applyAllButton.style.width = 80f;

        visualsContent.Add(applyAllButton);
        
        visualsInlayBox.Add(visualsContent);
    }

    private void ClickApplyAll()
    {
        bool toggleBase = selectedPin.ToggleBase;
        bool toggleColor = selectedPin.ToggleColor;
        bool toggleIcon = selectedPin.ToggleIcon;
        bool toggleName = selectedPin.ToggleName;
        
        foreach (var pin in RootControl.World.Pins)
        {
            pin.ToggleBase = toggleBase;
            pin.ToggleColor = toggleColor;
            pin.ToggleIcon = toggleIcon;
            pin.ToggleName = toggleName;
        }
        RootMap.RebuildPinElements();
    }

    private void OnPinVisualsChange()
    {
         RootMap.GetPinElementForPin(selectedPin).ApplyPinToggles();
    }

    private void BuildControlsBox()
    {
        controlsContent = new VisualElement();
        
        WhiteButton deleteButton = new WhiteButton(RootControl, "Delete Pin", ClickDeleteButton); 
        deleteButton.style.marginTop = 4f;
        deleteButton.style.marginLeft = 4f;
        deleteButton.style.marginBottom = 4f;
        deleteButton.style.width = 70f;
        controlsContent.Add(deleteButton);

        
        string scaleStr = "Zoomscale: " + selectedPin.Zoomscale;
        zoomscaleDropdown = new SquareDropdown(RootControl,scaleStr , OnZoomScaleDropdownChange );
        zoomscaleDropdown.tooltip = "Zoomscale 3: Always show Pin" +
                                    "\nZoomscale 2: Transparent when zoomed out over two thirds" +
                                    "\nZoomscale 1: Transparent when zoomed out over one third, hidden over two thirds";
        List<string> zoomscales = new List<string>();
        zoomscales.Add("Zoomscale: 3");
        zoomscales.Add("Zoomscale: 2");
        zoomscales.Add("Zoomscale: 1");
        zoomscaleDropdown.choices = zoomscales;
        if (selectedPin.Zoomscale == 1)
            zoomscaleDropdown.SetValueWithoutNotify(zoomscales[2]);
        if (selectedPin.Zoomscale == 2)
            zoomscaleDropdown.SetValueWithoutNotify(zoomscales[1]);
        if (selectedPin.Zoomscale == 3)
            zoomscaleDropdown.SetValueWithoutNotify(zoomscales[0]); 
        zoomscaleDropdown.style.marginBottom = 4f; 
        controlsContent.Add(zoomscaleDropdown);

     

        controlsInlayBox.Add(controlsContent); 
    }

    private void ClickDeleteButton()
    {
        RootMap.DeletePin(selectedPin);
    }

    // create an InlayBox with content that depends on selected pin category
    private void BuildAdaptiveBox()
    {
        if (selectedPin.category == Pin.Category.Element)
            adaptiveInlayBox.Add(CreateAdaptiveElement());
        else    if (selectedPin.category == Pin.Category.Map)
            adaptiveInlayBox.Add(CreateAdaptiveMap());
        else    if (selectedPin.category == Pin.Category.Descriptive)
            adaptiveInlayBox.Add(CreateAdaptiveDescriptive());
    }

  

    private VisualElement CreateAdaptiveElement()
    {
        adaptiveContent = new VisualElement();
        Element element = RootMap.GetElementForPin(selectedPin);
        
        nameClickLabel = new ClickLabel(RootControl, OnNameLabelChange, OnNameLabelConfirm, OnNameLabelLoseFocus); 
        nameClickLabel.value = "" + selectedPin.Name;
        nameClickLabel.style.fontSize = 13f;
        FrontLabel nameFrontLabel = new FrontLabel(RootControl,false, "Pin Name", nameClickLabel);
        nameFrontLabel.style.marginBottom = 4f;
        nameFrontLabel.style.marginLeft = 1f;
        adaptiveContent.Add(nameFrontLabel);

        var elementLabel = new Label(element.Name);
        FrontLabel elementFrontLabel = new FrontLabel(RootControl, false,element.category.ToString(), elementLabel);
        elementFrontLabel.style.marginBottom = 5f;
        elementFrontLabel.style.marginLeft = 1f;
        
        adaptiveContent.Add(elementFrontLabel);

        if (element.category == Element.Category.Location)
        {
            Location loc = element as Location;

            SquareDropdown mapDropdown = new SquareDropdown(RootControl, "locationMapDropdown", OnLocationMapDropdownChange);
            FrontLabel dropdownFrontLabel = new FrontLabel(RootControl, true,"Map", mapDropdown);
            
            dropdownFrontLabel.style.marginBottom = 3f;
            dropdownFrontLabel.style.marginLeft = 1f;
            List<string> potentials = new List<string>();
            potentials.Add("None");
            potentials.AddRange(RootMap.GetUnpinnedMapNames());
            if (potentials.Contains(RootControl.Map.Name))
                potentials.Remove(RootControl.Map.Name);
            if (potentials.Contains(selectedPin.PinnedMap))
                potentials.Remove(selectedPin.PinnedMap);
            mapDropdown.choices = potentials;
            if (loc.Map == "")
                mapDropdown.value = "None";
            else 
                mapDropdown.value = loc.Map; 
            mapDropdown.style.marginLeft = 10f;
            mapDropdown.style.width = 100f;
            locationMapImage = new VisualElement();
         
            if (loc.Map == "")
            {
                locationMapImage.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                Texture2D mapTexture =RootMap.GetMapTexture(RootMap.GetMapForName(loc.Map).FileName);
               
                locationMapImage.style.backgroundImage = new StyleBackground(mapTexture);
                locationMapImage.style.height = 78f; 
                locationMapImage.RegisterCallback<MouseUpEvent>(evt =>
                {
                    RootControl.SetMap(RootMap.GetMapForName(loc.Map));
                });
            }
           
            adaptiveContent.Add(dropdownFrontLabel);
            adaptiveContent.Add(locationMapImage);

        }
        return adaptiveContent;
    }

    private void OnLocationMapDropdownChange(string value)
    {
        Location loc = RootMap.GetElementForPin(selectedPin) as Location;
        if (loc == null)
            Debug.LogWarning("!Null Location when casting " + selectedPin.Name);

        loc.Map = value;
        if (value == "None")
        {
            loc.Map = "";
            locationMapImage.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
        else
        {
            locationMapImage.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            Texture2D mapTexture =RootMap.GetMapTexture(RootMap.GetMapForName(value).FileName);
               
            locationMapImage.style.backgroundImage = new StyleBackground(mapTexture);
            locationMapImage.style.height = 78f; 
            locationMapImage.RegisterCallback<MouseUpEvent>(evt =>
            {
                RootControl.SetMap(RootMap.GetMapForName(selectedPin.PinnedMap));
            });
        }
          
    }

    private VisualElement CreateAdaptiveMap()
    {
        adaptiveContent = new VisualElement();

        // create unpinned maps dropdown with text label in front of it
        nameClickLabel = new ClickLabel(RootControl, OnNameLabelChange, OnNameLabelConfirm, OnNameLabelLoseFocus); 
        nameClickLabel.value = "" + selectedPin.Name;
        nameClickLabel.style.fontSize = 13f;
        FrontLabel nameFrontLabel = new FrontLabel(RootControl,false, "Pin Name", nameClickLabel);
        nameFrontLabel.style.marginBottom = 4f;
        nameFrontLabel.style.marginLeft = 1f;
        adaptiveContent.Add(nameFrontLabel);
        
        SquareDropdown mapDropdown = new SquareDropdown(RootControl, "maDropdown", OnPinnedMapDropdownChange);
        FrontLabel dropdownFrontLabel = new FrontLabel(RootControl, true,"Pinned Map", mapDropdown);
        dropdownFrontLabel.style.marginBottom = 3f;
        dropdownFrontLabel.style.marginLeft = 1f;
        List<string> potentials = new List<string>();
        potentials.Add(selectedPin.PinnedMap);
        potentials.AddRange(RootMap.GetUnpinnedMapNames());
      //  if (potentials.Contains(RootControl.Map.Name)) // allow multiple pins for same map for now
      //      potentials.Remove(RootControl.Map.Name);
        mapDropdown.choices = potentials;
        mapDropdown.value = selectedPin.PinnedMap;
        mapDropdown.style.marginLeft = 10f;
        mapDropdown.style.width = 100f;
        Texture2D mapTexture =RootMap.GetMapTexture(RootMap.GetMapForName(selectedPin.PinnedMap).FileName);
        mapImage = new VisualElement();
        mapImage.style.backgroundImage = new StyleBackground(mapTexture);
        mapImage.style.height = 96f; 
        mapImage.RegisterCallback<MouseUpEvent>(evt =>
        {
            RootControl.SetMap(RootMap.GetMapForName(selectedPin.PinnedMap));
        });
        
        adaptiveContent.Add(dropdownFrontLabel);
        adaptiveContent.Add(mapImage);
        return adaptiveContent;
    }

    private void OnPinnedMapDropdownChange(string value)
    {
        selectedPin.PinnedMap = value;
        Texture2D mapTexture =RootMap.GetMapTexture(RootMap.GetMapForName(selectedPin.PinnedMap).FileName);
        mapImage.style.backgroundImage = new StyleBackground(mapTexture);
    }

    private VisualElement CreateAdaptiveDescriptive()
    {
        adaptiveContent = new VisualElement();
        
        nameClickLabel = new ClickLabel(RootControl, OnNameLabelChange, OnNameLabelConfirm, OnNameLabelLoseFocus); 
        nameClickLabel.value = "" + selectedPin.Name;
        nameClickLabel.style.fontSize = 13f;
        FrontLabel nameFrontLabel = new FrontLabel(RootControl,false, "Pin Name", nameClickLabel);
        nameFrontLabel.style.marginBottom = 4f;
        nameFrontLabel.style.marginLeft = 1f;
        adaptiveContent.Add(nameFrontLabel);
        
        DescriptionField descrField = new DescriptionField(RootControl);
        descrField.style.marginTop = -1f;
        descrField.style.marginBottom = -1f;
        descrField.style.marginLeft = -0f;
        descrField.style.marginRight = -0f;
        descrField.style.height = 96f;
        descrField.value = selectedPin.Description;
        descrField.RegisterValueChangedCallback(evt =>
        {
            selectedPin.Description = evt.newValue; 
        });
        adaptiveContent.Add(descrField);
        return adaptiveContent;
    }
    private void OnZoomScaleDropdownChange(string value)
    {
        string numberStr = value.Substring(11);

        Debug.Log(numberStr);
        if (int.TryParse(numberStr, out int zoomScale))
        {
            Debug.Log("GO");

            selectedPin.Zoomscale = zoomScale;
            RegisterPinChange();
        }
    }

    private void OnNameLabelChange(string value)
    {
       // selectedPin.Name = value; 
    }

    private void OnNameLabelConfirm(string value)
    { 
        selectedPin.Name = value;
        RegisterPinChange();
    }
    private void OnNameLabelLoseFocus()
    { 
         nameClickLabel.SetValueWithoutNotify(selectedPin.Name);
    }

    private void UpdateUI()
    {
        // Populate UI elements with the details of the selected pin.
        // For example, update the label with the pin's name.
     

        // Add more UI update logic here.
    }


    private void RegisterPinChange()
    {
        // todo save world automatically ?
        RootControl.SaveWorldAfterPinChange();
    }
    
    
    
    /*// Called on RootControl.Pin change
    public void RefreshWindow()
    {
        if (RootControl.Pin == null)
            return;
        selectedPin = RootControl.Pin;
    }*/
    
    
    #region Getters
    private MonoLoader _monoLoader;
    private MonoLoader MonoLoader
    {
        get
        {
            if (_monoLoader == null)
                _monoLoader = UnityEngine.Object.FindObjectOfType<MonoLoader>(true);
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