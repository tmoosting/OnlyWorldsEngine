using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "RootView", menuName = "ScriptableObjects/RootView", order = 0)]

public class RootView : ScriptableObject
{

    [Header("General")]
    public float ContentMargin = 1f;
    public int TopSelectorsHeight = 40;
    
    [Header("Browse")]
    public int CategorySelectorsHeight = 30;
    public int ElementSelectorHeight = 30;
    public int HideRowHeight = 20;
    public int ConceptsHeight = 20;
    public bool ShowSelectorLabels = true;
    public float iconSize = 1.5f;
    public float hoverAlpha = 1.5f;
    public float fontSizeTop = 13;
    public float fontSizeCategory = 12;
    public float fontSizeElement = 11;
    public int labelWidthPercent = 60;
    public int labelHeightPercent = 33;
    [Header("Browse - Search")] 
    public bool searchOnPressEnter = false;
    public float searchFontSize = 14f;
    public bool hoverOppositeSortBarField;
    public float sortBarHeight = 32f;

    [Header("Browse - Results")] 
    public int ResultRowHeight = 35;
    public int ResultRowIconSize = 20;
    public int ResultRowIconMargin = 4;
    public int ResultRowGeneralMargin = 6;
    
   
    
  //  [Header("Examine")]

  [Header("Colors")]
  public  Color backgroundColor; 
  public  Color customizeContentColor;  
  public  Color Scheme1Enabled ;
  public Color Scheme1Disabled;
  public  Color Scheme1Activated;  
  public  Color Scheme2Enabled; 
  public  Color Scheme2Disabled; 
  public  Color Scheme2Activated; 
  public  Color Scheme3Enabled; 
  public  Color Scheme3Disabled; 
  public  Color Scheme3Activated; 
  public  Color Scheme4Enabled; 
  public  Color Scheme4Disabled; 
  public  Color Scheme4Activated; 
  public  Color Scheme5Enabled; 
  public  Color Scheme5Disabled; 
  public  Color Scheme5Activated;
  public Color CustomizeBackgroundColor;
    
  [Header("Icons")]
  public   Sprite DefaultSprite;
    public   Sprite BrowseSprite;
    public   Sprite ExamineSprite;
    public   Sprite CharacterSprite;
    public   Sprite ObjectSprite;
    public   Sprite LocationSprite;
    public   Sprite FamilySprite;
    public   Sprite TerritorySprite;
    public   Sprite InstitutionSprite;
    public   Sprite RaceSprite; 
    public   Sprite CreatureSprite;
    public   Sprite TitleSprite;
    public   Sprite TraitSprite;
    public   Sprite GodSprite;
    public   Sprite CollectiveSprite;
    public   Sprite LanguageSprite;
    public   Sprite EventSprite;
    public   Sprite LawSprite;
    public   Sprite ConceptsSprite;


  
    
     

    public Sprite GetSelectorSprite(string name)
    { 
        if (name == "Browse")
            return BrowseSprite;
        else if (name == "Examine")
            return ExamineSprite;
        else if (name == "Character")
            return CharacterSprite;
        else if (name == "Object")
            return ObjectSprite;
        else if (name == "Matter")
            return ObjectSprite;
        else if (name == "Location")
            return LocationSprite;
        else if (name == "Family")
            return FamilySprite;
        else if (name == "Territory")
            return TerritorySprite;
        else if (name == "Institution")
            return InstitutionSprite;
        else if (name == "Race")
            return RaceSprite;
        else if (name == "Creature")
            return CreatureSprite;
        else if (name == "Title")
            return TitleSprite;
        else if (name == "Trait")
            return TraitSprite;
        else if (name == "God")
            return GodSprite;
        else if (name == "Collective")
            return CollectiveSprite; 
        else if (name == "Language")
            return LanguageSprite; 
        else if (name == "Event")
            return EventSprite; 
        else if (name == "Law")
            return LawSprite;    
        else if (name == "Concept")
            return ConceptsSprite; 
        else if (name == "Concepts")
            return ConceptsSprite; 
        return DefaultSprite;
    }
    public  StyleColor GetSelectorColor(int colorScheme, bool enabled, bool activated, bool hovered)
    {
        if (enabled == false)
        { 
            switch (colorScheme)
            {
                case 1:
                    return new StyleColor(Scheme1Disabled);
                case 2:
                    return new StyleColor(Scheme2Disabled);
                case 3:
                    return new StyleColor(Scheme3Disabled);
                case 4:
                    return new StyleColor(Scheme4Disabled);
                case 5:
                    return new StyleColor(Scheme5Disabled);
                default:
                    Debug.LogError("Invalid color scheme: " + colorScheme);
                    return new StyleColor(Color.black); 
            }
        }
        else   if (activated)
        {
            switch (colorScheme)
            {
                case 1:
                    return new StyleColor(Scheme1Activated);
                case 2:
                    return new StyleColor(Scheme2Activated);
                case 3:
                    return new StyleColor(Scheme3Activated);
                case 4:
                    return new StyleColor(Scheme4Activated);
                case 5:
                    return new StyleColor(Scheme5Activated);
                default:
                    Debug.LogError("Invalid color scheme: " + colorScheme);
                    return new StyleColor(Color.black); 
            }
        }
        else if (hovered)
        { 
            switch (colorScheme)
            {
                case 1:
                    return new StyleColor(TintColor(Scheme1Enabled)); 
                case 2:
                    return new StyleColor(TintColor(Scheme2Enabled)); 
                case 3:
                    return new StyleColor(TintColor(Scheme3Enabled)); 
                case 4:
                    return new StyleColor(TintColor(Scheme4Enabled)); 
                case 5:
                    return new StyleColor(TintColor(Scheme5Enabled)); 
                default:
                    Debug.LogError("Invalid color scheme: " + colorScheme);
                    return new StyleColor(Color.black);
            }
        }
        /*else if (hovered)
        {
            switch (colorScheme)
            {
                case 1:
                    return new StyleColor(new Color(Scheme1Enabled.r, Scheme1Enabled.g, Scheme1Enabled.b, hoverAlpha));
                case 2:
                    return new StyleColor(new Color(Scheme2Enabled.r, Scheme2Enabled.g, Scheme2Enabled.b, hoverAlpha));
                case 3:
                    return new StyleColor(new Color(Scheme3Enabled.r, Scheme3Enabled.g, Scheme3Enabled.b, hoverAlpha));
                case 4:
                    return new StyleColor(new Color(Scheme4Enabled.r, Scheme4Enabled.g, Scheme4Enabled.b, hoverAlpha));
                case 5:
                    return new StyleColor(new Color(Scheme5Enabled.r, Scheme5Enabled.g, Scheme5Enabled.b, hoverAlpha));
                default:
                    Debug.LogError("Invalid color scheme: " + colorScheme);
                    return new StyleColor(Color.black);
            }
        }*/
        /*else   if (hovered)
        {
            switch (colorScheme)
            {
                case 1:
                    return new StyleColor(Scheme1Hovered);
                case 2:
                    return new StyleColor(Scheme2Hovered);
                case 3:
                    return new StyleColor(Scheme3Hovered);
                case 4:
                    return new StyleColor(Scheme4Hovered);
                case 5:
                    return new StyleColor(Scheme5Hovered);
                default:
                    Debug.LogError("Invalid color scheme: " + colorScheme);
                    return new StyleColor(Color.black); 
            }
        }*/
        else
        {
            switch (colorScheme)
            {
                case 1:
                    return new StyleColor(Scheme1Enabled);
                case 2:
                    return new StyleColor(Scheme2Enabled);
                case 3:
                    return new StyleColor(Scheme3Enabled);
                case 4:
                    return new StyleColor(Scheme4Enabled);
                case 5:
                    return new StyleColor(Scheme5Enabled);
                default:
                    Debug.LogError("Invalid color scheme: " + colorScheme);
                    return new StyleColor(Color.black);
            }
        }
    }

    private Color TintColor(Color originalColour)
    {
        // Define the overlay color as semi-transparent white
        Color overlay = new Color(1f, 1f, 1f, 0.3f);

        // Blend the original color with the overlay
        Color tintedColor = Color.Lerp(originalColour, overlay, overlay.a);

        return tintedColor;
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
