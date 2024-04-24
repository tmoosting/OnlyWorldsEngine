using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = World_Model.Elements.Object;


public class Map
{
    public enum Type
    {
        Undefined = 0,
        Space = 1,
        Planet = 2,
        Region = 3,
        Location = 4,
        Building = 5,
        Battle = 6
    }
    
    public string ID{ get; set; }
    public string Name{ get; set; }
    public string Supertype{ get; set; }
    public string Subtype{ get; set; }
    public string Description{ get; set; }
    public string FileName{ get; set; }
    public string BackgroundColor{ get; set; }
    public string ParentMap{ get; set; }
    public int Hierarchy{ get; set; }
    public Type type;

    public Map()
    {
        type = Type.Undefined;
    }


    public Map(string mapName, string fileName)
    {
        ID = System.Guid.NewGuid().ToString();
        Name = mapName;
        Supertype = "Undefined";
        Subtype = "Undefined";
        FileName = fileName;
        BackgroundColor = "#383838";
        Hierarchy = 0;
        type = Type.Undefined;
    }
    
 
    
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

}
