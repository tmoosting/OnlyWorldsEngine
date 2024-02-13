using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using World_Model;
using Debug = UnityEngine.Debug;


public class EditWindow : EditorWindow
{
 
    private VisualElement windowContent;
    private VisualElement fieldsContent;
    private VisualElement fieldsTopContent;
    private VisualElement fieldsMidContent;
    private VisualElement fieldsEditFieldsContent;

    #region Instancing
    private static EditWindow _instance;
    public static EditWindow Instance
    {
        get
        {
            if (_instance == null)
                _instance = GetWindow<EditWindow>("Element");
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
    [MenuItem("OnlyWorlds/Edit Window")]
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
        if (RootControl.World != null) // todo make uniform across windows
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
        //    RootControl.OnFieldChanged -= RefreshField;
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
        RefreshWindow();
    }  
    private void RefreshWorld(World newWorld)
    {
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
        RefreshWindow();
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
        RefreshWindow();
    }
    private void RefreshLossField ()
    { 
        
    }   
    private void RefreshLossMap ()
    { 
        
    }  
    private void RefreshLossTimestate ()
    { 
        
    }
    #endregion
    #region Window Building
    
    private void BuildWindow()
    {
        rootVisualElement.Clear();
        UpdateWindowCallbacks(); 
        BuildFieldsContent(); 
        Repaint();
    } 
    private void RefreshWindow()
    {
        Element element = RootControl.Element;
        if (element == null)
        {
            ClearWindowElementContent();
            return;
        }

        nameLabel.SetValueWithoutNotify(element.Name); 
        deleteButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        fieldsMidContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

        CreateElementEditFields();
    }

    private ScrollView editFieldsScrollView;
    private List<VisualElement> editFields;
    private ClickLabel nameLabel;
    private WhiteButton deleteButton;
    private SquareDropdown sortDropdown;
    private BlueDropdownField typeDropdown;
    private BlueDropdownField subtypeDropdown;
    private string dropdownStartString = "Sort by..";
    private void CreateElementEditFields()
    {
        float bottomSpacing = 5f;
        
        Element element = RootControl.Element;
        
        // Reset edit fields
        if (editFields != null) 
            foreach (var visualElement in editFields)
                visualElement.RemoveFromHierarchy(); 
        editFields = new List<VisualElement>();
        if (fieldsEditFieldsContent != null) 
            fieldsEditFieldsContent.Clear();
        

        Dictionary<string, string> propertiesAndValues = RootEdit.GetPropertiesAndValues(    RootControl.Element);


        typeDropdown = new BlueDropdownField(RootControl, "Supertype", fieldsContent.resolvedStyle.width, OnSupertypeValueChange);
        List<string> tableTypes = new List<string>();
        foreach (var tableTyping in RootControl.WorldParser.GetTypingTablesForElementTable(element.category))
            tableTypes.Add(tableTyping.GetPotentialCustomSupertype());
        typeDropdown.Dropdown.choices = tableTypes;
        typeDropdown.Dropdown.SetValueWithoutNotify( RootControl.WorldParser.GetPotentialCustomSupertypeForElement( element));
        typeDropdown.style.marginBottom = bottomSpacing;
        fieldsEditFieldsContent.Add(typeDropdown);

        subtypeDropdown = new BlueDropdownField(RootControl, "Subtype", fieldsContent.resolvedStyle.width, OnSubtypeValueChange);
        subtypeDropdown.Dropdown.choices = RootControl.WorldParser.GetTypingTableForElement(element).GetPotentiallyCustomSubtypes();
        string subTypeStr = RootControl.WorldParser.GetPotentialCustomSubtypeForElement(element);
        subtypeDropdown.Dropdown.SetValueWithoutNotify(subTypeStr );
        subtypeDropdown.style.marginBottom = bottomSpacing;
        fieldsEditFieldsContent.Add(subtypeDropdown);
        
        foreach (var propertiesAndValue in propertiesAndValues)
        {
            if (propertiesAndValue.Key == "Name" || propertiesAndValue.Key == "Supertype" ||
                propertiesAndValue.Key == "Subtype" || propertiesAndValue.Key == "Description")
                continue;
            EditField editField = new EditField(RootControl, true,  propertiesAndValue.Key, 
                propertiesAndValue.Value, fieldsContent.resolvedStyle.width, OnEditFieldValueChange, OnEditFieldEnter, ClickEditField);
            editField.style.marginBottom = bottomSpacing;
           
            editFields.Add(editField);
        }  
        foreach (var visualElement in editFields)
            fieldsEditFieldsContent.Add(visualElement); 
    }

    private void OnSupertypeValueChange(string value)
    {
        string actualSupertype =
            RootControl.WorldParser.GetOriginalSupertypeForPotentialCustomSupertype(RootControl.Element, value);
        RootEdit.MakeDirectActiveElementChange( "Supertype", actualSupertype);
        UpdateSubtypesAfterSupertypeChange();
    }

    // update both the Element's subtype, now invalid due to supertype change, and the subtypes dropdown
    private void UpdateSubtypesAfterSupertypeChange()
    {
        List<string> originalSubtypes = RootControl.WorldParser.GetTypingTableForElement(RootControl.Element).GetOriginalSubtypes();
        List<string> customSubtypes = RootControl.WorldParser.GetTypingTableForElement(RootControl.Element).GetPotentiallyCustomSubtypes();
        
        subtypeDropdown.Dropdown.choices = customSubtypes.ToList();
        RootEdit.MakeDirectActiveElementChange("Subtype", originalSubtypes[0]); 
        subtypeDropdown.Dropdown.SetValueWithoutNotify(customSubtypes[0]);
    }

    private void OnSubtypeValueChange(string value)
    {
        string actualSubtype  =
            RootControl.WorldParser.GetOriginalSubtypeForPotentialCustomSubtype(RootControl.Element, value);
        RootEdit.MakeDirectActiveElementChange("Subtype", actualSubtype);
    }

   
    private void BuildFieldsContent()
    {
        windowContent = new VisualElement();
        windowContent.style.width = Length.Percent(100);
        windowContent.style.height = Length.Percent(100);
        windowContent.style.backgroundColor = RootControl.generalBackgroundColor;
        fieldsContent = new VisualElement();
        fieldsContent.style.width = Length.Percent(100);
        fieldsContent.style.height = Length.Percent(100);
        fieldsContent.style.marginLeft = 2f;
        fieldsContent.style.marginTop = 2f;
        
        fieldsTopContent = new VisualElement( );
        fieldsTopContent.name = "fieldsTopContent";
        fieldsTopContent.style.flexGrow = 1f;
        fieldsTopContent.style.flexShrink = 1f;
        fieldsTopContent.style.marginBottom = 5f;
        fieldsTopContent.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        fieldsTopContent.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween); 

        nameLabel = new ClickLabel(RootControl, null, PressEnterOnNameLabel, LoseNameLabelFocus); 
        nameLabel.style.fontSize = 15f;
        nameLabel.style.minHeight = 22f;
        nameLabel.style.marginLeft = -1f;
        nameLabel.style.marginTop = -2f;
        nameLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        fieldsTopContent.Add(nameLabel);

    
        sortDropdown = new SquareDropdown(RootControl, "SortDropdown", OnSortDropdownChange);
        List<string> dropdownStrings = new List<string>();
        dropdownStrings.Add("Name");
        dropdownStrings.Add("Type"); 
        sortDropdown.choices = dropdownStrings;
        sortDropdown.value = dropdownStartString;
        sortDropdown.style.marginLeft = 0f;
        //todo-enable 
        sortDropdown.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);      
        
        fieldsTopContent.Add(sortDropdown);
        
        deleteButton = new WhiteButton(RootControl, "Delete", ClickElementDeleteButton);
        deleteButton.style.marginRight = 5f;
        deleteButton.style.marginTop = 2f;
        fieldsTopContent.Add(deleteButton);
        
        
        fieldsMidContent = new VisualElement();
        fieldsMidContent.name = "fieldsMidContent";
        fieldsMidContent.style.width = Length.Percent(100);
        fieldsMidContent.style.height = Length.Percent(100);
        
        fieldsEditFieldsContent = new VisualElement();
        fieldsEditFieldsContent.style.width = Length.Percent(100);
        fieldsEditFieldsContent.style.height = Length.Percent(100);
        fieldsEditFieldsContent.style.marginTop = 5f;
 

        // Initialize the ScrollView and add fieldsEditFieldsContent to it.
     
        if (editFieldsScrollView != null)
            editFieldsScrollView.Clear();
        editFieldsScrollView = new ScrollView();
        editFieldsScrollView.style.flexGrow = 1; // Ensure it grows to fill available space
        editFieldsScrollView.Add(fieldsEditFieldsContent);

        fieldsMidContent.Add(editFieldsScrollView);

        RefreshWindow(); 
        fieldsContent.Add(fieldsTopContent);
        fieldsContent.Add(fieldsMidContent);
        windowContent.Add(fieldsContent);
        rootVisualElement.Add(windowContent); 
    }


    private void PressEnterOnNameLabel(string value)
    {
        if (RootControl.Element == null)
        {
            nameLabel.value = noElementSelectedString;
            return;
        }
        if (nameLabel.value != noElementSelectedString)
            RootEdit.MakeDirectActiveElementChange("Name", value);
        RootControl.RegisterElementValueChange();
    }
    private void LoseNameLabelFocus()
    {
        if (RootControl.Element == null)
        {
            nameLabel.value = noElementSelectedString;
            return;
        } 
        nameLabel.SetValue( RootControl.Element.Name); 
    }
    
    
    private void ClickElementDeleteButton()
    {
        RootEdit.DeleteElement(RootControl.Element);
    }

    private void OnSortDropdownChange(string value)
    {
     // reorder editfields   
    }

  
   

   

    private void ClickEditField(EditField editField)
    { 
        if (RootEdit.IsElementChangeAllowed(editField, RootControl.Element))
            enterAllowed = true;
        else
            enterAllowed = false;
    }


    private EditorCoroutine entryCoroutine = null;
    private void OnEditFieldValueChange(EditField editField, string value)
    {
        if (entryCoroutine != null)
        {
            EditorCoroutineUtility.StopCoroutine(entryCoroutine);
            entryCoroutine = null;
        }
        entryCoroutine =   EditorCoroutineUtility.StartCoroutineOwnerless(AttemptWriteEntryAfterDelay(editField, RootControl.Element )); 
    }


    private bool enterAllowed = true;
    private IEnumerator AttemptWriteEntryAfterDelay(EditField editField,Element element )
    {
        enterAllowed = false;
       yield return new EditorWaitForSeconds(RootEdit.entryDelay);

       if (RootEdit.IsElementChangeAllowed(editField, element))
       {
           RootEdit.MakeElementChangeFromEditField(editField, element);
           enterAllowed = true;
           editField.SetColor(new Color(Color.green.r,Color.green.g,Color.green.b,  0.3f));
       }
       else
       {
           editField.SetColor(new Color(Color.red.r,Color.red.g,Color.red.b,  0.2f)); 
       }
       
     

    }
    
    private void OnEditFieldEnter(EditField editField, string arg2)
    {
        if (enterAllowed == false)
            return;
        RootControl.RegisterElementValueChange(); 
        editField.SetColor(new Color(Color.cyan.r,Color.cyan.g,Color.cyan.b,  0.25f));
    }


    private string noElementSelectedString = "No Element Selected";
    private void ClearWindowElementContent()
    { 
        deleteButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        fieldsMidContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
       nameLabel.SetValueWithoutNotify( noElementSelectedString);
        
        // remove all editfields
    }

    #endregion
    
    
    
    private EventCallback<KeyDownEvent> _keyPressCallback;
    private void UpdateWindowCallbacks()
    {
        if (_keyPressCallback != null)
            rootVisualElement.UnregisterCallback(_keyPressCallback);
        
        rootVisualElement.focusable = true;
        _keyPressCallback = (e) => PressKey(e);
        rootVisualElement.RegisterCallback(_keyPressCallback, TrickleDown.TrickleDown);
    }
    private void PressKey(KeyDownEvent evt)
    {  
        if (evt.keyCode == KeyCode.Escape)
        {
            if (RootControl.Element != null)
                RootControl.SetElement(null);
        }
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
    private RootEdit _rootEdit;
    private RootEdit RootEdit
    {
        get
        {
            if (_rootEdit == null)
                _rootEdit = LoadRootEdit();
            return _rootEdit;
        }
    }
    private RootEdit LoadRootEdit()
    {
        RootEdit rootEdit =  AssetDatabase.LoadAssetAtPath<RootEdit>(MonoLoader.projectPath + MonoLoader.rootPath + "RootEdit.asset");
        if (rootEdit == null)
            Debug.LogWarning("! No RootEdit found. Please re-load the tool from Launcher.");
        return rootEdit;
    }
}
