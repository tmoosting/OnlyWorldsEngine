using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
 


// make column for commaseparated Subtypes, then custom subtypes,split up

// All Typing Tables:
 // Copy location typing to ALL other elements
 // create lists in world
// update worldparser GetTypingTablesForElementTable
 // update default types in RootEdit CreateElements
 // Enter default data values for all elements..

// fix subtypes not correctly  writing



// make name a clicklabel
// separate type and subtype
// larger description field
// new dropdowns in editwindow
// better color and tiling. one tile fir typing ?
// create element through Edit
// add subtypes / delete


// Interfaces through Launcher, loads scene objects and provides getters, tracks selected states

public enum WorldState
{
    Unloaded,
    Loaded,
    Unsaved,
    Saved
}

[CreateAssetMenu(fileName = "RootControl", menuName = "ScriptableObjects/RootControl", order = 0)]
public class RootControl : ScriptableObject
{
    
    [Header("General")]  
    public bool infoMode = true;
  //  public Element.Table startingTable;
     
    
    
    [Header("UI")] 
    public float blueDropdownHeight = 24f;
    public float dropdownHeight = 24f;
    public float dropdownWidth = 80f;
    public Sprite dropdownArrow;
    public Sprite toggleArrow;
    public Sprite maskfieldArrow;
    public float dropdownArrowSize = 10f;
    public float buttonWidth = 50f;
    public float buttonHeight = 24f;
    public float toggleWidth = 50f;
    public float toggleHeight = 24f;
    public float toggleIconSize = 8f;
    public float clickLabelHeight = 14f;
    public float toggleLeftMargin = 100f;
    public float EditFieldHeight= 18f; 
    public float textfieldLeftMargin = 100f;
    public float sliderLeftMargin = 60f;
    public float frontLabelLeftMargin = 100f;
    public float imageChooserSize = 20f;
    public Color imageChooserBackgroundColor;
   // public float clickLabelWidth = 14f;
    
   [Header("Colors")]
   public Color generalBackgroundColor; 
   public  Color BlueElementColor;   
   public  Color SearchColorInside;      
   public  Color ResultsColorContainer;   
   public  Color ResultsColorFilterLeft;   
   public  Color ResultsColorFilterRight;   
   public  Color ResultsColorListLight;   
   public  Color ResultsColorListDark;   
   public  Color ResultsColorListHoverLight;   
   public  Color ResultsColorListHoverDark;   
   public  Color ResultsColorListHighlightOne;   
   public  Color ResultsColorListHighlightTwo;   
   public  Color ResultsColorResultTextOne;   
   public  Color ResultsColorResultTextTwo;   
   public  Color ResultsColorResultTextThree;
   public  Color DescriptionFieldColor;
   public  Color ToggleColor;
   
   
   
    [HideInInspector]
    public WorldState worldState = WorldState.Unloaded;


    //todo public toggles for each of these
    
    public void RegisterViewWindowValueChange(bool storeWorld = true)
    {
        if (World == null)
            return;
        if (storeWorld)
            WorldParser.StoreWorld(World);
    }

    public void RegisterElementValueChange(bool storeWorld = true)
    {
        if (World == null)
            return;
        if (storeWorld)
           WorldParser.StoreWorld(World);
        OnElementEdited?.Invoke(Element);
    } 
    public void RegisterTableTypingValueChange(bool storeWorld = true)
    {
        if (World == null)
            return;
        if (storeWorld)
           WorldParser.StoreWorld(World);
    }
    public void SaveWorldAfterPinChange()
    {
        if (World == null)
            return;
        WorldParser.StoreWorld(World);
    }
    
    
    #region Load and Initialize

    /// <summary>
    /// called from Launcher to load a specific World when the tool frontend is already open
    /// </summary>
    public void LoadWorld(string worldName)
    {
        DBReader.SetDatabasePath(worldName);
        DBWriter.SetDatabasePath(worldName);

        ClearSelectionStates();
        
        World loadedWorld = WorldParser.CreateWorldFromName(worldName);

     //   _table = startingTable;
        SetWorld(loadedWorld, true);
        Map baseMap = RootMap.GetBaseMap();
        if (baseMap != null) 
            Map = baseMap;
        worldState = WorldState.Loaded;
        OnWorldChanged?.Invoke(loadedWorld);
    }
    
   

    /// <summary>
    /// called from Launcher to init tool and load the layout
    /// </summary>
    public void LoadInterfaceAndWorld (string worldName)
    {
        Debug.ClearDeveloperConsole();
        VerifyEnvironment(); 
        LoadLayoutFile();

        LoadWorld(worldName);
        
        string fullReportString = "OnlyWorlds toolset successfully validated and loaded for World: " + World.ID;
        if (missingCount != 0)
            fullReportString  = "OnlyWorlds toolset has missing folders or files! Please re-import the project.\n" + reportString;
        Debug.Log(fullReportString);
    }
   
 
    
    private void LoadLayoutFile()
    {
        string layoutPath = MonoLoader.projectPath + "Resources/OWLayout.wlt";
        EditorUtility.LoadWindowLayout(layoutPath);
    }
   
    #endregion
    
    
    #region Refreshing, Active Object Getters and Delegates

    public delegate void WindowRefreshHandler();
    public delegate void WorldChangedHandler(World newWorld);
    public delegate void TableChangedHandler(Element.Table newTable);
    public delegate void ElementChangedHandler(Element newElement);
    public delegate void ElementEditedHandler(Element newElement);
   // public delegate void FieldChangedHandler(Field newField);
    public delegate void MapChangedHandler(Map newMap);
    public delegate void PinChangedHandler(Pin newPin);
    public delegate void TimestateChangedHandler(Timestate newTimestate);
    
    public event WindowRefreshHandler OnWindowRefresh; // no-selection-refresh only. Rest of windowupdates is handled through delegates below
    public event WorldChangedHandler OnWorldChanged;
    public event TableChangedHandler OnTableChanged;
    public event ElementChangedHandler OnElementChanged;
    public event ElementEditedHandler OnElementEdited;
  //  public event FieldChangedHandler OnFieldChanged;
    public event MapChangedHandler OnMapChanged;
    public event PinChangedHandler OnPinChanged;
    public event TimestateChangedHandler OnTimestateChanged;


    public void GlobalRefresh()
    {
        OnWindowRefresh?.Invoke();
    }
    public void ElementRefresh()
    {
        //todo optimise this ?
        OnElementChanged?.Invoke(Element);
    }
   
    private void ClearSelectionStates()
    {
   //     Table = startingTable;
        _world = null;
        _element = null; 
        _pin = null;
     //   _field = null;
        _map = null;
        _timestate = null;
    }

    public List<Pin> GetPinsForCurrentMap()
    {
        if (Map == null)
        {
            Debug.LogWarning("!No map is active! Can not get pins");
            return null;
        }
        return World.Pins.Where(pin => pin.Map == Map.ID).ToList();
    }
    
    //todo properly set up as types and create access somewhere
    public string Protagonist = "Hero";
    public string Homebase = "Home";
    
    private World _world;
    public World World
    {
        get { return _world; }
        private   set
        {
            _world = value; 
            if (skipNextWorldInvoke)
                skipNextWorldInvoke = false;
            else 
              OnWorldChanged?.Invoke(_world);
        }
    }
    private bool skipNextWorldInvoke = false;
    public void SetWorld(World world, bool skipInvoke = false)
    {
        skipNextWorldInvoke = skipInvoke;
        World = world;
    }
    private Element.Table _table;
    public Element.Table Table
    {
        get { return _table; }
        private  set
        {
            _table = value; 
            if (skipNextTableInvoke)
                skipNextPinInvoke = false;
            else 
                OnTableChanged?.Invoke(_table);
        }
    }
    private bool skipNextTableInvoke = false;
    public void SetTable(Element.Table table, bool skipInvoke = false)
    {
        skipNextTableInvoke = skipInvoke;
        Table =table;
    }
    private Element _element;
    public Element Element
    {
        get { return _element; }
        private  set
        { 
            _element = value; 
            if (skipNextElementInvoke)
                skipNextElementInvoke = false;
            else 
              OnElementChanged?.Invoke(_element);
        }
    }
    private bool skipNextElementInvoke = false;
    public void SetElement(Element element, bool skipInvoke = false)
    {
        skipNextElementInvoke = skipInvoke;
        Element = element;
    }
    /*private Field _field;
    private Field Field // will I even use this? 
    {
        get { return _field; }
        set
        {
            _field = value; 
            OnFieldChanged?.Invoke(_field);
        }
    }*/
    private Map _map;
    public Map Map
    {
        get
        {
            return _map;
        }
        private set
        {
            _map = value; 
            if (skipNextMapInvoke)
                skipNextMapInvoke = false;
            else 
            OnMapChanged?.Invoke(_map);
        }
    }
    private bool skipNextMapInvoke = false;
    public void SetMap(Map map, bool skipInvoke = false)
    {
        skipNextMapInvoke = skipInvoke;
        Map = map;
    }
    private Pin _pin;
    public Pin Pin
    {
        get { return _pin; }
        private set
        {
            _pin = value;
            if (skipNextPinInvoke)
                skipNextPinInvoke = false;
            else
                OnPinChanged?.Invoke(_pin);
        }
    }

    private bool skipNextPinInvoke = false;
    public void SetPin(Pin pin, bool skipInvoke = false)
    {
        skipNextPinInvoke = skipInvoke;
        Pin = pin;
    }
    private Timestate _timestate;
    public Timestate Timestate
    {
        get { return _timestate; }
        private set
        {
            _timestate = value; 
            if (skipNextTimestateInvoke)
                skipNextTimestateInvoke = false;
            else 
             OnTimestateChanged?.Invoke(_timestate);
        }
    }
    private bool skipNextTimestateInvoke = false;
    public void SetTimestate(Timestate timestate, bool skipInvoke = false)
    {
        skipNextTimestateInvoke = skipInvoke;
        Timestate = timestate;
    }
    public void ValidateNullCheck(string type)
    {
        //world, element, map, pin, timestate
        if (type == "World")
            if (World == null)
                Debug.LogWarning("!Validate for World failed - value is null");
        if (type == "Element")
            if (Element == null)
                Debug.LogWarning("!Validate for Element failed - value is null");
        if (type == "Map")
            if (Map == null)
                Debug.LogWarning("!Validate for Map failed - value is null");
        if (type == "Pin")
            if (Pin == null)
                Debug.LogWarning("!Validate for Pin failed - value is null");
        if (type == "Timestate")
            if (Timestate == null)
                Debug.LogWarning("!Validate for Timestate failed - value is null");
    }


    
 

 
  
    public List<Element> GetWorldElements()
    {
        List<Element> returnList = new List<Element>();

        // Loop through all the values in the Table enum
        foreach (Element.Table table in Enum.GetValues(typeof(Element.Table)))
        {
            var elementsFromTable =  GetElementsOfTable(table);
            if (elementsFromTable != null)
                returnList.AddRange(elementsFromTable);
        }
        return returnList;
    }

    public List<Element> GetElementsOfTable(Element.Table table)
    { 
        switch (table)
        {
            case Element.Table.Character:
                return World.Characters.Cast<Element>().ToList();
            case Element.Table.God:
                return World.Gods.Cast<Element>().ToList();
            case Element.Table.Event:
                return World.Events.Cast<Element>().ToList();
            case Element.Table.Relation:
                return World.Relations.Cast<Element>().ToList();
            case Element.Table.Collective:
                return World.Collectives.Cast<Element>().ToList();
            case Element.Table.Concept:
                return World.Concepts.Cast<Element>().ToList();
            case Element.Table.Creature:
                return World.Creatures.Cast<Element>().ToList();
            case Element.Table.Location:
                return World.Locations.Cast<Element>().ToList();
            case Element.Table.Matter:
                return World.Matters.Cast<Element>().ToList();
            case Element.Table.Institution:
                return World.Institutions.Cast<Element>().ToList();
            case Element.Table.Territory:
                return World.Territorys.Cast<Element>().ToList();
            case Element.Table.Title:
                return World.Titles.Cast<Element>().ToList();
            case Element.Table.Race:
                return World.Races.Cast<Element>().ToList();
            case Element.Table.Family:
                return World.Familys.Cast<Element>().ToList();  
            case Element.Table.Trait:
                return World.Traits.Cast<Element>().ToList();
            case Element.Table.Law:
                return World.Laws.Cast<Element>().ToList();  
            case Element.Table.Language:
                return World.Languages.Cast<Element>().ToList();        
            case Element.Table.Ability:
                return World.Abilitys.Cast<Element>().ToList();
            default:
                Debug.LogError($"Unsupported table: {table}");
                return null;
        }
    }

    #endregion
    
    
    #region State and Storage Logic
    public void IntervalBackup()
    {
        if (MonoLoader.intervalSaveEnabled == false)
            return;
        if (worldState is not (WorldState.Loaded or WorldState.Unsaved))
            return;
        if (World != null)
            WorldParser.StoreWorld(World);
    }
    
    
    
    #endregion
    
    
    #region Verifying
    private int missingCount = 0;
    private string reportString = "";
    /// <summary>
    /// check for core files and folders. Use hardcoded asset path twice here because it is currently stored in MonoLoader which is scene-dependent
    /// </summary>
    private void VerifyEnvironment()
    {
        // Check for correct scene existing and loaded first
        var asset = AssetDatabase.LoadAssetAtPath<Object>("Assets/Scenes/OnlyWorlds.unity");

        if (asset == null)
        {
            Debug.LogWarning("OnlyWorlds scene is missing from /Scenes! Please re-import the project");
            return;
        } 
        // Check the currently open scene and load OnlyWorlds scene if it's not that
        string activeSceneName = EditorSceneManager.GetActiveScene().name; 
        if (activeSceneName != "OnlyWorlds")
            EditorSceneManager.OpenScene("Assets/Scenes/OnlyWorlds.unity");
         
        missingCount = 0;
        reportString = "";
        // verify folders and resources and monoloader
        reportString += VerifyFolders();
        reportString += VerifyFiles();  
    }
      private string VerifyFolders()
    {
        string missingFolders = ""; 
        missingFolders +=  VerifyFolder("Scenes");  
        missingFolders +=  VerifyFolder("Resources");  
        missingFolders +=  VerifyFolder("Resources/Root Files");   
        missingFolders +=  VerifyFolder("Resources/Worlds");   
        missingFolders +=  VerifyFolder("Resources/Images");   
        missingFolders +=  VerifyFolder("Resources/Images/Maps");   
        missingFolders +=  VerifyFolder("Resources/Images/Characters");   
        missingFolders +=  VerifyFolder("Resources/Images/Various");    
        return missingFolders;
    }
    private string VerifyFiles()
    {
        string missingFiles = "";
        missingFiles +=  VerifyFile("Resources/OWLayout.wlt");  
        missingFiles +=  VerifyFile("Resources/Root Files/RootControl.asset");  
        missingFiles +=  VerifyFile("Resources/Root Files/RootView.asset");  
        missingFiles +=  VerifyFile("Resources/Root Files/RootEdit.asset");  
        missingFiles +=  VerifyFile("Resources/Root Files/RootMap.asset");  
        missingFiles +=  VerifyFile("Resources/Root Files/RootTimeline.asset");   
 
        return missingFiles; 
    }

  

    /// <summary>
    /// check if a folder exists, create it if not. prePath is everything between projectPath and folder name
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="prePath"></param>
    /// <returns></returns>
    private string VerifyFolder(string folderPath)
    {  
        string str = ""; 
        string actualPath = MonoLoader.projectPath + folderPath; 
        if (AssetDatabase.IsValidFolder(actualPath) == false)
        { 
            missingCount++;
            str +="- "+ actualPath + "\n";
        } 
        return str;
    } 
    private string VerifyFile(string filePath)
    {
        string str = "";  
        string actualPath = MonoLoader.projectPath + filePath; 
        
        var asset = AssetDatabase.LoadAssetAtPath<Object>(actualPath);

        if (asset == null)  
        { 
            missingCount++;
            str +="- "+ actualPath + "\n";
        } 
        return str;
    }
    #endregion


    
    #region Loaders
    
    
    private MonoLoader _monoLoader;
    public MonoLoader MonoLoader
    {
        get
        {
            if (_monoLoader == null)
                _monoLoader = FindObjectOfType<MonoLoader>();
            if (_monoLoader == null)
                Debug.LogWarning("! No MonoLoader GameObject found. Please re-load the tool from Launcher.");
            return _monoLoader;
        }
    }     
    
    private DBReader _dbReader;
    public DBReader DBReader
    {
        get
        {
            if (_dbReader == null)
                _dbReader = LoadDBReader();
            return _dbReader;
        }
    }
    private DBReader LoadDBReader()
    {
        DBReader dbReader  = AssetDatabase.LoadAssetAtPath<DBReader>(MonoLoader.projectPath+MonoLoader.rootPath+"DBReader.asset");
        if (dbReader == null)
            Debug.LogWarning("! No DBReader found. Please re-load the tool from Launcher.");
        return dbReader;
    }
    
    private DBWriter _dbWriter;
    public DBWriter DBWriter
    {
        get
        {
            if (_dbWriter == null)
                _dbWriter = LoadDBWriter();
            return _dbWriter;
        }
    }
    private DBWriter LoadDBWriter()
    {
        DBWriter dbWriter = AssetDatabase.LoadAssetAtPath<DBWriter>(MonoLoader.projectPath+MonoLoader.rootPath+"DBWriter.asset");
        if (dbWriter == null)
            Debug.LogWarning("! No DBWriter found. Please re-load the tool from Launcher.");
        return dbWriter;
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
        RootView rootView = AssetDatabase.LoadAssetAtPath<RootView>(MonoLoader.projectPath+MonoLoader.rootPath+"RootView.asset");
        if (rootView == null)
            Debug.LogWarning("! No RootView found. Please re-load the tool from Launcher.");
        return rootView;
    }
    private RootEdit _rootEdit;
    public RootEdit RootEdit
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
        RootEdit rootEdit = AssetDatabase.LoadAssetAtPath<RootEdit>(MonoLoader.projectPath+MonoLoader.rootPath+"RootEdit.asset");
        if (rootEdit == null)
            Debug.LogWarning("! No RootEdit found. Please re-load the tool from Launcher.");
        return rootEdit;
    } 
    private RootMap _rootMap;
    public RootMap RootMap
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
        RootMap rootMap = AssetDatabase.LoadAssetAtPath<RootMap>(MonoLoader.projectPath+MonoLoader.rootPath+"RootMap.asset");
        if (rootMap == null)
            Debug.LogWarning("! No RootMap found. Please re-load the tool from Launcher.");
        return rootMap;
    }
    private RootTimeline _rootTimeline;
    public RootTimeline RootTimeline
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
        RootTimeline  rootTimeline = AssetDatabase.LoadAssetAtPath<RootTimeline>(MonoLoader.projectPath+MonoLoader.rootPath+"RootTimeline.asset");
        if (rootTimeline == null)
            Debug.LogWarning("! No RootTimeline found. Please re-load the tool from Launcher.");
        return rootTimeline;
    }
    private WorldParser _worldParser;

    public WorldParser WorldParser
    {
        get
        {
            if (_worldParser == null)
                _worldParser = LoadWorldParser();
            return _worldParser;
        }
    }
    private WorldParser LoadWorldParser()
    {
        WorldParser  worldParser = AssetDatabase.LoadAssetAtPath<WorldParser>("Assets/Resources/Root Files/WorldParser.asset");
        if (worldParser == null)
            Debug.LogWarning("! No WorldParser found. Please re-load the tool from Launcher.");
        return worldParser;
    }
    #endregion

 
}
