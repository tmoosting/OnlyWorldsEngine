using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using World_Model;
using World_Model.Elements;
using Event = World_Model.Elements.Event;
using Object = World_Model.Elements.Object;

// Checks World files in assets. Loads, validates, initializes them
// Could maybe rename to: WorldParser? 

[CreateAssetMenu(fileName = "WorldParser", menuName = "ScriptableObjects/WorldParser", order = 0)]
public class WorldParser : ScriptableObject
{ 

    //todo integrate World meta data:
      // Timeliner: start date; end date; dividers; World date;
    
      
      #region Public Access / Getters

      public string GetOriginalSupertypeForPotentialCustomSupertype(Element element, string givenSupertype)
      {
          foreach (var tableTyping in GetTypingTablesForElementTable(element.category))
              if (givenSupertype == tableTyping.GetPotentialCustomSupertype())
                      return tableTyping.GetOriginalSupertype();
          Debug.LogWarning("! Did not find Original Supertype for element: " + element.Name + " and supertype: " + givenSupertype);
          return "";
      } 
      
      public string GetOriginalSubtypeForPotentialCustomSubtype(Element element, string givenSubtype)
      {
          foreach (var tableTyping in GetTypingTablesForElementTable(element.category))
              if (tableTyping.GetPotentiallyCustomSubtypes().Contains(givenSubtype))
              {
                  int index = tableTyping.GetPotentiallyCustomSubtypes().IndexOf(givenSubtype);
                  return tableTyping.GetOriginalSubtypes()[index]; 
              }
          Debug.LogWarning("! Did not find Original Subtype for element: " + element.Name + " and subtype: " + givenSubtype);
          return "";
      }
      
      public string GetPotentialCustomSupertypeForElement(Element element)
      {
          foreach (var tableTyping in  GetTypingTablesForElementTable(element.category))
              if (element.Supertype == tableTyping.Supertype)
                  return tableTyping.GetPotentialCustomSupertype();
          Debug.LogWarning("! Did not find Effective Supertype for element: " + element.Name + " With supertype: " + element.Supertype);
          return "";
      }    
      public string GetPotentialCustomSubtypeForElement(Element element)
      {
          foreach (var tableTyping in  GetTypingTablesForElementTable(element.category))
              if (element.Supertype == tableTyping.Supertype)
                  return tableTyping.GetPotentialCustomSubtype(element.Subtype);
          Debug.LogWarning("! Did not find Effective Supertype for element: " + element.Name + " With supertype: " + element.Supertype);
          return "";
      }
      
      public List<TableTyping> GetTypingTablesForElementTable(Element.Category category)
      {
          List<TableTyping> returnList = new List<TableTyping>();

          TableTyping noneType = new TableTyping("None"); 
          returnList.Add(noneType);
          if (category == Element.Category.Character)
              returnList.AddRange( RootControl.World.TypesCharacter); 
          if (category == Element.Category.Location)
              returnList.AddRange(  RootControl.World.TypesLocation);
           
          return returnList;
      }

      public TableTyping GetTypingTableForElement(Element element)
      {
          List<TableTyping> potentialTypingTables = GetTypingTablesForElementTable(element.category);
          foreach (var potentialTypingTable in potentialTypingTables)
              if (potentialTypingTable.GetOriginalSupertype() == element.Supertype)
                  return potentialTypingTable;
          Debug.LogWarning("Did not find a Subtyping Table for Element: " + element.ID);
          return null;
      }
      
    public List<string> GetWorldsFileNames()
    { 
        List<string> worldFileNames = new List<string>();
        try
        {
            var info = new DirectoryInfo("Assets/Resources/Worlds");
            var fileInfo = info.GetFiles()
                .Where(file => file.Extension == ".db")
                .OrderByDescending(file => file.Length);

            foreach (FileInfo file in fileInfo)
                worldFileNames.Add(Path.GetFileNameWithoutExtension(file.Name));
            
            // Check if "Default.db" is in the folder
            if (!fileInfo.Any(file => file.Name == "Default.db"))
            {
                CreateDefault();
                worldFileNames.Add(Path.GetFileNameWithoutExtension("Default.db"));
            } 
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
        return worldFileNames;
    }

    void CreateDefault()
    {
        Debug.Log("CRD");
        string sourcePath = RootControl.MonoLoader.projectPath+ "Resources/DatabaseTemplate.db";
        string destinationPath =RootControl.MonoLoader.projectPath+ RootControl.MonoLoader.worldPath+ "Default.db";
        File.Copy(sourcePath, destinationPath, true); // "true" to overwrite existing file if it exists
        AssetDatabase.Refresh();
    }
    
    public World GetWorldByName(string worldName)
    {

        return new World();
    }
    
    #endregion
    
    #region World Parsing
    
    /// <summary>
    /// Create the world with passed name, then return it so RootControl can load it into root windows
    /// </summary>
    /// <param name="worldName"></param>
    /// <returns></returns>
    public World CreateWorldFromName(string worldName)
    {
        DBReader dbReader = RootControl.DBReader;
        World world = new World
        {
            Name = worldName,
            Characters = dbReader.GetAllElementsOfType<Character>(Element.Category.Character),
            Forces = dbReader.GetAllElementsOfType<Force>(Element.Category.Force),
            Events = dbReader.GetAllElementsOfType<Event>(Element.Category.Event),
            Relations = dbReader.GetAllElementsOfType<Relation>(Element.Category.Relation),
            Collectives = dbReader.GetAllElementsOfType<Collective>(Element.Category.Collective),
            Concepts = dbReader.GetAllElementsOfType<Construct>(Element.Category.Construct),
            Creatures = dbReader.GetAllElementsOfType<Creature>(Element.Category.Creature),
            Locations = dbReader.GetAllElementsOfType<Location>(Element.Category.Location),
            Objects = dbReader.GetAllElementsOfType<Object>(Element.Category.Object),
            Institutions = dbReader.GetAllElementsOfType<Institution>(Element.Category.Institution),
            Territorys = dbReader.GetAllElementsOfType<Territory>(Element.Category.Territory),
            Titles = dbReader.GetAllElementsOfType<Title>(Element.Category.Title),
            Races = dbReader.GetAllElementsOfType<Species>(Element.Category.Species),
            Familys = dbReader.GetAllElementsOfType<Family>(Element.Category.Family),
            Traits = dbReader.GetAllElementsOfType<Family>(Element.Category.Trait),
            Laws = dbReader.GetAllElementsOfType<Law>(Element.Category.Law),
            Languages = dbReader.GetAllElementsOfType<Language>(Element.Category.Language),
            Abilitys = dbReader.GetAllElementsOfType<Ability>(Element.Category.Ability),
            Maps = dbReader.GetAllMaps(),
            Pins = dbReader.GetAllPins(),
            TypesLocation = dbReader.GetTableTyping(Element.Category.Location.ToString()),
            TypesCharacter = dbReader.GetTableTyping(Element.Category.Character.ToString()),
          //  CreatureTypes = dbReader.GetTableTyping("Creature"),
          //  CreatureTypes = dbReader.GetTableTyping("Creature"),
        };
        Debug.Log("Parsed world: " + worldName + " with " + world.Pins.Count + " Pins");
        Debug.Log("TypesCharacter has subtypescount: " +  world.TypesCharacter[0].Subtypes.Count);
   
        return world;
    }
    private DBWriter dbWriter; 
       public void StoreWorld(World world)
       {
           Debug.Log("Writing " + world + " to file");
             dbWriter = RootControl.DBWriter;

          // Create a backup 
           dbWriter.CopyActiveToTempTable();
           List<Element.Category> typeTables = new List<Element.Category>(); 
        // Iterate over all properties
        foreach (var prop in world.GetType().GetProperties())
        { 
            // Maps and Pins are not Properties so not handled here
            
            var propName = prop.Name;

            if (propName.StartsWith("Types")) // Typing table
            { 
                Element.Category category = (Element.Category)Enum.Parse(typeof(Element.Category), propName.Substring(5, propName.Length - 5));
                typeTables.Add( category); 
            }
            else // Standard table
            {
                if (propName.EndsWith("s"))
                    propName = propName.Substring(0, propName.Length - 1);

                // Get the list of elements from the property
                var elements = (IList)prop.GetValue(world); 
 
                Element.Category categoryCategory;
                Enum.TryParse<Element.Category>(propName, out categoryCategory); 
  
                dbWriter.FlushTable(categoryCategory);

                foreach (Element element in elements)
                    dbWriter.WriteElement(element);
            }
           
        }
        
        foreach (var typeTable in typeTables)
        {
           dbWriter.FlushTable( typeTable+"Typing");
            dbWriter.WriteTypingTable(typeTable ); 
        }
        dbWriter.FlushTable("Map");
        dbWriter.FlushTable("Pin");
        foreach (Map map in world.Maps)
            dbWriter.WriteNonElement(map, "Map");
        foreach (Pin pin in world.Pins)
            dbWriter.WriteNonElement(pin, "Pin"); 
        
    }
       
       /*public void StoreElement(Element element)
       {
       //    Debug.Log("Storing element: " + element.ID);
           DBWriter dbWriter = RootControl.DBWriter;
           dbWriter.OverwriteElement(element);
       }*/
    #endregion
    
 
    
    public void GetWorldsFromUser()
    {
        //todo-big build auth and sync system
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

public class TableTyping
{
    public string Supertype { get; private set; }
    public string TypeCustom { get; set; }
    public List<string> Subtypes { get; private set; }  
    public List<string> SubtypeCustoms { get; set; }

    public TableTyping( )
    {
       
    }
    public TableTyping(string supertypeName)
    {
        Supertype = supertypeName;
        Subtypes = new List<string>();
        SubtypeCustoms = new List<string>();
    }

    public void SetSupertype(string str)
    {
        Supertype = str;
    }
    public void SetSubtype(List<string> strs)
    { 
        Subtypes = strs;
    }
    public string GetOriginalSupertype()
    { 
        return Supertype;
    }
    public string GetPotentialCustomSupertype()
    {
        if (string.IsNullOrEmpty(TypeCustom) == false)
            return TypeCustom;
        return Supertype;
    }  
    public string GetPotentialCustomSubtype(string originalSubtypeName)
    {
        int index = 99;
        foreach (var subtype in Subtypes)
            if (subtype == originalSubtypeName)
            {
                index = Subtypes.IndexOf(subtype);
            }

        if (index == 99)
        {
            //  Debug.LogWarning("!Did not find GetPotentialCustomSubtype for supertype " + Supertype + " argument: " + originalSubtypeName );
            return "";
        }
        if (SubtypeCustoms[index] != "")
            return SubtypeCustoms[index];
        return Subtypes[index]; 
    }
    public List<string> GetOriginalSubtypes()
    {
        return Subtypes;
    }
    public List<string> GetPotentiallyCustomSubtypes()
    {
        List<string> returnList = new List<string>();
        foreach (var subtype in Subtypes)
        {
            if ( string.IsNullOrEmpty( SubtypeCustoms[Subtypes.IndexOf(subtype)]) == false)
                returnList.Add(SubtypeCustoms[Subtypes.IndexOf(subtype)]);
            else
                returnList.Add(subtype);
        }
        return returnList;
    }
}