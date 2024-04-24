using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Access point that opens frontend layout
// Targets RootControl
// eventually: login, load worlds, etc.
   


public class Launcher : EditorWindow
{  
    
    private VisualElement _uxmlRoot;

    private DropdownField dropdownFieldWorlds;
    private Label  labelWorldHeader;
    private Label  labelWorldContent;
    private Button buttonLoadLayout;
    private Button buttonLoadWorld;
    private Button buttonSaveWorld;
    private Button buttonFetch;
    private Button buttonSend;
    private TextField worldKeyTextField;
   
    #region Instancing
    private static Launcher _instance;
    public static Launcher Instance
    {
        get
        {
            if (_instance == null)
                _instance = GetWindow<Launcher>("Launcher");
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
    [MenuItem("OnlyWorlds/Launcher", false, 1)]
    public static void ShowWindow()
    {
        Instance.Show();
    }
    #endregion
    #region WindowFunctions
    
 
    private void OnEnable()
    {
        VerifyInstance(); 
        BuildFromUXML();
        RootControl.OnWindowRefresh += BuildFromUXML;

    }

  

    private void OnGUI()
    {
        /*GUILayout.FlexibleSpace(); // This adds a flexible space before your content
        GUILayout.BeginHorizontal(); // This starts a horizontal group
        {
            GUILayout.FlexibleSpace(); // This adds a flexible space before your button
            if (GUILayout.Button("Launch", GUILayout.Width(200), GUILayout.Height(50))) // You can set your own width and height
            {
                RootControl.LoadInterface();
            }
            GUILayout.FlexibleSpace(); // This adds a flexible space after your button
        }
        GUILayout.EndHorizontal(); // This ends the horizontal group
        GUILayout.FlexibleSpace(); // This adds a flexible space after your content*/
    }
  
    private void OnFocus()
    {
        
    } 
    private void OnLostFocus()
    {
        
    }
    private void OnDestroy()
    {
         
    }
    #endregion

    private void BuildFromUXML()
    {
        rootVisualElement.Clear();  
        QueryVisualElements();
        VisualElementsLogic();
        rootVisualElement.Add(_uxmlRoot); 
        Repaint(); 
    }

    private void QueryVisualElements()
    {
        _uxmlRoot = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MonoLoader.projectPath +"UI/UXML/Launcher.uxml").Instantiate();
        dropdownFieldWorlds =   _uxmlRoot.Q<DropdownField>("dropdownFieldWorlds");
        labelWorldHeader =   _uxmlRoot.Q<Label>("labelWorldHeader");
        labelWorldContent =   _uxmlRoot.Q<Label>("labelWorldContent");
        buttonLoadLayout =   _uxmlRoot.Q<Button>("buttonLoadLayout"); 
        buttonLoadWorld =   _uxmlRoot.Q<Button>("buttonLoadWorld"); 
        buttonSaveWorld =   _uxmlRoot.Q<Button>("buttonSaveWorld"); 
        buttonFetch =   _uxmlRoot.Q<Button>("buttonFetch"); 
        buttonSend =   _uxmlRoot.Q<Button>("buttonSend"); 
        worldKeyTextField =   _uxmlRoot.Q<TextField>("worldKeyTextField"); 
    }

    private void VisualElementsLogic()
    { 
        DropdownLogic(); 
        ButtonLogic(); 
    }

    private void DropdownLogic()
    {
        var worldNames = RootControl.WorldParser.GetWorldsFileNames();

        if (worldNames.Contains("Default"))
            worldNames.Remove("Default");
        dropdownFieldWorlds.choices = worldNames;
        if (worldNames.Count > 0)
            dropdownFieldWorlds.value = worldNames[0];
        dropdownFieldWorlds.RegisterValueChangedCallback(OnWorldDropdownChanged);
    }

    private void ButtonLogic()
    {
        buttonLoadWorld.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        if (AreRootWindowsOpen())
            buttonLoadWorld.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
        
        buttonLoadWorld.clicked -= ClickButtonLoadWorld;
        buttonLoadWorld.clicked += ClickButtonLoadWorld;     
        buttonSaveWorld.clicked -= ClickButtonSaveWorld;
        buttonSaveWorld.clicked += ClickButtonSaveWorld;      
        buttonLoadLayout.clicked -= ClickButtonLoadLayout;
        buttonLoadLayout.clicked += ClickButtonLoadLayout;  
         
        buttonFetch.clicked -= ClickButtonFetch;
        buttonFetch.clicked += ClickButtonFetch;     
        buttonSend.clicked -= ClickButtonSend;
        buttonSend.clicked += ClickButtonSend;

    }
    private void ClickButtonFetch()
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        RootControl.APIHandler.FetchWorldWithKey(worldKeyTextField.value, dropdownFieldWorlds.value);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }
    private void ClickButtonSend ()
    { 
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        RootControl.APIHandler.SendWorldWithKey(worldKeyTextField.value); 
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
     
    }
    private bool AreRootWindowsOpen()
    {
        return (IsWindowOpen<ViewWindow>() && IsWindowOpen<EditWindow>() && IsWindowOpen<MapWindow>() && IsWindowOpen<TimelineWindow>());
    }

    private static bool IsWindowOpen<T>() where T : EditorWindow
    {
        return Resources.FindObjectsOfTypeAll<T>().Length > 0;
    }
    private void ClickButtonLoadWorld()
    {
       RootControl.LoadWorld( dropdownFieldWorlds.value);
    } 
    private void ClickButtonLoadLayout()
    {
    
        RootControl.LoadInterfaceAndWorld(dropdownFieldWorlds.value);
    }

    private void ClickButtonSaveWorld()
    {
         RootControl.WorldParser.StoreWorld(RootControl.World);
    }

    private void OnWorldDropdownChanged(ChangeEvent<string> evt)
    {
        World world = RootControl.WorldParser.GetWorldByName(evt.newValue); 
        Repaint();
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
        RootControl rootControl =  AssetDatabase.LoadAssetAtPath<RootControl>("Assets/Resources/Root Files/RootControl.asset");
        if (rootControl == null)
            Debug.LogWarning("! No RootControl found. Please re-load the tool from Launcher.");
        return rootControl;
    }


   }
