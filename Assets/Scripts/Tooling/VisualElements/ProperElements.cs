using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UIElements.Slider;
using Toggle = UnityEngine.UIElements.Toggle;

// non-project specific resuable custom elements
// Questions:
//    Most get passed a RootControl now - is there a cleaner way to access a UI config object? something that is not necessarily static? 

public class ProperElements  
{
 
}


#region Combo Elements


/// <summary>
///  Takes a visualelement, eg dropdown or clicklabel, and places a label in front of it 
/// </summary>
public class FrontLabel : VisualElement
{
    public Label label;
    private RootControl RootControl;
    public FrontLabel(RootControl rootControl, bool anchorRight ,string textValue, VisualElement secondElement)
    {
        RootControl = rootControl;
        secondElement.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        if (anchorRight)
         style.justifyContent = Justify.SpaceBetween;
        else
            style.justifyContent = Justify.FlexStart;

        style.alignItems = Align.Center;
    
        label = new Label(textValue);
        label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        this.Add(label); 
        EditorCoroutineUtility.StartCoroutineOwnerless(SetMarginAfterDelay(secondElement));
        
        this.Add(secondElement);
    }
    private IEnumerator SetMarginAfterDelay(VisualElement secondElement)
    {
        yield return new EditorWaitForSeconds(0.01f); 
        float margin = RootControl.frontLabelLeftMargin - label.resolvedStyle.width;
        secondElement.style.marginLeft =  margin;
        secondElement.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
    }
    
}

#endregion

#region Styling Elements


/// <summary>
///  Subcontainer with a border that has border, lighter color, settable height, and some margin from sides
/// </summary>
public class InlayBox : VisualElement
{

    public InlayBox( float height, Color color)
    {
        this.style.backgroundColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, color.a ); 
        this.style.width = Length.Percent(98);
        this.style.height = height;
        this.style.marginLeft = 2f;
        this.style.marginRight = 2f;
        this.style.borderTopWidth = 1f;
        this.style.borderRightWidth = 1f;
        this.style.borderBottomWidth = 1f;
        this.style.borderLeftWidth = 1f;
        this.style.borderTopColor = Color.black;
        this.style.borderBottomColor = Color.black;
        this.style.borderRightColor = Color.black;
        this.style.borderLeftColor = Color.black; 

    }
    
}



#endregion

#region Core Elements


public class BlueDropdownField : VisualElement
{ 
    private RootControl RootControl;
    private Action<string> onValueChanged;
    private VisualElement arrowElement;
    private float minWidth;
    public BlueDropdown Dropdown;
    public string baseString;
    private Label textLabel;
    
    public BlueDropdownField(RootControl rootControl, string labelText,  float parentWidth, Action<string> OnValueChange)
    {
        RootControl = rootControl;
        baseString = labelText;
        
        style.flexDirection = FlexDirection.Row;
        style.justifyContent = Justify.FlexStart;
        style.alignItems = Align.Center;

        textLabel = new Label();
        textLabel.text = labelText;
        textLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);

        Dropdown = new BlueDropdown(RootControl, parentWidth, OnValueChange);
        Dropdown.style.position = new StyleEnum<Position>(Position.Absolute);  

        this.Add(textLabel);
        Dropdown.style.left = RootControl.textfieldLeftMargin; 
        float calculatedWidth = parentWidth   - RootControl.textfieldLeftMargin - 10f; 
        Dropdown.style.width = calculatedWidth ;

        this.Add(Dropdown);
    }

}


public class BlueDropdown : DropdownField
{ 
    private RootControl RootControl;
    private Action<string> onValueChanged;
    private VisualElement arrowElement;
    private float minWidth;
    
    public BlueDropdown( RootControl rootControl, float width, Action<string> onValueChanged) : base()
    { 
        RootControl = rootControl;
        this.onValueChanged = onValueChanged;
        minWidth = width;
        SetupStyles();
    }
    private void SetupStyles()
    {
        this.style.backgroundImage = null; 
        
        this.style.width = minWidth;
        this.style.height = RootControl.EditFieldHeight;

        this.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold); 
      
        float borderSize =0f;
        Color borderColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
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
        {       
            body.style.backgroundColor = RootControl.BlueElementColor; 
            body.style.borderTopWidth = borderSize;
            body.style.borderBottomWidth = borderSize;
            body.style.borderLeftWidth = borderSize;
            body.style.borderRightWidth = borderSize;
        }

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
         });
        
    }
    public void SetChoices(List<string> choices)
    {
        this.choices = choices;
        // todo adjust width to max string size if > minWidth
    }
}
 


/// <summary>
/// TextField with no border and color background
/// </summary>
public class BlueTextField : TextField
{
    public BlueTextField(RootControl RootControl )
    {
        VisualElement textBox = this.Q("unity-text-input");
        textBox.style.backgroundColor = RootControl.BlueElementColor; 
        textBox.style.borderBottomWidth = 0f;
        textBox.style.borderTopWidth = 0f;
        textBox.style.borderRightWidth = 0f;
        textBox.style.borderLeftWidth = 0f;
    } 
}
/// <summary>
/// Multiline TextField with no border and color background
/// </summary>
public class DescriptionField : TextField
{
    public DescriptionField(RootControl RootControl )
    {
        this.multiline = true;
        VisualElement textBox = this.Q("unity-text-input");
        textBox.style.backgroundColor = RootControl.DescriptionFieldColor; 
        textBox.style.borderBottomWidth = 0f;
        textBox.style.borderTopWidth = 0f;
        textBox.style.borderRightWidth = 0f;
        textBox.style.borderLeftWidth = 0f;
    } 
}




/// <summary>
/// Label and inputfield together on one line
/// </summary>
public class EditField : VisualElement
{
    public string baseString;
    private ReferenceText _referenceText;
    private RootControl RootControl;
    private Label textLabel;
    public PropertyInfo property;
    public string fieldValue;  
    
    public EditField(RootControl rootControl, bool showLabel, string labelText, string valueText, float parentWidth, Action<EditField, string> OnValueChange, Action<EditField, string> OnPressEnter, Action<EditField> OnClickField)
    {
        
        RootControl = rootControl;
        baseString = labelText;
        
        this.fieldValue = valueText;
        style.flexDirection = FlexDirection.Row;
        style.justifyContent = Justify.FlexStart;
        style.alignItems = Align.Center;

        textLabel = new Label();
        textLabel.text = labelText;
        textLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        
        _referenceText = new ReferenceText(RootControl, showLabel, this, OnValueChange, OnPressEnter, OnClickField);
        _referenceText.value = valueText;
        _referenceText.style.position = new StyleEnum<Position>(Position.Absolute);  

        
        if (showLabel)
        {
            this.Add(textLabel);
            _referenceText.style.left = RootControl.textfieldLeftMargin; 
            float calculatedWidth = parentWidth   - RootControl.textfieldLeftMargin - 10f; 
            _referenceText.style.width = calculatedWidth ; 
        }
        else
        {
            _referenceText.style.width = Length.Percent(100);
        }

        this.Add(_referenceText);
    }

    public void SetColor(Color color)
    {
        VisualElement textBox =     _referenceText.Q("unity-text-input");
        textBox.style.backgroundColor = color; 
    }

    public void SetInteractable(bool b)
    {
        _referenceText.SetInteractable(b);
    }
}

public class ReferenceText : TextField
{
    private EditField parentEditField;
    private RootControl RootControl;
    public ReferenceText(RootControl RootControl,bool showLabel, EditField parentEditField, Action<EditField, string> OnValueChange, Action<EditField, string> OnPressEnter, Action<EditField> OnClickField)
    {
        this.RootControl = RootControl;
        this.style.width = Length.Percent(96);
        this.parentEditField = parentEditField;
        VisualElement textBox = this.Q("unity-text-input"); 
            textBox.style.height = RootControl.EditFieldHeight;
            textBox.style.backgroundColor = RootControl.BlueElementColor; 
            textBox.style.borderBottomWidth = 0f;
            textBox.style.borderTopWidth = 0f;
            textBox.style.borderRightWidth = 0f;
            textBox.style.borderLeftWidth = 0f;
            textBox.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        
           
        this.RegisterValueChangedCallback(evt =>
        {
            if (OnValueChange != null)
            {
                OnValueChange.Invoke(this.parentEditField, evt.newValue);
                parentEditField.fieldValue = evt.newValue;
            } 
        });
        this.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                if (OnPressEnter != null)
                    OnPressEnter(this.parentEditField, this.value);
        });
        this.RegisterCallback<MouseUpEvent>(evt =>
        { 
            if (OnClickField != null)
                OnClickField.Invoke(parentEditField);
        });
    }

    public void SetInteractable(bool b)
    {
        this.focusable = b;
        VisualElement textBox = this.Q("unity-text-input");  
        textBox.style.backgroundColor = new Color(RootControl.BlueElementColor.r,RootControl.BlueElementColor.g,RootControl.BlueElementColor.b,
            RootControl.BlueElementColor.a * 0.3f); 
    }
}



/// <summary>
/// Slider with a label in front of it
/// </summary>
public class SimpleSlider : VisualElement
{
    private string baseString;
    private ReferenceSlider _referenceSlider;
    private RootControl RootControl;
    private Label textLabel; 
    
// right click reset to base value
// set to 1 on windowbnuild or smth
    public SimpleSlider(RootControl rootControl,float parentWidth, string labelText, Func<float> getValue, Action<float> setValue, Action<float> onValueChange, float minValue, float maxValue)
    {
        RootControl = rootControl;
        baseString = labelText; 
        style.flexDirection = FlexDirection.Row;
        style.justifyContent = Justify.FlexStart;
        style.alignItems = Align.Center;

        textLabel = new BoldLabel(); 
        textLabel.text = baseString + ": " + RootControl.RootMap.pinScaling;//.ToString("1F");
        this.Add(textLabel);

        _referenceSlider = new ReferenceSlider(getValue, setValue, onValueChange, textLabel, baseString);
        _referenceSlider.value = getValue();
        _referenceSlider.lowValue = minValue;
        _referenceSlider.highValue = maxValue;
        _referenceSlider.style.position = new StyleEnum<Position>(Position.Absolute);
        float calculatedWidth = parentWidth   - RootControl.sliderLeftMargin - 10f; 
        _referenceSlider.style.width = calculatedWidth ; 
        _referenceSlider.style.left = RootControl.sliderLeftMargin;
        this.Add(_referenceSlider);

        textLabel.RegisterCallback<MouseUpEvent>(evt =>
        {
            if (evt.button == 1)
                _referenceSlider.value = 1f; 
        });
    }

    public void SetValueWithoutNotify(float value)
    {
        _referenceSlider.SetValueWithoutNotify(value);
        textLabel.text = baseString + ": " + RootControl.RootMap.pinScaling;
    }
}


public class ReferenceSlider : Slider
{
    private EditorCoroutine sliderDelayCoroutine;
    private bool signalAllowed = true;
    
    public ReferenceSlider(Func<float> getValue, Action<float> setValue, Action<float> onValueChange, Label label, string baseText)
    { 
        this.value = getValue();

        this.RegisterValueChangedCallback(evt =>
        {
            label.text = baseText + ": " + this.value;//.ToString("1F");
            setValue(this.value);
            if (signalAllowed)
               EditorCoroutineUtility.StartCoroutineOwnerless(AttemptValueChange(onValueChange));   

        });
    }
    private IEnumerator AttemptValueChange(Action<float> onValueChange)
    {
        signalAllowed = false;
        yield return new EditorWaitForSeconds(0.02f);
        signalAllowed = true;
        onValueChange(this.value);
    }
}





/// <summary>
/// Toggle parent that can take a bool variable reference, and places the tickbox at a consistent left margin instead of based on label text
/// </summary>
public class SimpleToggle : VisualElement
{
    private ReferenceToggle _referenceToggle;
    private RootControl RootControl;
    
    public SimpleToggle(RootControl rootControl, string labelText, Func<bool> getValue, Action<bool> setValue, Action<bool> onValueChange)
    {
        RootControl = rootControl;
        style.flexDirection = FlexDirection.Row;
        style.justifyContent = Justify.FlexStart;
        style.alignItems = Align.Center;

        var textLabel = new BoldLabel();
        textLabel.text = labelText;
        this.Add(textLabel);

        _referenceToggle = new ReferenceToggle(RootControl, getValue, setValue, onValueChange);
        _referenceToggle.style.position = new StyleEnum<Position>(Position.Absolute);
        _referenceToggle.style.left = RootControl.toggleLeftMargin;
        this.Add(_referenceToggle);
    }
}


/// <summary>
/// Custom Toggle that can take a bool variable reference and update it directly
/// </summary>
public class ReferenceToggle : Toggle
{
    public ReferenceToggle(RootControl RootControl, Func<bool> getValue, Action<bool> setValue, Action<bool> onValueChange)
    { 
        this.value = getValue();
 //       VisualElement toggleBox = this.Q("unity-checkmark");
 //       toggleBox.style.backgroundColor = RootControl.ToggleColor;
        
        this.RegisterValueChangedCallback(evt =>
        {
            setValue(evt.newValue);   
            onValueChange(evt.newValue);
        });
    }
}


#endregion


//#region Funky Elements

