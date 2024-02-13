using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using World_Model;
using Object = UnityEngine.Object;

public class MapScreen : VisualElement
{
    private SearchBox pinSearchBox;

    private VisualElement pinResultsContent;
    private VisualElement pinSortBar;
    private VisualElement pinMaskFieldBar;
    private VisualElement pinResultsContainer;
    private ListView listView;
    private SquareDropdown dropdownSortOne;
    private SquareDropdown dropdownSortTwo;
    private SquareMaskField elementMaskField;
    private SimpleSlider scaleSlider;
    
    public MapScreen()
    {
        float bottomSpacing = 3f;
        RootMap.OnPinListDeselect -= RefreshPinBrowserCall;
        RootMap.OnPinListDeselect += RefreshPinBrowserCall;
        SimpleToggle nameToggle = new SimpleToggle(
            RootControl,
            "Show Names",
            () => RootMap.showNameFlags,           
            newValue => RootMap.showNameFlags = newValue,  
            newVal => OnSettingsChange()
        );
        nameToggle.style.marginBottom = bottomSpacing;  
        this.Add(nameToggle);
        SimpleToggle zoomscaleToggle = new SimpleToggle(
            RootControl,
            "Hide on Zoom",
            () => RootMap.useZoomscales,           
            newValue => RootMap.useZoomscales = newValue,  
            newVal => OnSettingsChange()
        );
        zoomscaleToggle.style.marginBottom = bottomSpacing;  
        this.Add(zoomscaleToggle);
        
        SimpleToggle rescaleToggle = new SimpleToggle(
            RootControl,
            "Constant Scale",
            () => RootMap.constantPinScale,           
            newValue => RootMap.constantPinScale = newValue,  
            newVal => OnSettingsChange()
        );
        rescaleToggle.style.marginBottom = bottomSpacing;  
        this.Add(rescaleToggle);
        
        scaleSlider = new SimpleSlider(
            RootControl,
            RootMap.pinWindowWidth,
            "Scale",
            () => RootMap.pinScaling,           
            newValue => RootMap.pinScaling = newValue,  
            newVal => OnSettingsChange(),
            RootMap.pinMinScaling,
            RootMap.pinMaxScaling
        );
        scaleSlider.style.marginBottom = bottomSpacing;
        
        this.Add(scaleSlider);

        BuildPinBrowser();
        this.Add(pinResultsContent);
    }

 

    const string elementCategory = "Cat: Element";
    const string mapCategory = "Cat: Map";
    const string descriptionCategory = "Cat: Descrip";
    private void BuildPinBrowser()
    {
        _elementMaskFieldApplied = false;
        pinResultsContent = new VisualElement(); 
        pinResultsContainer = new VisualElement();
        pinResultsContainer.style.backgroundColor = RootControl.ResultsColorContainer; 
        pinResultsContainer.style.marginLeft = -1f;
        pinResultsContainer.style.marginRight = -1f;
        pinResultsContainer.style.borderTopWidth = 0f;
        pinResultsContainer.style.borderLeftWidth = 1f;
        pinResultsContainer.style.borderRightWidth = 1f;
        pinResultsContainer.style.borderBottomWidth = 1f;
        pinResultsContainer.style.borderLeftColor = new Color(.6f, .6f, .6f);
        pinResultsContainer.style.borderRightColor = new Color(.6f, .6f, .6f);
        pinResultsContainer.style.borderBottomColor = new Color(.6f, .6f, .6f);
        pinResultsContainer.AddToClassList("backgroundContainer");
        var stylesheet =   AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath +"UI/USS/SelectorStyleSheet.uss"); 
        pinResultsContainer.styleSheets.Add(stylesheet);

        pinSortBar = new VisualElement();
        pinSortBar.style.marginLeft = -1f;
        pinSortBar.style.marginRight = -1f;
        pinSortBar.style.marginTop = 10f;    
        pinSortBar.style.backgroundColor = RootMap.pinSortBarColor;
        pinSortBar.style.height = RootControl.RootMap.ResultRowHeight;
        pinSortBar.style.borderTopWidth = 1f;
        pinSortBar.style.borderLeftWidth = 1f;
        pinSortBar.style.borderRightWidth = 1f;
        pinSortBar.style.borderTopColor  = new Color(.6f, .6f, .6f);
        pinSortBar.style.borderLeftColor = new Color(.6f, .6f, .6f);
        pinSortBar.style.borderRightColor = new Color(.6f, .6f, .6f);
        pinSortBar.style.borderTopLeftRadius = 5f;
        pinSortBar.style.borderTopRightRadius = 5f;
        pinSortBar.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        pinSortBar.style.justifyContent = new StyleEnum<Justify>(Justify.FlexStart);
        pinSortBar.style.alignItems = new StyleEnum<Align>(Align.Center);
        dropdownSortOne = new SquareDropdown( RootControl,"sortDropdownOne",  OnSortDropdownOneChange);
        dropdownSortTwo = new SquareDropdown( RootControl,"sortDropdownTwo",  OnSortDropdownTwoChange);
        dropdownSortOne.style.width = Length.Percent(47);
        dropdownSortTwo.style.width = Length.Percent(47);
        var choicesDropdownOne = new List<string>();
        var choicesDropdownTwo = new List<string>();
        choicesDropdownOne.Add("Name");
        choicesDropdownOne.Add("Zoomscale"); 
        choicesDropdownOne.Add("Category");
    
        choicesDropdownOne.Add(elementCategory);
        choicesDropdownOne.Add(mapCategory);
        choicesDropdownOne.Add(descriptionCategory);
        choicesDropdownTwo.Add("Ascend");
        choicesDropdownTwo.Add("Descend");
        choicesDropdownTwo.Add("Include");
        choicesDropdownTwo.Add("Exclude");
        dropdownSortOne.choices = choicesDropdownOne;
        dropdownSortTwo.choices = choicesDropdownTwo;
        dropdownSortOne.value = choicesDropdownOne[0];
        dropdownSortTwo.value = choicesDropdownTwo[0];
        pinSortBar.Add(dropdownSortOne);
        pinSortBar.Add(dropdownSortTwo);
        pinResultsContent.Add(pinSortBar);
        pinResultsContent.Add(pinResultsContainer);
        
        
        pinMaskFieldBar = new VisualElement();
        pinMaskFieldBar.style.marginLeft = -1f;
        pinMaskFieldBar.style.marginRight = -1f; 
        pinMaskFieldBar.style.backgroundColor = RootMap.pinSortBarColor;
        pinMaskFieldBar.style.height = RootControl.RootMap.ResultRowHeight;
        pinMaskFieldBar.style.borderTopWidth = 0f;
        pinMaskFieldBar.style.borderLeftWidth = 1f;
        pinMaskFieldBar.style.borderRightWidth = 1f;
        pinMaskFieldBar.style.borderBottomWidth = 1f;
        pinMaskFieldBar.style.borderBottomLeftRadius = 5f;
        pinMaskFieldBar.style.borderBottomRightRadius = 5f;
        pinMaskFieldBar.style.borderBottomColor = new Color(.6f, .6f, .6f);
        pinMaskFieldBar.style.borderLeftColor = new Color(.6f, .6f, .6f);
        pinMaskFieldBar.style.borderRightColor = new Color(.6f, .6f, .6f); 
        pinMaskFieldBar.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        pinMaskFieldBar.style.justifyContent = new StyleEnum<Justify>(Justify.FlexStart);
        pinMaskFieldBar.style.alignItems = new StyleEnum<Align>(Align.Center);

        elementMaskField = new SquareMaskField(RootControl, "ElementMaskField", OnElementMaskFieldChange);
        elementMaskField.style.width = Length.Percent(95);
        elementMaskField.choices = Enum.GetNames(typeof (Element.Category)).ToList();
        var allValues = Enum.GetValues(typeof(Element.Category)).Cast<Element.Category>().Aggregate((current, next) => current | next);
        int allValuesInt = (int)allValues;
        elementMaskField.value = allValuesInt; 
    
        pinMaskFieldBar.Add(elementMaskField);
        pinResultsContent.Add(pinMaskFieldBar);
        ToggleElementMaskField(false);

        RefreshPinBrowser();
    }

    private void OnElementMaskFieldChange()
    {
        RootMap.includedElementTables = GetActiveElementsFromMaskField(); 
        RefreshPinBrowser();
        RootMap.RebuildPinElements();
    }

    private void OnSortDropdownOneChange(string value)
    {
        if (dropdownSortOne.value == elementCategory && dropdownSortTwo.value != "Exclude")
            ToggleElementMaskField(true);
        else
            ToggleElementMaskField(false); 
        RefreshPinBrowser();
        if (dropdownSortOne.value == elementCategory)
        {
            RootMap.filterMode = RootMap.FilterMode.Element;
            RootMap.includedElementTables = GetActiveElementsFromMaskField();
        }
        else   if (dropdownSortOne.value == mapCategory)
            RootMap.filterMode = RootMap.FilterMode.Map;
        else   if (dropdownSortOne.value == descriptionCategory)
            RootMap.filterMode = RootMap.FilterMode.Descriptive;
        else
            RootMap.filterMode = RootMap.FilterMode.None;
    
        RootMap.RebuildPinElements();
    }

    private void OnSortDropdownTwoChange(string value)
    {
        if (dropdownSortOne.value == elementCategory && dropdownSortTwo.value != "Exclude")
            ToggleElementMaskField(true);
        else
            ToggleElementMaskField(false); 
        RefreshPinBrowser();
    }

    private void RefreshPinBrowserCall()
    {
        RefreshPinBrowser();
    }
    private void RefreshPinBrowser(bool resetLastClickedPin = true)
    {
        if (resetLastClickedPin)
             lastClickedPin = null;
        pins = new List<Pin>();
        if (RootControl.World == null || RootControl.Map == null )
            return;
        if (pinResultsContainer == null)
            pinResultsContainer = new VisualElement();
        pinResultsContainer.Clear();

        foreach (var pin in RootControl.World.Pins.Where(pin => pin.Map == RootControl.Map.ID))
            pins.Add(pin);

        pins = SortPinList().ToList();
        if (_elementMaskFieldApplied)
            pins = FilterElementPins().ToList();
        listView = CreatePinBrowser();
        pinResultsContainer.Add(listView);
    }


    private List<Pin> FilterElementPins()
    {
        List<Pin> tempList = new List<Pin>();
        List<Element.Category> activeList = GetActiveElementsFromMaskField(); 
        foreach (var pin in pins)
        {
            if (pin.category == Pin.Category.Element)
            {
                if ((elementMaskField.value & (int)RootMap.GetElementForPin(pin).category) != 0)
                    tempList.Add(pin);
            }
            else
                tempList.Add(pin);
        } 
        return tempList;
    }
    
    private bool _elementMaskFieldApplied = false;
    private void ToggleElementMaskField(bool enable)
    {
        _elementMaskFieldApplied = enable;
        if (enable)
            pinMaskFieldBar.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else
            pinMaskFieldBar.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    private List<Element.Category> GetActiveElementsFromMaskField()
    {
        List<Element.Category> activeElements = new List<Element.Category>();
        foreach (Element.Category element in Enum.GetValues(typeof(Element.Category)))
            if ((elementMaskField.value & (int)element) != 0)
                activeElements.Add(element);
        return activeElements;
    }
    
    private List<Pin> pins = new List<Pin>();
   private List<Pin> SortPinList()
{
    var tempList = new List<Pin>(pins);

    string criteria = dropdownSortOne.value;
    string orderOrFilter = dropdownSortTwo.value;

    // Sort based on criteria and order
    switch (criteria)
    {
        case "Name":
            tempList = (orderOrFilter == "Ascend") 
                ? tempList.OrderBy(pin => pin.Name).ToList() 
                : tempList.OrderByDescending(pin => pin.Name).ToList();
            break;

        case "Zoomscale":
            tempList = (orderOrFilter == "Ascend")
                ? tempList.OrderBy(pin => pin.Zoomscale).ToList()
                : tempList.OrderByDescending(pin => pin.Zoomscale).ToList();
            break;

        case "Category":
            tempList = tempList.OrderBy(pin => pin.category).ToList();
            if (orderOrFilter == "Descend") 
                tempList.Reverse();
            break;

        case elementCategory:
            if (orderOrFilter == "Include")
                tempList = tempList.Where(pin => pin.category == Pin.Category.Element).ToList();
            else if (orderOrFilter == "Exclude")
                tempList = tempList.Where(pin => pin.category != Pin.Category.Element).ToList();
            break;

        case mapCategory:
            if (orderOrFilter == "Include")
                tempList = tempList.Where(pin => pin.category == Pin.Category.Map).ToList();
            else if (orderOrFilter == "Exclude")
                tempList = tempList.Where(pin => pin.category != Pin.Category.Map).ToList();
            break;

        case descriptionCategory:
            if (orderOrFilter == "Include")
                tempList = tempList.Where(pin => pin.category == Pin.Category.Descriptive).ToList();
            else if (orderOrFilter == "Exclude")
                tempList = tempList.Where(pin => pin.category != Pin.Category.Descriptive).ToList();
            break;
    }

    return tempList;
}






    private Pin lastClickedPin = null;
    private ListView CreatePinBrowser()
    {
        VisualElement MakeItem() => new ResultRowPin();

        void BindItem(VisualElement e, int i)
        {
            ResultRowPin row = (ResultRowPin)e;
            Pin pin = pins[i];
            row.UploadPin(pin, RootControl, i);

            if (RootControl.Pin != null)
            {
                if (pin == RootControl.Pin)
                    row.Highlight(1);
            }
            
            if (lastClickedPin != null)
               if (lastClickedPin == pin)
                   if (pin != RootControl.Pin)
                        row.Highlight(2); 
       

            e.RegisterCallback<ClickEvent>(evt => ClickPinRow(evt, pin, row));
        }
        return new ListView(pins, RootControl.RootMap.ResultRowHeight, MakeItem, BindItem)
        {
            selectionType = SelectionType.Single,
            reorderable = true,
            style =
            { 
                height = pins.Count *  RootControl.RootMap.ResultRowHeight   ,
            }
        };
    }

    private void ClickPinRow(ClickEvent evt, Pin pin, ResultRowPin resultRow)
    { 
        if (RootControl.Pin == null)
            resultRow.Highlight(2);
        else if (RootControl.Pin != pin)
            resultRow.Highlight(2);
        
        if (lastClickedPin == pin)
        {
             RootControl.SetPin(pin);
             lastClickedPin = null;
             RootMap.RebuildPinElements();
        }
        else
        {
            lastClickedPin = pin;
            RootMap.SecondaryHighlightSinglePin(lastClickedPin);
        }
        RefreshPinBrowser(false);
    }


    private void OnSettingsChange()
    {
        RootMap.RebuildPinElements();
    }
    
    
    // called on pin change, no refresh required atm (?)
    public void RefreshWindow()
    {
        RefreshPinBrowser();
        scaleSlider.SetValueWithoutNotify(RootMap.pinScaling);
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