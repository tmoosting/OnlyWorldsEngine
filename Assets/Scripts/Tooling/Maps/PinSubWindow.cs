using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using World_Model;
using Object = UnityEngine.Object;

public class PinSubWindow : VisualElement
{
    private int collapsedHeight = 26;
    
    
    // state variables
    private bool showSpecific = false;
    public bool isExtended = false;
    public PinElement tempPinElement;
    private List<PinElement> pinElements;
    
    private VisualElement windowContent;
    private WhiteToggle toggleExtendButton;
    private VisualElement mainContainer;
    private VisualElement mapScreenContainer;
    private VisualElement pinScreenContainer;
    private VisualElement permanentPinContainer;
    private VisualElement tempPinContainer;
    private Label headerLabel;
    
    public void ReceiveEscapeKey()
    {
        if (showSpecific)
            RefreshWindow();
        else
            if (isExtended)
                ClickToggleExtendButton(false);
    }

    public void ReceiveRightClick()
    { 
        if (isExtended == false)
        {
            ClickToggleExtendButton(true);
            // if pin isnot null, show that screen. otherwise show control scree
            if (RootControl.Pin != null )
                RefreshWindow(true, null, RootControl.Pin);
            else 
                RefreshWindow();
        }
        else
        {
            if (showSpecific)
                RefreshWindow();
            else
            if (isExtended)
                ClickToggleExtendButton(false);
        }
    }
    
    public PinSubWindow()
    {
        BuildWindow(); 
    }

    private void BuildWindow()
    {
        this.style.width = RootMap.pinWindowWidth;
        this.style.height = collapsedHeight;
        this.style.position = Position.Absolute;
        
        if (windowContent != null)
            windowContent.Clear();
        windowContent = new VisualElement();
        windowContent.style.backgroundColor = RootMap.pinWindowColor;
     //   windowContent.style.backgroundColor = new StyleColor(new Color(0.25f, 0.3f, 0.4f));
        windowContent.style.width = Length.Percent(100);
        windowContent.style.height = Length.Percent(100);
        
        windowContent.style.borderTopLeftRadius = 1;
        windowContent.style.borderTopRightRadius = 5;
        windowContent.style.borderBottomLeftRadius = 5;
        windowContent.style.borderBottomRightRadius = 5;
        windowContent.style.borderBottomColor = Color.black;
        windowContent.style.borderBottomWidth = 1f; 
        windowContent.style.borderRightColor = Color.black;
        windowContent.style.borderRightWidth = 1f;
        BuildContentBase();
        RefreshWindow();
        this.Add(windowContent);
    } 
    
    
    
    
    
    
    
    /// <summary>
    /// called on escape to close temp pin; on first window create; on temp pin create
    /// on new Pin selected; 
    /// </summary>
    /// <param name="showPinSpecific"></param>
    /// <param name="pinElement"></param>
    /// <param name="pin"></param>
    public void RefreshWindow(bool showPinSpecific = false, PinElement pinElement = null, Pin pin = null )
    {   
        if (pin != null)
        { 
            ClickToggleExtendButton(true);
            showSpecific = true;
            RootMap.RebuildPinElements(); 
            tempPinElement = RootMap.GetPinElementForPin(pin);
            SetMapScreenContainer(false);
            SetPinScreenContainer(true); 

            RefreshPinSpecific(tempPinElement);
            return;
        }

        if (isExtended == false)
            return;

        showSpecific = showPinSpecific; 
        tempPinElement = pinElement;

        // hide the two main content sections
        SetMapScreenContainer(false);
        SetPinScreenContainer(false); 
        
        if (showSpecific)
        { 
            RefreshPinSpecific(pinElement);
            SetPinScreenContainer(true); 
        }
        else
        { 
            headerLabel.text = "Controls";
            RefreshMapScreen();
            SetMapScreenContainer(true);
        }
    }
    public void SetPinScreenContainer(bool active)
    { 
        if (active)
            pinScreenContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else
            pinScreenContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    public void SetMapScreenContainer(bool active)
    {   
        if (active)
            mapScreenContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else
            mapScreenContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }

    private void BuildContentBase()
    {
        mainContainer = new VisualElement();
        mapScreenContainer = new VisualElement();
        pinScreenContainer = new VisualElement();
        
    var headerCombo = new VisualElement();
    headerCombo.style.height = new StyleLength(collapsedHeight);  // explicit height for header
    headerCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
    headerCombo.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
    headerCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
  //  headerCombo.style.marginBottom = new StyleLength(3f); 

    headerLabel = new Label("Controls");
    headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    headerLabel.style.fontSize = new StyleLength(14f);
    headerLabel.style.paddingLeft = new StyleLength(4);
    headerLabel.style.height = 20f;
    headerLabel.RegisterCallback<MouseUpEvent>(evt  =>
    {
        RefreshWindow();
    });

   toggleExtendButton = new WhiteToggle(RootControl , "Expand", ClickToggleExtendButton);
   toggleExtendButton.style.height = 20f;
    toggleExtendButton.style.marginRight = new StyleLength(4f); 
    toggleExtendButton.style.marginTop = new StyleLength(2f);  
    toggleExtendButton.style.marginBottom = new StyleLength(2f);  
    toggleExtendButton.style.fontSize = new StyleLength(13f); 


    mainContainer = new VisualElement();
    mainContainer.style.marginLeft = 2f; 
    mainContainer.style.marginRight = 2f;

    permanentPinContainer = new VisualElement( );
    permanentPinContainer.name = "permanentPinContainer";
    tempPinContainer = new VisualElement();
    tempPinContainer.name = "tempPinContainer"; 

    BuildMapScreen();
    BuildPinSpecific();
    SetMapScreenContainer(false);
    SetPinScreenContainer(false);
    mapScreenContainer.style.marginTop = 3f;
    pinScreenContainer.style.marginTop = 3f;

    headerCombo.Add(headerLabel);
    headerCombo.Add(toggleExtendButton);
    windowContent.Add(headerCombo);
    mainContainer.Add(mapScreenContainer);
    mainContainer.Add(pinScreenContainer);
    windowContent.Add(mainContainer);

}
    private void BuildPinSpecific()
    {
        CreateTempPinSpecific();
        pinScreenContainer.Add(tempPinContainer);
        pinScreenContainer.Add(permanentPinContainer);
    }


    private PinScreen pinScreen;
    private MapScreen mapScreen;
    private void BuildMapScreen()
    {
        mapScreen = new MapScreen();
        mapScreenContainer.Add(mapScreen); 
    }
    private void RefreshMapScreen()
    { 
        mapScreen.RefreshWindow();
    }
    
    private void RefreshPinScreen()
    {
        if (pinScreen != null)
            pinScreen.RemoveFromHierarchy();

        pinScreen = new PinScreen(this);
        permanentPinContainer.Add(pinScreen);

    }
    #region Specific - Temporary Pin

    // Global
    private Label pinTempLabel;
    private BlueTextField tempPinNameField;
    private SquareDropdown tempPinTypeDropdown;
    
    // Element
    private VisualElement tempPinElementContent ;
    private SquareDropdown tempPinElementTypeDropdown;
    private BlueTextField tempPinElementIDField;
    private WhiteButton tempPinElementCreateButton;


    // Descriptive
    private VisualElement tempDescriptionContent ;
    private WhiteButton tempPinDescriptiveCreateButton;
    private DescriptionField tempPinDescriptionField;

    // Map
    private  VisualElement tempMapContent ;
    private SquareDropdown tempPinMapDropdown;
    private WhiteButton tempPinMapCreateButton;

    
    private void CreateTempPinSpecific()
    { 
        float bottomSpacing = 2f;
        
        pinTempLabel = new Label("Creating Pin.."); 

        pinTempLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        pinTempLabel.style.marginBottom = bottomSpacing+2f;

        // Name field
        var pinNameCombo = new VisualElement();
        pinNameCombo.tooltip = "Set the Pin name, stored as part of the World and current Map";
        pinNameCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        pinNameCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        pinNameCombo.style.marginBottom = bottomSpacing;
        var pinNameLabel = new Label("Name");
        pinNameLabel.style.marginRight = new StyleLength(5f);
        tempPinNameField = new BlueTextField(RootControl);
        tempPinNameField.style.flexShrink = 1;
        tempPinNameField.style.width = Length.Percent(100);  
        tempPinNameField.RegisterValueChangedCallback(OnTempPinNameFieldChange);
        pinNameCombo.Add(pinNameLabel);
        pinNameCombo.Add(tempPinNameField);
        
        
        // Type Dropdown
        // Element Table Type below pin type dropdown
        // ID field, uninteractable, below that. new element when not linked
        // create and linkm buttons below that (ideally just one)
        var typeDropdownCombo = new VisualElement();
        typeDropdownCombo.tooltip = "Set the Pin type";
        typeDropdownCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        typeDropdownCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        typeDropdownCombo.style.marginBottom = bottomSpacing;
        var pinDropdownLabel = new Label("Type");
        pinDropdownLabel.style.marginRight = new StyleLength(9f);
    tempPinTypeDropdown = new SquareDropdown(RootControl,"tempPinTypeDropdown", OnTempPinCategoryDropdownChange);
        tempPinTypeDropdown.style.flexShrink = 1;
        tempPinTypeDropdown.style.width = Length.Percent(100); 
        List<string> typeDropdownChoices =  Enum.GetNames(typeof(Pin.Category  )).ToList();
        tempPinTypeDropdown.choices = typeDropdownChoices;
        tempPinTypeDropdown.value = typeDropdownChoices[0];
        typeDropdownCombo.Add(pinDropdownLabel);
        typeDropdownCombo.Add(tempPinTypeDropdown);
        
        // Triple Containers - Element
        tempPinElementContent = new VisualElement();

        // Triple Containers - Element - Type Dropdown
        var elementTypeDropdownCombo = new VisualElement();
        elementTypeDropdownCombo.tooltip = "Set the Element type";
        elementTypeDropdownCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        elementTypeDropdownCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        elementTypeDropdownCombo.style.marginBottom = bottomSpacing;
        var  elementTypeDropdownLabel = new Label("");
        elementTypeDropdownLabel.style.marginRight = new StyleLength(37f);
        tempPinElementTypeDropdown = new SquareDropdown(RootControl,"tempPinTypeDropdown", OnTempPinCategoryDropdownChange);
        tempPinElementTypeDropdown.style.flexShrink = 1;
        tempPinElementTypeDropdown.style.width = Length.Percent(100);
        tempPinElementTypeDropdown.RegisterValueChangedCallback(OnTempPinElementTypeDropdownChange);
        List<string> elementTypeDropdownChoices =  Enum.GetNames(typeof(Element.Category  )).ToList();
        tempPinElementTypeDropdown.choices = elementTypeDropdownChoices;
        tempPinElementTypeDropdown.value = elementTypeDropdownChoices[0];
        elementTypeDropdownCombo.Add(elementTypeDropdownLabel);
        elementTypeDropdownCombo.Add(tempPinElementTypeDropdown);

        // Triple Containers - Element - ID Field
        var tempElementIDCombo = new VisualElement();
        tempElementIDCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        tempElementIDCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        tempElementIDCombo.style.marginBottom = bottomSpacing;
        var tempElementIDLabel = new Label("ID"); 
        tempElementIDLabel.style.marginRight = new StyleLength(25f);
        tempPinElementIDField = new BlueTextField(RootControl);
        tempPinElementIDField.style.flexShrink = 1;
        tempPinElementIDField.value = "";
        tempPinElementIDField.SetEnabled(false);
        tempElementIDCombo.Add(tempElementIDLabel);
        tempElementIDCombo.Add(tempPinElementIDField);

        tempPinElementCreateButton = new WhiteButton(RootControl, "", ClickTempPinElementCreateButton); 
        tempPinElementCreateButton.style.width = Length.Percent(100);
        tempPinElementCreateButton.style.flexShrink = 1;
        tempPinElementCreateButton.style.marginTop = 4f;
        
        tempPinElementContent.Add(elementTypeDropdownCombo);
        tempPinElementContent.Add(tempElementIDCombo);
        tempPinElementContent.Add(tempPinElementCreateButton);

        // Triple Containers - Description
        tempDescriptionContent = new VisualElement();
        tempPinDescriptionField = new DescriptionField(RootControl);  
        tempPinDescriptionField.style.marginBottom = bottomSpacing;
        tempPinDescriptionField.style.height = 96f;
        
        tempPinDescriptiveCreateButton = new WhiteButton(RootControl, "Create Descriptive Pin", ClickTempPinDescriptiveCreateButton); 
        tempPinDescriptiveCreateButton.style.width = Length.Percent(100);
        tempPinDescriptiveCreateButton.style.flexShrink = 1;
        tempPinDescriptiveCreateButton.style.marginTop = 4f;

        tempDescriptionContent.Add(tempPinDescriptionField);
        tempDescriptionContent.Add(tempPinDescriptiveCreateButton);

        // Triple Containers - Map
        tempMapContent = new VisualElement();
        var tempMapCombo = new VisualElement();
        tempMapCombo.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row); 
        tempMapCombo.style.alignItems = new StyleEnum<Align>(Align.Center);
        tempMapCombo.style.marginBottom = bottomSpacing;
        var tempMapLabel = new Label("Map"); 
        tempMapLabel.style.marginRight = new StyleLength(13f);
        
        tempPinMapDropdown = new SquareDropdown(RootControl,"tempPinMapDropdown", null);
        tempPinMapDropdown.style.flexShrink = 1;
        tempPinMapDropdown.style.width = Length.Percent(100);
      //  UpdateTempPinMapContent();
        tempMapCombo.Add(tempMapLabel);
        tempMapCombo.Add(tempPinMapDropdown);
        
        
        tempPinMapCreateButton = new WhiteButton(RootControl, "Create Map Pin", ClickTempPinMapCreateButton);
        tempPinMapCreateButton.text = "Create Map Pin";
        tempPinMapCreateButton.style.width = Length.Percent(100);
        tempPinMapCreateButton.style.flexShrink = 1;
        tempPinMapCreateButton.style.marginTop = 4f;

        tempMapContent.Add(tempMapCombo);
        tempMapContent.Add(tempPinMapCreateButton);

        tempPinElementContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        tempDescriptionContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        tempMapContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        if (tempPinTypeDropdown.value == Pin.Category.Element.ToString())
            tempPinElementContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else       if (tempPinTypeDropdown.value == Pin.Category.Descriptive.ToString())
            tempDescriptionContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else       if (tempPinTypeDropdown.value == Pin.Category.Map.ToString())
            tempMapContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

         
        tempPinContainer.Add(pinTempLabel);
        tempPinContainer.Add(pinNameCombo);
        tempPinContainer.Add(typeDropdownCombo);
        tempPinContainer.Add(tempPinElementContent);
        tempPinContainer.Add(tempDescriptionContent);
        tempPinContainer.Add(tempMapContent); 
    }

    private void OnTempPinCategoryDropdownChange(string value)
    {
        tempPinElementContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        tempDescriptionContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        tempMapContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        if (value == Pin.Category.Element.ToString())
        {
            tempPinElementContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }
        else if (value == Pin.Category.Descriptive.ToString())
        {
            tempDescriptionContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            tempPinNameField.SetEnabled(true);
            tempPinNameField.SetValueWithoutNotify("");
        }
        else if (value == Pin.Category.Map.ToString())
        {
            tempMapContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            tempPinNameField.SetEnabled(true);
            tempPinNameField.SetValueWithoutNotify("");
            UpdateTempPinMapContent();
        }
    }
  

 
  
    private void ClickTempPinElementCreateButton()
    {
        if (tempPinIsLink)
        {
            /*Element element = RootControl.Element;
            if (RootMap.DoesElementHavePinOnMap(element.ID, RootControl.Map.ID))
            {
                RootControl.SetPin( RootMap.GetPinForElement(element.ID, RootControl.Map.ID));
                RootMap.AttemptSelectAndCenterCameraOnPinForElement(element);
            }
            else*/
                RootMap.CreateLinkedElementPin(tempPinElement,  RootControl.Element);
        }
        else
            RootMap.CreateElementAndPin(tempPinElement,  tempPinNameField.text, tempPinElementTypeDropdown.value);
        tempPinNameField.value = "";
    }
    private void ClickTempPinDescriptiveCreateButton()
    {
      RootMap.CreateDescriptivePin(tempPinElement,  tempPinNameField.text, tempPinDescriptionField.text);
      tempPinNameField.value = "";
      tempPinDescriptionField.value = "";
      tempPinTypeDropdown.value = Pin.Category.Element.ToString();
    }
    private void ClickTempPinMapCreateButton()
    {
        RootMap.CreateMapPin(tempPinElement, tempPinNameField.text, RootMap.GetMapForName(tempPinMapDropdown.value));
        tempPinNameField.value = "";
        tempPinTypeDropdown.value = Pin.Category.Element.ToString(); 
    } 
    private void OnTempPinElementTypeDropdownChange(ChangeEvent<string> evt)
    {
    }
    private void OnTempPinNameFieldChange(ChangeEvent<string> evt)
    {
        if (evt.newValue == "")
        {
            tempPinElementCreateButton.SetEnabled(false);
            tempPinDescriptiveCreateButton.SetEnabled(false);
            tempPinMapCreateButton.SetEnabled(false);
        }
        else
        {
            tempPinElementCreateButton.SetEnabled(true);
            tempPinDescriptiveCreateButton.SetEnabled(true);
            tempPinMapCreateButton.SetEnabled(true);
        }
    }
    private void RefreshPinTempContent()
  {
      if (tempPinTypeDropdown.value == Pin.Category.Element.ToString())
            UpdateTempPinElementContent();
      if (tempPinTypeDropdown.value == Pin.Category.Descriptive.ToString())
          UpdateTempPinDescriptiveContent();
      if (tempPinTypeDropdown.value == Pin.Category.Map.ToString()) 
        UpdateTempPinMapContent();
  }
    private void UpdateTempPinElementContent()
    {
        pinTempLabel.text ="Creating Pin at x:" + tempPinElement.relativeX.ToString("F3").Substring(2) + " y:" + tempPinElement.relativeY.ToString("F3").Substring(2);

        if (RootControl.Element == null)
            UpdateTempPinElementContentForNullElement();
        else
        {
            if  (RootMap.DoesElementHavePinOnMap(RootControl.Element.ID, RootControl.Map.ID))
                UpdateTempPinElementContentForNullElement();
            else
                UpdateTempPinElementContentForExistingElement();
        }
    }

    private void UpdateTempPinElementContentForNullElement()
    {
        tempPinIsLink = false;
        tempPinNameField.value = "";
        tempPinNameField.SetEnabled(true);
        tempPinElementIDField.value = "new";
    //    tempPinElementTypeDropdown.SetValueWithoutNotify(Element.Table.Location.ToString());
        tempPinElementTypeDropdown.SetEnabled(true);
        
        tempPinElementCreateButton.text = "Create Element and Pin";
        tempPinElementCreateButton.SetEnabled(false);
    }

    private bool tempPinIsLink = false;
    private void UpdateTempPinElementContentForExistingElement()
    { 
        tempPinIsLink = true;
        Element element = RootControl.Element;
        tempPinNameField.value = element.Name;
        tempPinNameField.SetEnabled(false);
        tempPinElementIDField.value = element.ID;
        tempPinElementTypeDropdown.SetValueWithoutNotify(element.category.ToString());
        tempPinElementTypeDropdown.SetEnabled(false);
        tempPinElementCreateButton.text = "Create Pin for Element";
    }
  private void UpdateTempPinDescriptiveContent()
  {
  
  }
  private void UpdateTempPinMapContent()
  {
      if (RootControl.Map == null)
          return; 
      List<string> pinMapNames = new List<string>(); 
    
      foreach (var map in RootMap.OrderMapList( RootControl.World.Maps))
          if (map != null)
              if (map.ID != RootControl.Map.ID)
                  pinMapNames.Add(map.Name);
      tempPinMapDropdown.choices = pinMapNames;
      if (pinMapNames.Count > 0)
      {
          tempPinMapDropdown.value = pinMapNames[^1];
          tempPinMapCreateButton.SetEnabled(true);
          tempPinMapDropdown.SetEnabled(true);
      }
      else
      {
          tempPinMapDropdown.value = "No maps available";
          tempPinMapCreateButton.SetEnabled(false);
          tempPinMapDropdown.SetEnabled(false);
      }
  } 
#endregion

//todo-refactor ugh
    private void RefreshPinSpecific(PinElement pinElement = null)
    { 
        if (pinElement != null)
            tempPinElement = pinElement;
        if (tempPinElement != null)
           pinTempLabel.text ="Creating Pin at x:" + tempPinElement.xCoord + " y:" + tempPinElement.yCoord;
 
        permanentPinContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        tempPinContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

        if (tempPinElement == null)
        { 
            if ( RootControl.Pin != null)
            { 
                headerLabel.text =  RootControl.Pin.category + " Pin";
                permanentPinContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                RefreshPinScreen();
            } 
        }
        else
        { 
            if (tempPinElement.temporary)
            { 
                headerLabel.text = "Create Pin";
                tempPinContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                RefreshPinTempContent();
            }
            else if ( RootControl.Pin != null)
            {  
                headerLabel.text =  RootControl.Pin.category + " Pin";
                permanentPinContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                RefreshPinScreen();
            }
        }
        
    }
    

    private string TruncateString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        if (input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength - 3) + "...";
    }

   

    /// <summary>
    ///  called when a new Element is selected through MapWindow's refresh system
    /// </summary>
    public void NewElementSelected()
    {
        if (isExtended == false)
            return;
        if (showSpecific == false)
            return;
        if (tempPinElement != null)
           RefreshPinSpecific();
    }

    public void ElementDeselected()
    {
        if (isExtended == false)
            return;
        if (showSpecific == false)
            return;
        RefreshWindow();
        RootMap.RebuildPinElements();
    }
    

    // toggle the window extending downwards
    public void ClickToggleExtendButton(bool toggled)
    {  
        isExtended = toggled;

        if (isExtended)
        {

            this.style.height = new Length(100, LengthUnit.Percent); 
            if (tempPinElement != null)
                SetPinScreenContainer(true);
            else
                SetMapScreenContainer(true);
        }
        else
        { 
         //   headerLabel.text = "Controls";
            this.style.height = collapsedHeight; 
            SetMapScreenContainer(false);
            SetPinScreenContainer(false); 

        }
        toggleExtendButton.SetActive(isExtended);  

        // Refresh the main content according to current mode
   //     RefreshWindow(showSpecific); 
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