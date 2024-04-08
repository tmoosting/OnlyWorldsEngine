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
        RootControl.APIHandler.FetchWorldWithKey(worldKeyTextField.value);
    }
    private void ClickButtonSend()
    {
      RootControl.APIHandler.SendDataAsync(worldKeyTextField.value);
     
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


    private string jsonStr =
        "{\"Character\":[{\"id\":\"018eb8ae-bd39-7285-a3e6-baf1e3fc195f\",\"name\":\"Liat Oron\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"birth_date\":0,\"species\":\"\",\"traits\":\"\",\"charisma\":0,\"coercion\":0,\"psychology\":\"\",\"height\":0,\"weight\":0,\"physicality\":\"\",\"languages\":\"\",\"abilities\":\"\",\"birthplace\":\"\",\"background\":\"\",\"motivations\":\"\",\"situation\":\"\",\"capability\":0,\"compassion\":0,\"creativity\":0,\"courage\":0,\"corruption\":0,\"location\":\"\",\"titles\":\"\",\"objects\":\"\",\"relations\":\"\",\"collectives\":\"\",\"backstory\":\"\",\"institutions\":\"\",\"events\":\"\",\"family\":\"\",\"friends\":\"\",\"rivals\":\"\",\"hit_points\":0,\"class\":\"\",\"alignment\":\"\",\"inspirations\":\"\",\"tt_str\":0,\"tt_int\":0,\"tt_con\":0,\"tt_dex\":0,\"tt_wis\":0,\"tt_cha\":0,\"skill_stealth\":0,\"equipment\":\"\",\"backpack\":\"\",\"proficiencies\":\"\",\"features\":\"\",\"spells\":\"\"}],\"Object\":[{\"id\":\"018eb8ae-c625-7e96-a8c1-b13aa0438a71\",\"name\":\"New Object\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"aesthetics\":\"\",\"weight\":0,\"parent_object\":\"\",\"technology\":\"\",\"components\":\"\",\"origins\":\"\",\"amount\":0,\"location\":\"\",\"rarity\":\"\",\"titles\":\"\",\"claimants\":\"\",\"craftsmanship\":\"\",\"requirements\":\"\",\"durability\":\"\",\"value\":0,\"damage\":0,\"armor\":0,\"enables\":\"\",\"effects\":\"\",\"consumes\":\"\",\"requires\":\"\",\"language\":\"\",\"utility\":\"\"}],\"Location\":[{\"id\":\"018eb893-f9b6-701c-a570-65b63684512a\",\"name\":\"New Location\",\"description\":\"asdadsd\",\"supertype\":\"Settlement\",\"subtype\":\"None\",\"image_url\":\"\",\"founding_date\":22,\"parent_location\":\"018eb8ad-2d94-7fc5-9d82-e45208ffaa42\",\"maps\":\"\",\"pins\":\"\",\"events\":\"\",\"places\":\"\",\"population_size\":33,\"coordinates\":\"\",\"scene\":\"asdds\",\"activity\":\"sadsd\",\"populations\":\"018eb894-d3ed-7c1a-877d-5385d79b4f3a\",\"founders\":\"\",\"government\":\"22\",\"opposition\":\"33\",\"governing_title\":\"\",\"primary_faction\":\"\",\"secondary_factions\":\"\",\"territorial_policies\":\"\",\"rival\":\"\",\"friend\":\"\",\"soft_influence_on\":\"\",\"hard_influence_on\":\"\",\"economics\":\"\",\"generation_rate\":0,\"industry_rate\":0,\"primary_resource\":\"\",\"primary_industry\":\"\",\"secondary_resources\":\"\",\"secondary_industries\":\"\",\"commerce\":\"\",\"coinage\":\"\",\"logistics\":\"\",\"harbor\":\"\",\"primary_generation_offload\":\"\",\"primary_industry_offload\":\"\",\"secondary_generation_offloads\":\"\",\"secondary_industry_offloads\":\"\",\"architecture\":\"\",\"construction_rate\":0,\"building_expertise\":\"\",\"buildings\":\"018eb894-5574-73ef-bfc6-1c8c1c90cac0\",\"defensibility\":\"\",\"height\":0,\"primary_fighter\":\"\",\"secondary_fighters\":\"\",\"defenses\":\"018eb894-5574-73ef-bfc6-1c8c1c90cac0\",\"fortifications\":\"\",\"celebrations\":\"33\",\"stories\":\"22\",\"primary_cult\":\"018eb89c-ca17-7a8d-88c9-47b9cca41c1a\",\"secondary_cults\":\"\",\"delicacies\":\"018eb89b-1608-7d8a-885e-e3fc8f98759c\"},{\"id\":\"018eb894-5574-73ef-bfc6-1c8c1c90cac0\",\"name\":\"bbb\",\"description\":\"\",\"supertype\":\"Building\",\"subtype\":\"None\",\"image_url\":\"\",\"founding_date\":0,\"parent_location\":\"\",\"maps\":\"\",\"pins\":\"\",\"events\":\"\",\"places\":\"\",\"population_size\":0,\"coordinates\":\"\",\"scene\":\"\",\"activity\":\"\",\"populations\":\"\",\"founders\":\"\",\"government\":\"\",\"opposition\":\"\",\"governing_title\":\"\",\"primary_faction\":\"\",\"secondary_factions\":\"\",\"territorial_policies\":\"\",\"rival\":\"\",\"friend\":\"\",\"soft_influence_on\":\"\",\"hard_influence_on\":\"\",\"economics\":\"\",\"generation_rate\":0,\"industry_rate\":0,\"primary_resource\":\"\",\"primary_industry\":\"\",\"secondary_resources\":\"\",\"secondary_industries\":\"\",\"commerce\":\"\",\"coinage\":\"\",\"logistics\":\"\",\"harbor\":\"\",\"primary_generation_offload\":\"\",\"primary_industry_offload\":\"\",\"secondary_generation_offloads\":\"\",\"secondary_industry_offloads\":\"\",\"architecture\":\"\",\"construction_rate\":0,\"building_expertise\":\"\",\"buildings\":\"\",\"defensibility\":\"\",\"height\":0,\"primary_fighter\":\"\",\"secondary_fighters\":\"\",\"defenses\":\"\",\"fortifications\":\"\",\"celebrations\":\"\",\"stories\":\"\",\"primary_cult\":\"\",\"secondary_cults\":\"\",\"delicacies\":\"\"},{\"id\":\"018eb8ad-2d94-7fc5-9d82-e45208ffaa42\",\"name\":\"New Location\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"founding_date\":0,\"parent_location\":\"\",\"maps\":\"\",\"pins\":\"\",\"events\":\"\",\"places\":\"\",\"population_size\":0,\"coordinates\":\"\",\"scene\":\"\",\"activity\":\"\",\"populations\":\"\",\"founders\":\"\",\"government\":\"\",\"opposition\":\"\",\"governing_title\":\"\",\"primary_faction\":\"\",\"secondary_factions\":\"\",\"territorial_policies\":\"\",\"rival\":\"\",\"friend\":\"\",\"soft_influence_on\":\"\",\"hard_influence_on\":\"\",\"economics\":\"\",\"generation_rate\":0,\"industry_rate\":0,\"primary_resource\":\"\",\"primary_industry\":\"\",\"secondary_resources\":\"\",\"secondary_industries\":\"\",\"commerce\":\"\",\"coinage\":\"\",\"logistics\":\"\",\"harbor\":\"\",\"primary_generation_offload\":\"\",\"primary_industry_offload\":\"\",\"secondary_generation_offloads\":\"\",\"secondary_industry_offloads\":\"\",\"architecture\":\"\",\"construction_rate\":0,\"building_expertise\":\"\",\"buildings\":\"\",\"defensibility\":\"\",\"height\":0,\"primary_fighter\":\"\",\"secondary_fighters\":\"\",\"defenses\":\"\",\"fortifications\":\"\",\"celebrations\":\"\",\"stories\":\"\",\"primary_cult\":\"\",\"secondary_cults\":\"\",\"delicacies\":\"\"},{\"id\":\"018eb8b9-91ba-7c2b-be18-e719d34070ae\",\"name\":\"New Location\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"founding_date\":0,\"parent_location\":\"\",\"maps\":\"\",\"pins\":\"\",\"events\":\"\",\"places\":\"\",\"population_size\":0,\"coordinates\":\"\",\"scene\":\"\",\"activity\":\"\",\"populations\":\"\",\"founders\":\"\",\"government\":\"\",\"opposition\":\"\",\"governing_title\":\"\",\"primary_faction\":\"\",\"secondary_factions\":\"\",\"territorial_policies\":\"\",\"rival\":\"\",\"friend\":\"\",\"soft_influence_on\":\"\",\"hard_influence_on\":\"\",\"economics\":\"\",\"generation_rate\":0,\"industry_rate\":0,\"primary_resource\":\"\",\"primary_industry\":\"\",\"secondary_resources\":\"\",\"secondary_industries\":\"\",\"commerce\":\"\",\"coinage\":\"\",\"logistics\":\"\",\"harbor\":\"\",\"primary_generation_offload\":\"\",\"primary_industry_offload\":\"\",\"secondary_generation_offloads\":\"\",\"secondary_industry_offloads\":\"\",\"architecture\":\"\",\"construction_rate\":0,\"building_expertise\":\"\",\"buildings\":\"\",\"defensibility\":\"\",\"height\":0,\"primary_fighter\":\"\",\"secondary_fighters\":\"\",\"defenses\":\"\",\"fortifications\":\"\",\"celebrations\":\"\",\"stories\":\"\",\"primary_cult\":\"\",\"secondary_cults\":\"\",\"delicacies\":\"\"}],\"Species\":[{\"id\":\"018eb8ae-f1dc-7be7-85e8-3003e3a368da\",\"name\":\"New Species\",\"description\":\"eer\",\"supertype\":\"Folk\",\"subtype\":\"None\",\"image_url\":\"\",\"life_span\":0,\"average_weight\":0,\"appearance\":\"\",\"habitats\":\"\",\"agency\":\"\",\"aggression\":0,\"traits\":\"\",\"languages\":\"\",\"consumption\":\"\",\"instincts\":\"\",\"behaviour\":\"\",\"prey\":\"\",\"predators\":\"\"}],\"Territory\":[{\"id\":\"018eb8ae-e738-73b6-83b2-8d85f9c7ed48\",\"name\":\"New Territory\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"size\":0,\"terrain\":\"\",\"locations\":\"\",\"claimants\":\"\",\"creatures\":\"\",\"parent_territory\":\"\",\"sub_territories\":\"\",\"history\":\"\",\"species\":\"\",\"phenomena\":\"\",\"events\":\"\",\"conditions\":\"\",\"yield_rate\":0,\"primary_resource\":\"\",\"secondary_resources\":\"\"}],\"Institution\":[{\"id\":\"018eb8ae-cec7-7637-a900-a943c2d42cc4\",\"name\":\"New Institution\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"premise\":\"\",\"parent_institution\":\"\",\"cooperates\":\"\",\"dominion\":\"\",\"found_date\":0,\"end_date\":0,\"situation\":\"\",\"constructs\":\"\",\"phenomena\":\"\",\"events\":\"\",\"collectives\":\"\",\"characters\":\"\",\"titles\":\"\",\"competition\":\"\",\"locations\":\"\",\"territories\":\"\",\"creatures\":\"\",\"objects\":\"\",\"legal\":\"\"}],\"Family\":[],\"Creature\":[{\"id\":\"018eb8ae-d6dc-769d-92e3-143a69fefb85\",\"name\":\"New Creature\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"appearance\":\"\",\"weight\":0,\"height\":0,\"species\":\"\",\"instincts\":\"\",\"abilities\":\"\",\"demeanour\":\"\",\"habitat\":\"\",\"location\":\"\",\"territory\":\"\",\"collectives\":\"\",\"lore\":\"\",\"senses\":\"\",\"hit_points\":0,\"armor_class\":0,\"challenge_rating\":0,\"speed\":0,\"tt_str\":0,\"tt_int\":0,\"tt_con\":0,\"tt_dex\":0,\"tt_wis\":0,\"tt_cha\":0,\"languages\":\"\",\"traits\":\"\",\"actions\":\"\",\"reactions\":\"\",\"alignment\":\"\"}],\"Collective\":[],\"Trait\":[{\"id\":\"018eb8ae-df4e-7048-b3cb-7f6cffd96f6c\",\"name\":\"New Trait\",\"description\":\"\",\"supertype\":\"None\",\"subtype\":\"None\",\"image_url\":\"\",\"charisma\":0,\"coercion\":0,\"physical_effects\":\"\",\"mental_effects\":\"\",\"behaviour_effects\":\"\",\"ability_effects\":\"\",\"capability\":0,\"compassion\":0,\"creativity\":0,\"courage\":0,\"anti_trait\":\"\",\"empowered_abilities\":\"\",\"character_carriers\":\"\",\"creature_carriers\":\"\"}],\"Phenomenon\":[],\"Title\":[],\"Ability\":[],\"Language\":[],\"Law\":[],\"Relation\":[],\"Event\":[],\"Construct\":[],\"World\":{\"id\":\"018eb8ac-a2dc-7afd-8add-31be4268f3a7\",\"apiKey\":\"79531230\",\"name\":\"Hypalon\",\"description\":\"hmm\",\"user_id\":\"default_user_id\",\"ow_version\":\"14.00\",\"image_url\":\"default_image_url\",\"focusText\":\"\",\"time_format_names\":\"Eon,Era,Period,Epoch,Age,Year,Month,Day,Hour,Minute,Second\",\"time_format_equivalents\":\"Eon,Era,Period,Epoch,Age,Year,Month,Day,Hour,Minute,Second\",\"time_basic_unit\":\"Year\",\"time_current\":0,\"time_range_min\":0,\"time_range_max\":100}}";
}
