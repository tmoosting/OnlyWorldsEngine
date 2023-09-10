using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements; 

// creation zone for custom elements

public class ElementFactory  
{

    
}



public class SquareMaskField : MaskField
{
    
      public string Name;
    private RootControl RootControl;
    private Action onValueChanged;
    private VisualElement arrowElement;
    
    public SquareMaskField( RootControl rootControl, string name, Action onValueChanged) : base()
    {
        Name = name;
        RootControl = rootControl;
        this.onValueChanged = onValueChanged;
        SetupStyles();
    }

     

    private void SetupStyles()
    { 
        this.style.backgroundImage = null;

        this.style.height = RootControl.dropdownHeight;
        this.style.width = RootControl.dropdownWidth;
        this.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);

        float borderSize = 1f;
        Color borderColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        this.style.borderTopColor = borderColor;
        this.style.borderTopWidth = borderSize;
        this.style.borderBottomColor = borderColor;
        this.style.borderBottomWidth = borderSize;
        this.style.borderLeftColor = borderColor;
        this.style.borderLeftWidth = borderSize;
        this.style.borderRightColor = borderColor;
        this.style.borderRightWidth = borderSize;

        var body = this.Q(null, "unity-base-popup-field__input");
        if (body != null)
            body.style.backgroundColor = new Color(0, 0, 0, 0);
        arrowElement = this.Q(null, "unity-base-popup-field__arrow");
        if (arrowElement != null)
        {
            arrowElement.style.backgroundImage =  new StyleBackground(RootControl.maskfieldArrow.texture);
            arrowElement.style.width = RootControl.dropdownArrowSize;
            arrowElement.style.height = RootControl.dropdownArrowSize;
        } 
        
        var dropdownButton = this.Q("unity-dropdown");
        if (dropdownButton != null)
        {
            dropdownButton.style.backgroundImage = null;
            dropdownButton.style.color = Color.white;
        }

        this.RegisterValueChangedCallback(evt =>
        {
            onValueChanged.Invoke();
         });
        
    }
}







public static class ClickHandler
{
    public static Clickable CreateClickableWithButtonCheck(Action leftClickAction, Action rightClickAction)
    {
        return new Clickable((e) =>
        {
            var mouseEvent = e as IMouseEvent;
            if (mouseEvent == null) return;

            switch (mouseEvent.button)
            {
                case 0: 
                    leftClickAction?.Invoke();
                    break;
                case 1:  
                    rightClickAction?.Invoke();
                    break;
            }
        });
    }
}

 


// different colored section as background for separate UI set
public class SubContainer : VisualElement
{
    public SubContainer()
    {
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath + "UI/USS/SelectorStyleSheet.uss");
        this.styleSheets.Add(stylesheet);
        style.width = Length.Percent(100);
        style.height = 80f;
        this.AddToClassList("subContainer"); 
    }
}


public class ClickLabel : TextField
{
    private RootControl RootControl;
    private bool enterKeyPressed = false; 
    public ClickLabel(RootControl rootControl, Action<string> onTextChanged, Action<string> onPressEnter, Action OnLoseFocus)
    {
        RootControl = rootControl;

        SetupStyles();
        RegisterCallbacks(onTextChanged,onPressEnter, OnLoseFocus );
        
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath + "UI/USS/SelectorStyleSheet.uss");
        this.styleSheets.Add(stylesheet);
        this.AddToClassList("clickLabel"); // todo make the focus color 
        this.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.BoldAndItalic);

    }

 

    private void SetupStyles()
    {
        style.fontSize = new StyleLength(13f);
      //  style.borderBottomWidth = 1f;
    //   style.borderBottomColor = Color.white; 
        style.height = RootControl.clickLabelHeight;
       // style.width = Length.Percent(100);
        style.backgroundColor = Color.clear;
        VisualElement textBox = this.Q("unity-text-input"); 
        textBox.style.backgroundColor = Color.clear;
        textBox.style.borderTopLeftRadius = 0f;
        textBox.style.borderTopRightRadius = 0f;
        textBox.style.borderBottomLeftRadius = 0f;
        textBox.style.borderBottomRightRadius = 0f;
        textBox.style.borderTopWidth = 0f;
        textBox.style.borderLeftWidth = 0f;
        textBox.style.borderRightWidth = 0f;
        textBox.style.borderBottomWidth = 0f;
    }
    public void SetValue(string value)
    {
        //todo-bug where the subelement shifts to negative x when a longer value is added and focus then lost
        VisualElement textElement = this.Q("unity-text-element--inner-input-field-component");
        if (textElement != null) // can't query this way, is null..
            textElement.style.left = 4f;
        SetValueWithoutNotify(value);
    }
    private void RegisterCallbacks(Action<string> onTextChanged, Action<string> onPressEnter, Action OnLoseFocus)
    {
        this.RegisterValueChangedCallback(evt =>
        {
            if (onTextChanged == null)
                return; 
            onTextChanged(this.value);  
        });
        this.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (onPressEnter == null)
                return;
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                enterKeyPressed = true;
                onPressEnter(this.value);
            }
        });
        RegisterCallback<BlurEvent>(evt =>
        {
            if(enterKeyPressed)
            {
                enterKeyPressed = false;  
                return;  
            }

            if (OnLoseFocus == null)
                return;
            OnLoseFocus.Invoke();
        });
    }


   
}
public class BlackLabel : Label
{ 
    public BlackLabel()
    {
        style.color = Color.black;
    } 
}
public class BoldLabel : Label
{ 
    public BoldLabel()
    {
        style.unityFontStyleAndWeight = FontStyle.Bold;
        style.fontSize = new StyleLength(13f);
    } 
}

/// <summary>
/// for at top of sections etc
/// </summary>
public class HeaderLabel : Label
{ 
    public HeaderLabel()
    {
        style.unityFontStyleAndWeight = FontStyle.Bold;
        style.fontSize = new StyleLength(15f);
    } 
}

 
public class WhiteToggle : Button
{
    //todo integrate color scheme or at least color variables
    public bool Activated = false; 
    private RootControl RootControl;
    private Color hoverActivatedColor = new Color(0.8f, 0.8f, 0.8f);  // Slightly darker when activated
    private Color hoverNotActivatedColor = new Color(.6f, .6f, .6f); // Slightly lighter when not activated

    public WhiteToggle(RootControl rootControl, string buttonText, Action<bool> onToggleAction)
    {//
        RootControl = rootControl;
        this.text = buttonText;
        
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath + "UI/USS/SelectorStyleSheet.uss");
        this.styleSheets.Add(stylesheet);
        this.AddToClassList("whiteToggleOff"); // Attach the stylesheet class to the button
        this.style.backgroundColor = Color.clear; 
        this.style.width = RootControl.toggleWidth;
        this.style.height = RootControl.toggleHeight;
        
        this.RegisterCallback<MouseEnterEvent>((evt) =>
        {
            if (Activated)
            {
                this.style.borderTopColor = hoverActivatedColor;
                this.style.borderBottomColor = hoverActivatedColor;
                this.style.borderLeftColor = hoverActivatedColor;
                this.style.borderRightColor = hoverActivatedColor;
            }
            else
            {
                this.style.borderTopColor = hoverNotActivatedColor;
                this.style.borderBottomColor = hoverNotActivatedColor;
                this.style.borderLeftColor = hoverNotActivatedColor;
                this.style.borderRightColor = hoverNotActivatedColor;
            }
        });

        this.RegisterCallback<MouseLeaveEvent>((evt) =>
        {
            if (Activated)
            {
                this.style.borderTopColor = Color.white;
                this.style.borderBottomColor = Color.white;
                this.style.borderLeftColor = Color.white;
                this.style.borderRightColor = Color.white;
            }
            else
            {
                this.style.borderTopColor = Color.black;
                this.style.borderBottomColor = Color.black;
                this.style.borderLeftColor = Color.black;
                this.style.borderRightColor = Color.black;
            }

        });

        this.clicked += () =>
        {
            Activated = !Activated; // Toggle state
            onToggleAction(Activated);   // Callback with the current state

            SetActive(Activated);
        };
    }

    public void SetActive(bool active)
    {
        Activated = active;
        if (active)
        { 
            this.style.borderTopColor = Color.white;
            this.style.borderBottomColor = Color.white;
            this.style.borderLeftColor = Color.white;
            this.style.borderRightColor = Color.white;
            this.RemoveFromClassList("whiteToggleOff");
            this.AddToClassList("whiteToggleOn");
        }
        else
        { 
            this.style.borderTopColor = Color.black;
            this.style.borderBottomColor = Color.black;
            this.style.borderLeftColor = Color.black;
            this.style.borderRightColor = Color.black;
            this.RemoveFromClassList("whiteToggleOn");
            this.AddToClassList("whiteToggleOff");
        }
    }
}
public class WhiteButton : Button
{
    private RootControl RootControl;
    public WhiteButton(RootControl rootControl, string buttonText, Action onClickAction)
    {
        this.text = buttonText;
        this.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        RootControl = rootControl;
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath + "UI/USS/SelectorStyleSheet.uss");
        this.styleSheets.Add(stylesheet);
        this.AddToClassList("whiteButton"); // Attach the stylesheet class to the button
        this.style.width = RootControl.buttonWidth;
        this.style.height = RootControl.buttonHeight;
        this.style.backgroundColor = Color.clear;
        this.clicked -= onClickAction;
        this.clicked += onClickAction;
    }
}


public class SquareDropdown : DropdownField
{
    public string Name;
    private RootControl RootControl;
    private Action<string> onValueChanged;
    private VisualElement arrowElement;
    
    public SquareDropdown( RootControl rootControl, string name, Action<string> onValueChanged) : base()
    {
        Name = name;
        RootControl = rootControl;
        this.onValueChanged = onValueChanged;
        SetupStyles();
    }

    /*public SquareDropdown(string name, System.Collections.Generic.List<string> choices) : base(name)
    { 
        Name = name;
        value = choices.FirstOrDefault(); // Setting the first item as the default selected value.
        SetupStyles();
    }*/

    private void SetupStyles()
    { 
        this.style.backgroundImage = null;

        this.style.height = RootControl.dropdownHeight;
        this.style.width = RootControl.dropdownWidth;
        this.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);

        float borderSize = 1f;
        Color borderColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        this.style.borderTopColor = borderColor;
        this.style.borderTopWidth = borderSize;
        this.style.borderBottomColor = borderColor;
        this.style.borderBottomWidth = borderSize;
        this.style.borderLeftColor = borderColor;
        this.style.borderLeftWidth = borderSize;
        this.style.borderRightColor = borderColor;
        this.style.borderRightWidth = borderSize;

        var body = this.Q(null, "unity-base-popup-field__input");
        if (body != null)
            body.style.backgroundColor = new Color(0, 0, 0, 0);
        arrowElement = this.Q(null, "unity-base-popup-field__arrow");
        if (arrowElement != null)
        {
            arrowElement.style.marginRight = -3f;
            //    arrowElement.style.backgroundImage =  new StyleBackground(RootControl.dropdownArrow.texture);
            //    arrowElement.style.width = RootControl.dropdownArrowSize;
            //    arrowElement.style.height = RootControl.dropdownArrowSize;
            //    arrowElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        } 
        
        var dropdownButton = this.Q("unity-dropdown");
        if (dropdownButton != null)
        {
            dropdownButton.style.backgroundImage = null;
            dropdownButton.style.color = Color.white;
        }

        this.RegisterValueChangedCallback(evt =>
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(evt.newValue);
            // todo make styling of dropdown click-panel actually work  https://forum.unity.com/threads/change-styles-for-dropdownfield.1307496/  https://docs.unity3d.com/ScriptReference/UIElements.DropdownField.html 
        });
        
    }
    
}
 

/// <summary>
/// simple row that centers and spaces out its child elements
/// </summary>
public class SimpleRow : VisualElement
{
    public SimpleRow()
    {
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
      //  style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        style.alignItems = new StyleEnum<Align>(Align.Center); 
    }
}
/// <summary>
/// two halves, each of which can be hovered to extend sideways over the other
/// </summary>
public class SlideBar : VisualElement
{
    public VisualElement leftHalf;
    public VisualElement rightHalf;
    private VisualElement leftHiddenHalf;
    private VisualElement rightHiddenHalf;
    private bool leftToggled = false;
    private bool rightToggled = false;
    public SlideBar(RootView rootView, bool clickToSlide, Color leftColor, Color rightColor,
        VisualElement leftContainerOne, VisualElement leftContainerTwo, VisualElement rightContainerOne,
        VisualElement rightContainerTwo)
    {
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        style.alignItems = new StyleEnum<Align>(Align.Center);
        style.borderTopWidth = 0f;
        style.borderBottomWidth= 0f;
        style.borderLeftWidth = 0f;
        style.borderRightWidth = 0f;
        

        leftHalf = new VisualElement();
        rightHalf = new VisualElement();
        leftHalf.style.backgroundColor = leftColor;
        leftHalf.style.width = Length.Percent(100);
        leftHalf.style.height = Length.Percent(100);
        leftHalf.style.flexGrow = 1;
        rightHalf.style.backgroundColor = rightColor;
        rightHalf.style.width = Length.Percent(100);
        rightHalf.style.height = Length.Percent(100);
        rightHalf.style.flexGrow = 1;

        if (clickToSlide)
        {
            leftHalf.RegisterCallback<ContextClickEvent>(evt =>
            {
                if (leftToggled)
                    CloseLeft();
                else
                    OpenLeft(); 
            });
            rightHalf.RegisterCallback<ContextClickEvent>(evt =>
            {
                if (rightToggled)
                    CloseRight();
                else
                    OpenRight(); 
            });
        }
          // Adding hover events

          if (rootView.hoverOppositeSortBarField)
          {
              leftHalf.RegisterCallback<MouseEnterEvent>(evt => rightHalf.style.backgroundColor = new Color (rightColor.r, rightColor.g,rightColor.b, rightColor.a/1.1f));
              leftHalf.RegisterCallback<MouseLeaveEvent>(evt => rightHalf.style.backgroundColor = rightColor);
              rightHalf.RegisterCallback<MouseEnterEvent>(evt => leftHalf.style.backgroundColor = new Color (leftColor.r, leftColor.g,leftColor.b, leftColor.a/0.9f));
              rightHalf.RegisterCallback<MouseLeaveEvent>(evt => leftHalf.style.backgroundColor = leftColor);
          }
       


        leftHiddenHalf = new VisualElement();
        rightHiddenHalf = new VisualElement();
        leftHiddenHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        rightHiddenHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        leftHiddenHalf.Add(leftContainerTwo);
        rightHiddenHalf.Add(rightContainerTwo);
        leftHalf.Add(leftContainerOne);
        leftHalf.Add(leftHiddenHalf);
        rightHalf.Add(rightContainerOne);
        rightHalf.Add(rightHiddenHalf);
        this.Add(leftHalf);
        this.Add(rightHalf);
    }

    private void OpenLeft()
    {
        leftToggled = true;
        rightHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        leftHiddenHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
    }
    private void CloseLeft()
    {
        leftToggled = false;
        rightHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        leftHiddenHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    private void OpenRight()
    {
        rightToggled = true;
        leftHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        rightHiddenHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
    }

    private void CloseRight()
    {
        rightToggled = false;
        leftHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        rightHiddenHalf.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }  
    
}



  
/// <summary>
/// inputfield with some special styling and function
/// </summary>
public class SearchBox : TextField
{
    public string Name;

    private RootControl RootControl;
    // todo-qol override Focus() properly to immediately place cursor at end of line, perhaps with auto added space
  
    public SearchBox(RootControl rootControl, string name, Action<string> onTextChanged, Action<string> onPressEnter)
    {
        RootControl = rootControl;
        Name = name;
        this.label = "";
        this.value = "...";
        AddToClassList("inputBox");
        
        // Load and apply the stylesheet
        var stylesheet =   AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath +"UI/USS/SelectorStyleSheet.uss"); 
        if (stylesheet == null)
            Debug.Log("Null");
        styleSheets.Add(stylesheet);

        style.opacity = 0.85f;
        
        // set valid or invalid color depending on query 
        
        this.RegisterValueChangedCallback(evt =>
        {  
                onTextChanged(this.value);  
        });
        this.RegisterCallback<KeyUpEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                onPressEnter(this.value);
                //todo try add suggestions based on typed filters ?
            }
        });
        this.style.height = RootControl.RootView.ElementSelectorHeight;
        this.style.width = RootControl.RootView.ElementSelectorHeight;
        VisualElement textBox = this.Q("unity-text-input");
        style.backgroundColor = RootControl.BlueElementColor;
        textBox.style.fontSize = RootControl.RootView.searchFontSize;
        textBox.style.backgroundColor = RootControl.SearchColorInside;
        textBox.style.borderTopLeftRadius = 0f;
        textBox.style.borderTopRightRadius = 0f;
        textBox.style.borderBottomLeftRadius = 0f;
        textBox.style.borderBottomRightRadius = 0f;
        textBox.style.borderTopWidth = 0f;
        textBox.style.borderLeftWidth = 0f;
        textBox.style.borderRightWidth = 0f;
        textBox.style.borderBottomWidth = 0f;
     //   textBox.style.height = 22f; 

        style.opacity = 0.8f; 

    }
 
}


/// <summary>
/// list row for view window
/// </summary>
public class ResultRowElement : VisualElement
{ 
    public Label Label;
    private VisualElement _iconElement;
    private VisualElement _pinElement;
    private RootControl RootControl;
    private int index;
    private bool highlighted = false;
    public ResultRowElement()
    {
        this.style.flexDirection = FlexDirection.Row; // Set layout direction to Row
        this.style.alignItems = Align.Center; // Vertically center the children
        
        _iconElement = new VisualElement();
        this.Add(_iconElement);  
     
        Label = new Label();
        Label.style.flexGrow = 1;
        this.Add(Label);
        
        _pinElement = new VisualElement();
        _pinElement.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        this.Add(_pinElement);

        this.AddToClassList("resultRow"); // Assigning a class for USS styling
        
        
        this.RegisterCallback<MouseEnterEvent>(evt => { Hover(); });
        this.RegisterCallback<MouseLeaveEvent>(evt => { ExitHover(); });
    }
   
    public void UploadElement(Element element, RootControl rootControl, int index)
    {
        RootControl = rootControl;
        Label.text = element.Name;
        Label.style.marginLeft = RootControl.RootView.ResultRowGeneralMargin; 

        this.index = index;
        _iconElement.style.width = RootControl.RootView.ResultRowIconSize;
        _iconElement.style.height = RootControl.RootView.ResultRowIconSize; 
        _iconElement.style.marginLeft = RootControl.RootView.ResultRowIconMargin;
        _iconElement.style.backgroundImage = new StyleBackground(RootControl.RootView.GetSelectorSprite(element.table.ToString()).texture);
        if (RootControl.RootMap.DoesElementHavePinOnMap(element))
        {
            _pinElement.style.backgroundImage = new StyleBackground(RootControl.RootMap.pinWhiteSprite.texture);
            _pinElement.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            _pinElement.style.width = RootControl.RootView.ResultRowIconSize;
            _pinElement.style.height = RootControl.RootView.ResultRowIconSize;
            _pinElement.style.marginRight = 2f;
        }

        ResetStyle();
    }
 
    private void Hover()
    {
        if (highlighted == false)
            this.style.backgroundColor = index % 2 == 0 ? RootControl.ResultsColorListHoverDark : RootControl.ResultsColorListHoverLight;
    }
    private void ExitHover()
    {
            if (highlighted == false)
                ResetStyle(); 
    }
    public void Highlight(int colorNumber)
    {
        highlighted = true;
        if (colorNumber == 1)
               this.style.backgroundColor = RootControl.ResultsColorListHighlightOne;
        if (colorNumber == 2)
            this.style.backgroundColor = RootControl.ResultsColorListHighlightTwo;
    }
    private void ResetStyle()
    {
        highlighted = false;
        this.style.backgroundColor = index % 2 == 0 ? RootControl.ResultsColorListDark : RootControl.ResultsColorListLight;
    }
}
public class ResultRowPin : VisualElement
{ 
    public Label Label;
    private VisualElement _iconCategory; 
    private VisualElement _iconElement; 
    private RootControl RootControl;
    private int index;
    private bool highlighted = false;
    public ResultRowPin()
    {
        this.style.flexDirection = FlexDirection.Row; // Set layout direction to Row
        this.style.alignItems = Align.Center; // Vertically center the children
        
        _iconCategory = new VisualElement();
        this.Add(_iconCategory);   
      
        Label = new Label(); 
        this.Add(Label);
        _iconElement = new VisualElement();
        this.Add(_iconElement);
        
        this.AddToClassList("resultRow");  
        
        
        this.RegisterCallback<MouseEnterEvent>(evt => { Hover(); });
        this.RegisterCallback<MouseLeaveEvent>(evt => { ExitHover(); });
    }
   
    
    public void UploadPin(Pin pin, RootControl rootControl, int index)
    {
        RootControl = rootControl;
        Label.text = pin.Name  + " " + pin.Zoomscale;
        Label.style.marginLeft = RootControl.RootView.ResultRowGeneralMargin; 

        this.index = index;
        _iconCategory.style.width = RootControl.RootMap.ResultRowIconSize;
        _iconCategory.style.height = RootControl.RootMap.ResultRowIconSize; 
        _iconCategory.style.marginLeft = RootControl.RootMap.ResultRowIconMargin;  
        _iconElement.style.width = RootControl.RootMap.ResultRowIconSize;
        _iconElement.style.height = RootControl.RootMap.ResultRowIconSize; 
        _iconElement.style.marginLeft = RootControl.RootMap.ResultRowIconMargin;

        _iconElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        if (pin.category == Pin.Category.Element)
        {
            _iconCategory.style.backgroundImage = new StyleBackground(RootControl.RootMap.pinRowIconElement.texture);
            _iconElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _iconElement.style.backgroundImage = new StyleBackground(RootControl.RootView.GetSelectorSprite(  RootControl.RootMap.GetElementForPin(pin).table.ToString()).texture);
         }
        else if (pin.category == Pin.Category.Descriptive)
            _iconCategory.style.backgroundImage = new StyleBackground(RootControl.RootMap.pinRowIconDescriptive.texture);
        else if (pin.category == Pin.Category.Map)
            _iconCategory.style.backgroundImage = new StyleBackground(RootControl.RootMap.pinRowIconMap.texture);
            
           ResetStyle();
    }
    private void Hover()
    {
        if (highlighted == false)
            this.style.backgroundColor = index % 2 == 0 ? RootControl.ResultsColorListHoverDark : RootControl.ResultsColorListHoverLight;
    }
    private void ExitHover()
    {
            if (highlighted == false)
                ResetStyle(); 
    }
    public void Highlight(int colorNumber)
    {
        highlighted = true;
        if (colorNumber == 1)
               this.style.backgroundColor = RootControl.ResultsColorListHighlightOne;
        if (colorNumber == 2)
            this.style.backgroundColor = RootControl.ResultsColorListHighlightTwo;
    }
    private void ResetStyle()
    {
        highlighted = false;
        this.style.backgroundColor = index % 2 == 0 ? RootControl.ResultsColorListDark : RootControl.ResultsColorListLight;
    }
}




/// <summary>
/// a row with any number of elements cenetered and expanded
/// </summary>
public class SelectorRow : VisualElement
{
    public bool Hidden = false;
    public SelectorRow(float height)
    {
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        style.alignItems = new StyleEnum<Align>(Align.Center);
        style.width = Length.Percent(100);
        style.height = height;
    }


    public void Hide(bool deactivate)
    {
        if (deactivate)
            foreach (var selector in GetSelectors())
                selector.Deactivate();
        Hidden = true;
        style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

    }
    public void Unhide()
    {
        Hidden = false;
        style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
      
    }
    
    public List<Selector> GetSelectors()
    {
        return this.Children().OfType<Selector>().ToList();
    }
    public List<TopSelector> GetTopSelectors()
    {
        return this.Children().OfType<TopSelector>().ToList();
    }
    public List<CategorySelector> GetCategorySelectors()
    {
        return this.Children().OfType<CategorySelector>().ToList();
    }
    public List<ElementSelector> GetElementSelectors()
    {
        return this.Children().OfType<ElementSelector>().ToList();
    }
    
}



/// <summary>
/// base item for selector type
/// </summary>
public class Selector : VisualElement
{
    public bool Enabled = true;
    public bool Activated = false;
    public bool Hovered = false; 
    public Sprite iconSprite;
    public Label label;
    public VisualElement iconElement;
    public int colorScheme = 1; // color category
     

    public Selector()
    {
        label = new Label();
        Enable(); 
        this.style.width = Length.Percent(100);
        this.style.height = Length.Percent(100); 
    }

    public void Enable()
    {
        // make unclickable, unhoverable, and darkened color
        Enabled = true;
        RefreshColors(); 
    }
    public void Disable()
    {
        // make clickable, hoverable, and standard color
        Enabled = false;
        RefreshColors(); 
    }
    public void Activate()
    { 
        Activated = true; 
        RefreshColors();
    }
    public void Deactivate()
    {
        Activated = false; 
        RefreshColors();
    }

    public void Hover()
    {
        Hovered = true; 
        RefreshColors(); 
    }

    public void ExitHover()
    {
        Hovered = false;
        RefreshColors();
    }

    void RefreshColors()
    {
        style.backgroundColor = RootView.GetSelectorColor(colorScheme, Enabled, Activated, Hovered); 
        if (Hovered)
          label.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else if (RootView.ShowSelectorLabels == false)
                label.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        

    }
    public SelectorRow ParentRow 
    { 
        get { return parent as SelectorRow; } 
    }
    #region unfortunate
    
    private MonoLoader _monoLoader;
    public MonoLoader MonoLoader
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
    public RootControl RootControl
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
    private RootView _rootView;
    public RootView RootView
    {
        get
        {
            if (_rootView == null)
                _rootView = LoadRootView();
            return _rootView;
        }
    }
    private RootView LoadRootView()
    {
        RootView rootView =  AssetDatabase.LoadAssetAtPath<RootView>(MonoLoader.projectPath + MonoLoader.rootPath + "RootView.asset");
        if (rootView == null)
            Debug.LogWarning("! No RootView found. Please re-load the tool from Launcher.");
        return rootView;
    }
    #endregion 
    
} 

public class TopSelector : Selector
{ 
    public TopSelector(string givenName, Sprite givenSprite, Action onLeftClick, Action onRightClick, Action onHoverEnter, Action OnHoverExit, Action OnMiddleClick)
    {
        colorScheme = 1;
        name = givenName;
        iconSprite = givenSprite;

         
        
        string tooltipString = "";
        if (givenName == "Browse")
            tooltipString = "- Left Click to browse World data" +
                            "\n- Right Click to de/select all Element Types" +
                            "\n- Middle Click to show/hide all Element Types";
        if (givenName == "Examine")
            tooltipString = "- Left Click to examine selected Element";
      
        if (RootControl.infoMode)
            this.tooltip = tooltipString;
        MakeStylish();
        CreateIconAndLabel(givenName, givenSprite); 
        
        RegisterCallback<ClickEvent>(evt =>
        {
            if (Enabled == false)
                return;
            if (evt.button == 0) onLeftClick.Invoke(); 
        });
        RegisterCallback<MouseDownEvent>(evt =>
        {
            if (Enabled == false)
                return;
            if (evt.button == 2) OnMiddleClick.Invoke( );
        });
        RegisterCallback<ContextClickEvent>(evt =>
        {
            if (Enabled == false)
                return;
            onRightClick.Invoke();
        });

        RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (Enabled == false)
                return;
            onHoverEnter.Invoke();
        });
        RegisterCallback<MouseLeaveEvent>(evt =>
        {
            if (Enabled == false)
                return;
            OnHoverExit.Invoke();
        });
    }

    private void MakeStylish()
    {
        StyleColor bgColor = RootView.GetSelectorColor(colorScheme, true, false, Hovered);
        style.backgroundColor = bgColor;
        AddToClassList("topSelector");

        var stylesheet =   AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.projectPath +"UI/USS/SelectorStyleSheet.uss"); 
        if (stylesheet == null)
            Debug.Log("Nul");
        styleSheets.Add(stylesheet); 

        style.opacity = 0.9f;
 
    }

    private void CreateIconAndLabel(string givenName, Sprite givenSprite)
    {
 
        label.text= givenName;
        label.style.fontSize = 14;
        label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        label.style.color = Color.white;
        label.style.position = Position.Absolute;
        label.style.left = Length.Percent(40);
        label.style.top = Length.Percent(16);  
        style.justifyContent = Justify.Center;
        style.alignItems = Align.Center;  
        Add(iconElement); 
            Add(label); 
            if (RootView.ShowSelectorLabels == false)
                label.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        this.schedule.Execute(() => {
 
        
             
        }).StartingIn(1); // Execute after a short delay to give the UI time to layout  // todo-investigate does this work?
    }
    
}





/// <summary>
/// A square highlightable transparent button
///  click to activate/deactivate
///   hosts a set of toggleable CategoryElements
///   activate all of them on click 
///   deactivate when all of them are disabled
/// right click to select and deactivate all other categorysleectors in the row
/// </summary>
public class CategorySelector : Selector
{
    public int selectorID = 1;  

      public CategorySelector(int selectorID, string givenName, Sprite givenSprite, Action<CategorySelector>  onLeftClick, Action<CategorySelector> onRightClick, Action<CategorySelector> onHoverEnter, Action<CategorySelector> OnHoverExit,  Action<CategorySelector> OnMiddleClick,  Action OnRightMouseUp)
    {
        colorScheme = 2;
        this.selectorID = selectorID; 
        name = givenName;
        iconSprite = givenSprite;

        Element.Table table = (Element.Table)Enum.Parse(typeof(Element.Table), givenName);
        
        string    tooltipString = "";
        if (RootControl.infoMode)
            tooltipString+=  "- Left Click to de/activate" +
                             "\n- Right Click to solo activate / activate column" +
                             "\n- Middle Click to hide row\n";
        tooltipString += RootControl.DBReader.GetElementMetaDescription(table);
        this.tooltip = tooltipString;
        
        MakeStylish();
        CreateIconAndLabel(givenName, givenSprite); 
        

        RegisterCallback<MouseDownEvent>(evt =>
        {
            if (Enabled == false)
                return; 
            if (evt.button == 0) 
                onLeftClick.Invoke(this); 
            if (evt.button ==2) 
                OnMiddleClick.Invoke(this); 
        }); 
        RegisterCallback<MouseUpEvent>(evt =>
        {
            if (Enabled == false)
                return; 
            if (evt.button == 0) 
                OnRightMouseUp.Invoke(); 
        });
        RegisterCallback<ContextClickEvent>(evt =>
        {
            if (Enabled == false)
                return;
            onRightClick.Invoke(this);
        });

        RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (Enabled == false)
                return; 

            onHoverEnter.Invoke(this);
        });
        RegisterCallback<MouseLeaveEvent>(evt =>
        {
            if (Enabled == false)
                return;
            OnHoverExit.Invoke(this);
        });
    }

    private void MakeStylish()
    {
        StyleColor bgColor = RootView.GetSelectorColor(colorScheme, true, false, Hovered);
        style.backgroundColor = bgColor;
        AddToClassList("categorySelector");

        var stylesheet =   AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.projectPath +"UI/USS/SelectorStyleSheet.uss"); 
        if (stylesheet == null)
            Debug.Log("Nul");
        styleSheets.Add(stylesheet); 

        style.opacity = 0.85f; 
    }

    private void CreateIconAndLabel(string givenName, Sprite givenSprite)
    {
        float iconScale = 1f;
        float iconWidth = 1f;
        float iconHeight = 1f; 
        iconElement = new VisualElement();

        
        if (givenSprite != null)
        {
            iconScale = RootView.iconSize / Mathf.Max(givenSprite.texture.width, givenSprite.texture.height);
            iconWidth = givenSprite.texture.width * iconScale;
            iconHeight = givenSprite.texture.height * iconScale; 
            iconElement.style.backgroundImage = new StyleBackground(givenSprite.texture);
        }
        iconElement.style.width = iconWidth;
        iconElement.style.height = iconHeight;
        iconElement.style.position = Position.Absolute;

        if (givenName == "Matter")
          label.text= "Object";
        else      
           label.text=givenName;

        label.style.fontSize = RootView.fontSizeCategory;
        label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        label.style.color = Color.white;
        label.style.position = Position.Absolute;
        label.style.left = Length.Percent(RootView.labelWidthPercent);
        label.style.top = Length.Percent(RootView.labelHeightPercent);  
        style.justifyContent = Justify.Center;
        style.alignItems = Align.Center;  
        Add(iconElement);
 
            Add(label); 
            if (RootView.ShowSelectorLabels == false)
                label.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        this.schedule.Execute(() => {
            float parentWidth = resolvedStyle.width;
            float parentHeight = resolvedStyle.height;
            
            iconElement.style.left = (parentWidth - iconWidth) / 2;
            iconElement.style.top = (parentHeight - iconHeight) / 2;
             
        }).StartingIn(1); // Execute after a short delay to give the UI time to layout 
    }
    

    
}

/// <summary>
/// A square highlightable transparent button
///  click to activate/deactivate
///  right click to deactivate ALL other categoryelements everywhere
/// </summary>
public class ElementSelector : Selector
{
    public ElementSelector(string givenName, Sprite givenSprite, Action<ElementSelector>  onLeftClick, Action<ElementSelector> onRightClick, Action<ElementSelector> onHoverEnter, Action<ElementSelector> OnHoverExit,  Action<ElementSelector> OnMiddleClick,  Action OnRightMouseUp)
    {
        colorScheme = 3;
        name = givenName;
        iconSprite = givenSprite;
        
        Element.Table table = (Element.Table)Enum.Parse(typeof(Element.Table), givenName);
        string tooltipString = "";

     
        tooltipString = "";
        if (RootControl.infoMode)
            tooltipString+=   "- Left Click to de/activate" +
                              "\n- Right Click to solo activate" +
                              "\n- Middle Click to hide row\n";
        tooltipString += RootControl.DBReader.GetElementMetaDescription(table);
        this.tooltip = tooltipString;

        MakeStylish();
        CreateIconAndLabel(givenName, givenSprite);

        if (givenName == "Concept")
        {
            style.borderBottomLeftRadius = 6f;
            style.borderBottomRightRadius = 6f;
        }
        
        
        RegisterCallback<MouseDownEvent>(evt =>
        {
            if (Enabled == false)
                return; 
            if (evt.button == 0) onLeftClick.Invoke(this); 
            if (evt.button ==2) 
                OnMiddleClick.Invoke(this); 
        }); 
        RegisterCallback<MouseUpEvent>(evt =>
        {
            if (Enabled == false)
                return; 
            if (evt.button == 0) 
                OnRightMouseUp.Invoke(); 
        });
        RegisterCallback<ContextClickEvent>(evt =>
        {
            if (Enabled == false)
                return;
            onRightClick.Invoke(this); 
        });

        RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (Enabled == false)
                return; 

            onHoverEnter.Invoke(this);
        });
        RegisterCallback<MouseLeaveEvent>(evt =>
        {
            if (Enabled == false)
                return;
            OnHoverExit.Invoke(this);
        });
    }

    private void MakeStylish()
    {
        StyleColor bgColor = RootView.GetSelectorColor(colorScheme, true, false, Hovered);
        style.backgroundColor = bgColor;
        AddToClassList("elementSelector");

        var stylesheet =   AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.projectPath +"UI/USS/SelectorStyleSheet.uss"); 
        if (stylesheet == null)
            Debug.Log("Nul");
        styleSheets.Add(stylesheet); 

        style.opacity = 0.8f; 
    }

    private void CreateIconAndLabel(string givenName, Sprite givenSprite)
    {
        float iconScale = 1f;
        float iconWidth = 1f;
        float iconHeight = 1f; 
        iconElement = new VisualElement();

        
        if (givenSprite != null)
        {
            iconScale = RootView.iconSize / Mathf.Max(givenSprite.texture.width, givenSprite.texture.height);
            iconWidth = givenSprite.texture.width * iconScale;
            iconHeight = givenSprite.texture.height * iconScale; 
            iconElement.style.backgroundImage = new StyleBackground(givenSprite.texture); 
        }
        iconElement.style.width = iconWidth;
        iconElement.style.height = iconHeight;
        iconElement.style.position = Position.Absolute; 

        if (givenName == "Concept")
            label.text= "Concepts";
        else      
            label.text=givenName;

        label.style.fontSize = RootView.fontSizeElement;
        label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        label.style.color = Color.white;
        label.style.position = Position.Absolute;
        label.style.left = Length.Percent(RootView.labelWidthPercent);
        label.style.top = Length.Percent(RootView.labelHeightPercent);  
        style.justifyContent = Justify.Center;
        style.alignItems = Align.Center;  
        Add(iconElement);
        Add(label);
        if (RootView.ShowSelectorLabels == false)
            label.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
        this.schedule.Execute(() => {
            float parentWidth = resolvedStyle.width;
            float parentHeight = resolvedStyle.height;
            
            iconElement.style.left = (parentWidth - iconWidth) / 2;
            iconElement.style.top = (parentHeight - iconHeight) / 2;
             
        }).StartingIn(1); // Execute after a short delay to give the UI time to layout 
    }
    


}
 

/// <summary>
/// push for effect 
/// </summary>
public class StandardButton : VisualElement
{
    
}


/// <summary>
/// better list element
/// </summary>
public class ListElement : VisualElement
{
    
}

/// <summary>
/// optimised text field
/// </summary>
public class ProperTextField : VisualElement
{
    
}




/// <summary>
///  textfield for a specific topic eg for Character: hobbies, strengths, personality, nicknames
/// </summary>
public class CategoryField : VisualElement
{
    
}


public class ImageChooser : VisualElement
{
    public Label titleLabel;
    private VisualElement imagePreviewElement;
    private VisualElement imageSelectionElement;
    private VisualElement imageLargerElement;
    private string imagesFolderPath;
    private string selectedImagePath;
    private RootControl RootControl;
    public Sprite chosenSprite;
    
    
  //  iconChooser = new ImageChooser(RootControl, "Base Icon",3, MonoLoader.projectPath + "Resources/Images/ImageChoosers/PinIcons");

    public ImageChooser(RootControl rootControl, string labelText, int imagesPerRow, string folderPath)
    {
        RootControl = rootControl;
        imagesFolderPath = folderPath;
        
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        style.alignItems = new StyleEnum<Align>(Align.Center); 

        titleLabel = new Label();
        titleLabel.text = labelText;
        titleLabel.style.marginRight = 8f;
        this.Add(titleLabel);

        /*var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MonoLoader.staticProjectPath + "UI/USS/SelectorStyleSheet.uss");
        this.styleSheets.Add(stylesheet);
        this.AddToClassList("imageChooser");
        iconPanel.AddToClassList("iconPanel");*/
 
        VisualElement iconPanel = new VisualElement();

        Sprite defaultSprite = LoadSprites()[0];
        if (defaultSprite == null)
            Debug.LogWarning("Found 0 sprites in folder: " + imagesFolderPath);
      
        // todo check for world config or such table for already-chosen default sprite
        // todo create separate build function called from here, and updated on new image select
        // todo make image selector like a bigger extended image preview element
        
        
        imagePreviewElement = new VisualElement();
        imagePreviewElement.style.backgroundColor = new Color(RootControl.imageChooserBackgroundColor.r, RootControl.imageChooserBackgroundColor.b, 
            RootControl.imageChooserBackgroundColor.g, RootControl.imageChooserBackgroundColor.a * 0.6f);
        imagePreviewElement.style.height = RootControl.imageChooserSize;
        imagePreviewElement.style.width = RootControl.imageChooserSize;
        imagePreviewElement.style.flexDirection = FlexDirection.Row;
        imagePreviewElement.style.justifyContent = Justify.Center;
        imagePreviewElement.style.alignItems = Align.Center;
        
        var imageElementInner = new VisualElement();
        imageElementInner.style.backgroundImage = new StyleBackground(defaultSprite.texture);  
        imageElementInner.style.backgroundColor = RootControl.imageChooserBackgroundColor;
        imageElementInner.style.height = RootControl.imageChooserSize * 0.8f;
        imageElementInner.style.width = RootControl.imageChooserSize* 0.8f;
        
        this.AddManipulator(ClickHandler.CreateClickableWithButtonCheck(OnLeftClick, OnRightClick));

        imagePreviewElement.Add(imageElementInner);
        iconPanel.Add(imagePreviewElement);
        this.Add(iconPanel);
        CreateImageSelectionElement(imagesPerRow);
    }
    
    private void CreateImageSelectionElement(int imagesPerRow)
    {
        
    }

    private void OnLeftClick()
    {
        Debug.Log("Left click");
    }
    private void OnRightClick()
    {
        Debug.Log("OnRightClick");

    }
    
    private List<Sprite> LoadSprites()
    {
        List<Sprite> sprites = new List<Sprite>();

        // Get all files in the directory (assuming they're all image files for simplicity)
        string[] fileEntries = Directory.GetFiles(imagesFolderPath);

        foreach(string fileName in fileEntries)
        {
            // Assuming the images are stored as Textures, we'll convert them to Sprites after loading
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fileName);

            if(tex != null)
            {
                // Create a sprite from the texture and add it to the list
                sprites.Add(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
            }
        }

        return sprites;
    }

  

    private void LoadImagesIntoSubWindow(int imagesPerRow, string folderPath)
    {
        // Here you would have logic to read images from a given folderPath,
        // create individual VisualElements for each image,
        // and add a Clickable manipulator to each so they can be selected.

        // The following is just a placeholder and not actual logic:
        // foreach (var imagePath in Directory.GetFiles(folderPath))
        // {
        //     VisualElement img = new VisualElement();
        //     img.style.backgroundImage = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath));
        //     img.AddManipulator(new Clickable(() => SelectImage(imagePath)));
        //     subWindow.Add(img);
        // }
    }
 
}









