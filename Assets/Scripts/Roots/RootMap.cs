using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using World_Model;
using Object = World_Model.Elements.Object;


// Responsible for map, pin, pinelement:  creation, management, linking

[CreateAssetMenu(fileName = "RootMap", menuName = "ScriptableObjects/RootMap", order = 0)]

public class RootMap : ScriptableObject
{
    [Header("General")] 
    public bool selectPinOnElementSelect = true;
    public bool hoverPinColor = false;
    public float pinWindowWidth = 200f;
    public float defaultPinSize = 20f;
    public float pinHoverScaleMultiplier = 1.2f;
    
    [Header("Zooming")] 
    public float zoomSpeed = 0.05f;
    public float zoomMin = 0.5f;
    public float zoomMax = 20f;
    public float zoomScaleThresholdOne = 5f; // take as 1/thisvalue. best leave as is
    public float zoomScaleThresholdTwo = 2f;
    public float zoomScaleOneAlpha = 0.2f;
    public float zoomScaleTwoAlpha = 0.4f;

    [Header("Pin Visuals")] 
    public bool showNameFlags = true;
    public bool constantPinScale = true;
    [Tooltip("Zoomscales are three levels, the lowest of which get obscured at certain zoom-out thresholds")]
    public bool useZoomscales = false;
    public float pinColoringSize = 20f;
    public float pinScaling = 1f;
    public float pinMinScaling = 0.1f;
    public float pinMaxScaling = 3f;
    public float pinCenterIconSize = 20f;
    public Color pinBaseColor;
    public Color pinHoverColor;
    public Color pinSelectedColor;
    public Color pinHighlightColor; // when preselected in pinwindow list
    
    [Header("Sprites")] 
    public Sprite pinBaseSprite;
    public Sprite pinWhiteSprite;
    public Sprite pinHighlightSprite;
    public Sprite pinHoverSprite;
    public Sprite pinBannerSprite;
    public Sprite pinRowIconElement;
    public Sprite pinRowIconMap;
    public Sprite pinRowIconDescriptive;  
    
    [Header("Colors")]
    public Color pinWindowColor = new Color(0.25f, 0.3f, 0.4f);
    public Color pinSortBarColor;
    
    [Header("Browse - Results")] 
    public int ResultRowHeight = 35;
    public int ResultRowIconSize = 20; 
    public int ResultRowIconMargin = 4;

    
    // active Pin is a public RootControl getter
    public PinElement hoveredPinElement;
    
    public delegate void PinsRefreshHandler();
    public delegate void PinCenteringHandler(PinElement pinElement);
    public delegate void PinListDeselectHandler();

    public event PinsRefreshHandler OnPinRefresh;
    public event PinCenteringHandler OnPinCentering;
    public event PinListDeselectHandler OnPinListDeselect;


    public List<PinElement> activePinElements;

    
    public void ResetSettings()
    {
        activePinElements = new List<PinElement>();
        highlightMode = false;
        pinScaling = 1f;
        filterMode = RootMap.FilterMode.None;
        includedElementTables = new List<Element.Category>();
    }

    public List<string> GetUnpinnedMapNames()
    {
        List<string> mapNames = RootControl.World.Maps.ToList().Select(map => map.Name).ToList();
        List<string> returnNames = mapNames.ToList(); 
        foreach (var activePinElement in activePinElements)
            if (activePinElement.pin.category == Pin.Category.Map)
                if (mapNames.Contains(activePinElement.pin.PinnedMap)  )
                {
                    returnNames.Remove(activePinElement.pin.PinnedMap);
                }
    
        return returnNames;
    }
    
    public Texture2D GetActiveMapTexture()
    {
        if (RootControl.Map == null)
            return AssetDatabase.LoadAssetAtPath<Texture2D>(RootControl.MonoLoader.projectPath + "Resources/Images/Maps/Default.png");
       
        Texture2D loadedTexture =  AssetDatabase.LoadAssetAtPath<Texture2D>
            (RootControl.MonoLoader.projectPath + "Resources/Images/Maps/" + RootControl.Map.FileName + ".png");
        if (loadedTexture == null)
            return AssetDatabase.LoadAssetAtPath<Texture2D>(RootControl.MonoLoader.projectPath + "Resources/Images/Maps/Default.png"); 
        return loadedTexture;
    }
    public Texture2D GetMapTexture(string fileName)
    {
        Texture2D loadedTexture =  AssetDatabase.LoadAssetAtPath<Texture2D>
            (RootControl.MonoLoader.projectPath + "Resources/Images/Maps/" + fileName + ".png");
        if (loadedTexture == null)
            return AssetDatabase.LoadAssetAtPath<Texture2D>(RootControl.MonoLoader.projectPath + "Resources/Images/Maps/Default.png"); 
        return loadedTexture;
    }
    public float GetPinSize()
    {
        return defaultPinSize;
    }
    
    public Sprite GetPinBaseSprite()
    {
        if (pinBaseSprite == null)
            Debug.LogWarning("! No pinBaseSprite assigned!");
        return pinBaseSprite;
    }
    public Sprite GetPinHighlightSprite()
    {            
        if (pinHighlightSprite == null)
            Debug.LogWarning("! No pinHighlightSprite assigned!");
        return pinHighlightSprite;
    }  
    public Sprite GetPinHoverSprite()
    {            
        if (pinHoverSprite == null)
            Debug.LogWarning("! No pinHoverSprite assigned!");
        return pinHoverSprite;
    }
    public Sprite GetNameFlagSprite()
    {
        if (pinBannerSprite == null)
            Debug.LogWarning("! No pinBannerSprite assigned!");
        return pinBannerSprite;
    }
    public Sprite GetPinCenterIconSprite(Element.Category category)
    {
        return RootControl.RootView.GetSelectorSprite(category.ToString());
    }
    public void CreateElementAndPin(PinElement pinElement, string elementName, string elementType)
    {
        Element.Category elementCategory = (Element.Category)Enum.Parse(typeof(Element.Category), elementType); 
        Element newElement =  RootControl.RootEdit.CreateElement(elementCategory, elementName);
        CreatePin(Pin.Category.Element, newElement.Name, new Vector2( pinElement.relativeX, pinElement.relativeY),newElement.ID );
        RootControl.SetElement(newElement);
    }
    public void CreateLinkedElementPin(PinElement pinElement, Element element)
    {
        CreatePin(Pin.Category.Element,element.Name,  new Vector2(  pinElement.relativeX, pinElement.relativeY), element.ID);
    }
    public void CreateDescriptivePin(PinElement pinElement, string pinName, string description)
    {
        CreatePin(Pin.Category.Descriptive,pinName , new Vector2(  pinElement.relativeX, pinElement.relativeY), null, description);

    }   
    public void CreateMapPin(PinElement pinElement,string pinName, Map pinnedMap)
    {
        CreatePin(Pin.Category.Map,pinName,new Vector2(  pinElement.relativeX, pinElement.relativeY), null, "", pinnedMap);
        pinnedMap.ParentMap = RootControl.Map.ID;
    }

    private void CreatePin(Pin.Category category, string pinName, Vector2 coords, string elementID = null, string description = "", Map pinnedMap = null)
    {
        Pin pin = new Pin();
        pin.ID = System.Guid.NewGuid().ToString();
        pin.Name = pinName;
        if (pinName == "")
            if (pinnedMap != null)
                pin.Name = pinnedMap.Name;
        pin.category = category;
        pin.Type = category.ToString();
        if (elementID != null)
            pin.Element = elementID;
        if (pinnedMap != null)
             pin.PinnedMap = pinnedMap.Name;
        pin.Description = description;
        pin.Map = RootControl.Map.ID;
        pin.CoordX = coords.x;
        pin.CoordY = coords.y;
        pin.Zoomscale = 3;
        pin.ToggleBase = true;
        pin.ToggleColor = true;
        pin.ToggleIcon = true;
        pin.ToggleName = true; 
        Debug.Log($"Created Pin - CoordX: {pin.CoordX}, CoordY: {pin.CoordY}");

        RootControl.World.Pins.Add(pin);
        RootControl.SetPin(pin);
    }
    public void StoreTempPinElement(PinElement pinElement)
    {
        if (activePinElements == null)
            activePinElements = new List<PinElement>();
        activePinElements.Add(pinElement);
    }
    public void SelectMapFromDropdown(string mapName)
    {
        RootControl.SetMap( GetMapForName(mapName));
    }
    public Map CreateMap(string mapName, string fileName)
    {
        Map newMap = new Map(mapName, fileName);
        RootControl.World.Maps.Add(newMap);
        return newMap;
    }

    public void DeletePin(Pin pin, bool skipRebuild = false)
    {
        RootControl.World.Pins.Remove(pin);
        RootControl.SetPin(null, true);
        if (skipRebuild == false) 
            RebuildPinElements(); 
        RootControl.ElementRefresh();
    } 
    public void DeleteMap(string mapName)
    {
        Map map = GetMapForName(mapName);
        foreach (var pin in GetPinsForMap(map))
            DeletePin(pin, true);
        RootControl.World.Maps.Remove(map);
        if (RootControl.Map == map)
            if ( RootControl.World.Maps  != null)
                if ( RootControl.World.Maps.Count > 0)
                      RootControl.SetMap(RootControl.World.Maps[0]);
    }

    public List<Pin> GetPinsForMap(Map map)
    {
        return RootControl.World.Pins.Where(pin => pin.Map == map.ID).ToList();
    }
    [HideInInspector] public bool highlightMode = false;
    /// <summary>
    /// called when a pin is clicked in the Pin Browser listview
    /// make transparent all pins, except RootControl.Pin if not null
    /// </summary>
    public void SecondaryHighlightSinglePin(Pin pin)
    {
        highlightMode = true;
        // foreach (var pin1 in RootControl.GetPinsForCurrentMap())
        PinElement activePinElement = GetPinElementForPin(pin);
        activePinElement .SetTransparencyFull();
        activePinElement.SecondaryHighlight();
        foreach (var pinElement in activePinElements)
            if (pinElement.pin != pin)
            {
                if (RootControl.Pin == null)
                    pinElement.SetTransparencyLow();
                else
                {
                    if (RootControl.Pin != pinElement.pin)
                        pinElement.SetTransparencyLow();
                }
            }
    }

    public void ResetHighlightMode(bool rebuildElements = false)
    { 
        highlightMode = false;
        if (rebuildElements)
              RebuildPinElements();
        OnPinListDeselect?.Invoke();
    }


    public enum FilterMode
    {
        None,
        Element,
        Map,
        Descriptive
    }

    [HideInInspector] public FilterMode filterMode;
    public List<Element.Category> includedElementTables;
    
    public void RebuildPinElements()
    {  
//        Debug.Log("RebuildPinElements");
        DestroyActivePinElements();
        activePinElements = new List<PinElement>();
        CreatePinElementsFromPins();
        StylePins(); // pinelement base styling
        OnPinRefresh?.Invoke();  // Calls IntegratePinElements in MapWindow, which sets position, sprite, zoomscale transparency, and adds to visualelement
        ResolvePinTransparencies();
    }

    private void ResolvePinTransparencies()
    {
        if (filterMode != FilterMode.None)
        {
            if (filterMode == FilterMode.Element)
            {
                foreach (var activePinElement in activePinElements)
                {
                    if (includedElementTables == null || includedElementTables.Count == 0)
                    {
                        if (activePinElement.pin.category == Pin.Category.Element)
                            activePinElement.SetTransparencyFull();
                        else
                            activePinElement.SetTransparencyHidden();
                    }
                    else
                    {
                        if (activePinElement.pin.category == Pin.Category.Element && includedElementTables.Contains( GetElementForPin(activePinElement.pin).category))
                            activePinElement.SetTransparencyFull();
                        else
                            activePinElement.SetTransparencyHidden();
                    }
                }  
            }
            else   if (filterMode == FilterMode.Map)
            {
                foreach (var activePinElement in activePinElements)
                {
                    if (activePinElement.pin.category == Pin.Category.Map)
                        activePinElement.SetTransparencyFull();
                    else
                        activePinElement.SetTransparencyHidden();
                }
            }
            else   if (filterMode == FilterMode.Descriptive)
            {
                foreach (var activePinElement in activePinElements)
                {
                    if (activePinElement.pin.category == Pin.Category.Descriptive)
                        activePinElement.SetTransparencyFull();
                    else
                        activePinElement.SetTransparencyHidden();
                }
            }
           
        }
    }
    private void CreatePinElementsFromPins()
    {
        RootControl.ValidateNullCheck("World");
        if (RootControl.Map == null)
            return;
        foreach (var pin in RootControl.GetPinsForCurrentMap())
        {
            PinElement pinElement = new PinElement(pin);
            activePinElements.Add(pinElement);
        }
    }
    private void StylePins()
    {
        foreach (var activePinElement in activePinElements)
        {
            if (showNameFlags)
                activePinElement.ToggleNameFlag(true);  
            activePinElement.ApplyGlobalScale();
        }
        
        if (RootControl.Pin != null) 
               GetPinElementForPin(RootControl.Pin).SelectHighlight(); 
    }

    public void LeftClickPin(Pin pin)
    { 
        
        if (pin.category == Pin.Category.Element)
        {
            RootControl.SetPin(pin, true);
            Element element =  GetElementForPin(RootControl.Pin);
            if (element != null)
                RootControl.SetElement( element);
        }
        else
            RootControl.SetPin(pin);
        
    }
    public void RightClickPin(Pin pin)
    {
        
    }
    private void HighlightSelectedPin()
    {
        /*Pin hiPin = null;
        if (RootControl.Element != null)
        {
            Pin elementPin = GetPinForElement(RootControl.Element.ID);
            if (elementPin != null)
            {
                hiPin = elementPin;
            }
        }*/
    }

    private void DestroyActivePinElements()
    {
        if (activePinElements != null)
        {
            foreach (var pinElement in activePinElements)
                if (pinElement != null)
                      pinElement.RemoveFromHierarchy(); // removes the visual element from its parent and unregisters it from the panel
            activePinElements.Clear(); 
        }
    }
    
    public void UpdateMapData(string mapName, Map.Type mapType, Color color, string description)
    { 
        Map map = RootControl.Map;
        map.Name = mapName;
        map.type = mapType;
        map.Supertype = mapType.ToString();
        /*if (mapParent != "" && mapParent != "None")
            map.ParentMap =mapParent;*/
        map.BackgroundColor = ColorToHexString( color);
        map.Description =description;
    }
    
    public List<Map> OrderMapList(List<Map> mapList)
    {
        mapList.Sort((map1, map2) =>
        {
            // Sort by hierarchy in ascending order
            int hierarchyComparison = map1.Hierarchy.CompareTo(map2.Hierarchy);
            if (hierarchyComparison != 0)
                return hierarchyComparison;

            // If hierarchy is same, sort by whether it has a parent map or not
            bool map1HasParent = !string.IsNullOrEmpty(map1.ParentMap);
            bool map2HasParent = !string.IsNullOrEmpty(map2.ParentMap);

            if (map1HasParent && !map2HasParent)
                return 1;  // If map1 has a parent and map2 does not, map2 goes first
            if (!map1HasParent && map2HasParent)
                return -1; // If map1 does not have a parent and map2 does, map1 goes first

            // If hierarchy and parent map status are the same, sort by type
            // 'Undefined' should come last
            if (map1.type == Map.Type.Undefined && map2.type != Map.Type.Undefined)
                return 1; // If map1 is 'Undefined' and map2 is not, map2 goes first
            if (map1.type != Map.Type.Undefined && map2.type == Map.Type.Undefined)
                return -1; // If map1 is not 'Undefined' and map2 is, map1 goes first
        
            return map1.type.CompareTo(map2.type);
        });

        return mapList;
    }

    /// <summary>
    /// find the map 
    /// </summary>
    /// <returns></returns>
    public Map GetBaseMap()
    { 
        List<Map> maps = RootControl.World.Maps;
        if (maps.Count == 0)
            return null;
        if (maps.Count == 1)
            return maps[0];

        return OrderMapList(maps)[0]; 
    }

    public PinElement GetPinElementForPin(Pin pin)
    {
        foreach (var pinElement in activePinElements)
            if (pinElement.ID == pin.ID)
                return pinElement;
        Debug.LogWarning("!Error! Did not find a PinElement for Pin ID: " + pin.ID + " Name: " + pin.Name);
        return null;
    }
    public Pin  GetPinForPinElement(PinElement pinElement)
    {
        foreach (var pin in RootControl.World.Pins)
            if (pin.ID == pinElement.ID)
                return pin;
        Debug.LogWarning("!Error! Did not find a Pin for PinElement ID: " + pinElement.ID );
        return null;
    }

    public bool DoesElementHavePinOnMap (string elementID, string mapID = null)
    {
        if (mapID == null)
        {
            if (RootControl.Map == null)
                Debug.LogWarning("!Attempting to infer Map but none is active! Error!");
            mapID = RootControl.Map.ID;
        }
        foreach (var worldPin in RootControl.GetPinsForCurrentMap())
            if (worldPin.Element == elementID)
                    return true;
        return false;
    }
    public bool DoesElementHavePinOnMap (Element element, string mapID = null)
    {
        if (mapID == null)
        {
     //       if (RootControl.Map.ID == null)
                return false; 
        }
        foreach (var worldPin in RootControl.GetPinsForCurrentMap())
            if (worldPin.Element == element.ID) 
                    return true;
        return false;
    }
    public Pin GetPinForElement(string elementID, string mapID = null)
    {
        if (mapID == null)
        {
            if (RootControl.Map == null)
                Debug.LogWarning("!Attempting to infer Map but none is active! Error!");
            mapID = RootControl.Map.ID;
        }
        Pin pin = null;
        foreach (var worldPin in RootControl.World.Pins)
            if (worldPin.Element == elementID)
                if (worldPin.Map == mapID)
                  return worldPin;
        return pin;
    }
    public Pin GetPinForElement(Element element, string mapID = null)
    {
        if (mapID == null)
        {
            if (RootControl.Map == null)
                Debug.LogWarning("!Attempting to infer Map but none is active! Error!");
            mapID = RootControl.Map.ID;
        }
        Pin pin = null;
        foreach (var worldPin in RootControl.World.Pins)
            if (worldPin.Element == element.ID)
                if (worldPin.Map == mapID)
                    return worldPin;
        return pin;
    }

    public Element GetElementForPin(Pin pin, string mapID = null)
    {
        if (mapID == null)
        {
            if (RootControl.Map == null)
                Debug.LogWarning("!Attempting to infer Map but none is active! Error!");
            mapID = RootControl.Map.ID;
        }
        Element returnElement = null;
        if (pin.category != Pin.Category.Element)
            Debug.LogWarning("!Attempting to get Element for a Pin that is not of element category! " + pin.Name);
        foreach (var element in RootControl.GetWorldElements())
            if (pin.Element == element.ID)
                if (pin.Map == mapID)
                    return element;
        return returnElement;
    }

    public Map GetMapForName(string mapName)
    { 
        foreach (var map in RootControl.World.Maps)
            if (map.Name == mapName)
                return map;
        Debug.LogWarning("! Did not find a map for name: " + mapName + " in World: " + RootControl.World.Name);
        return null;
    }
    
    
    public static Color HexStringToColor(string hex)
    {
        hex = hex.Replace("0x", "");  // In case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");   // In case the string is formatted #FFFFFF
        byte a = 255;                // Assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        // Only use alpha if the string has enough characters
        if(hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }


    public bool AttemptSelectPinForElement(Element element)
    {
        bool pinFound = false;
        Pin pin = GetPinForElement(element);
        if (pin != null)
        {
            pinFound = true;
        } 
        return pinFound;
    }
    public void AttemptSelectAndCenterCameraOnPinForElement(Element element)
    {
        if (DoesElementHavePinOnMap(element))
            OnPinCentering?.Invoke(GetPinElementForPin(GetPinForElement(element)));
    }
    public static string ColorToHexString(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        int a = Mathf.RoundToInt(color.a * 255);
        string hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
        return hex;
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


  
}
