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
          foreach (var tableTyping in GetTypingTablesForElementTable(element.table))
              if (givenSupertype == tableTyping.GetPotentialCustomSupertype())
                      return tableTyping.GetOriginalSupertype();
          Debug.LogWarning("! Did not find Original Supertype for element: " + element.Name + " and supertype: " + givenSupertype);
          return "";
      } 
      
      public string GetOriginalSubtypeForPotentialCustomSubtype(Element element, string givenSubtype)
      {
          foreach (var tableTyping in GetTypingTablesForElementTable(element.table))
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
          foreach (var tableTyping in  GetTypingTablesForElementTable(element.table))
              if (element.Supertype == tableTyping.Supertype)
                  return tableTyping.GetPotentialCustomSupertype();
          Debug.LogWarning("! Did not find Effective Supertype for element: " + element.Name + " With supertype: " + element.Supertype);
          return "";
      }    
      public string GetPotentialCustomSubtypeForElement(Element element)
      {
          foreach (var tableTyping in  GetTypingTablesForElementTable(element.table))
              if (element.Supertype == tableTyping.Supertype)
                  return tableTyping.GetPotentialCustomSubtype(element.Subtype);
          Debug.LogWarning("! Did not find Effective Supertype for element: " + element.Name + " With supertype: " + element.Supertype);
          return "";
      }
      
      public List<TableTyping> GetTypingTablesForElementTable(Element.Table table)
      {
          if (table == Element.Table.Location)
              return RootControl.World.TypesLocation;
          //todo add rest 

        Debug.LogWarning("! Failed to find Typing Table for: " + table);
          return null;
      }

      public TableTyping GetTypingTableForElement(Element element)
      {
          List<TableTyping> potentialTypingTables = GetTypingTablesForElementTable(element.table);
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
            Characters = dbReader.GetAllElementsOfType<Character>(Element.Table.Character),
            Forces = dbReader.GetAllElementsOfType<Force>(Element.Table.Force),
            Events = dbReader.GetAllElementsOfType<Event>(Element.Table.Event),
            Relations = dbReader.GetAllElementsOfType<Relation>(Element.Table.Relation),
            Collectives = dbReader.GetAllElementsOfType<Collective>(Element.Table.Collective),
            Concepts = dbReader.GetAllElementsOfType<Concept>(Element.Table.Concept),
            Creatures = dbReader.GetAllElementsOfType<Creature>(Element.Table.Creature),
            Locations = dbReader.GetAllElementsOfType<Location>(Element.Table.Location),
            Objects = dbReader.GetAllElementsOfType<Object>(Element.Table.Object),
            Institutions = dbReader.GetAllElementsOfType<Institution>(Element.Table.Institution),
            Territorys = dbReader.GetAllElementsOfType<Territory>(Element.Table.Territory),
            Titles = dbReader.GetAllElementsOfType<Title>(Element.Table.Title),
            Races = dbReader.GetAllElementsOfType<Race>(Element.Table.Race),
            Familys = dbReader.GetAllElementsOfType<Family>(Element.Table.Family),
            Traits = dbReader.GetAllElementsOfType<Family>(Element.Table.Trait),
            Laws = dbReader.GetAllElementsOfType<Law>(Element.Table.Law),
            Languages = dbReader.GetAllElementsOfType<Language>(Element.Table.Language),
            Abilitys = dbReader.GetAllElementsOfType<Ability>(Element.Table.Ability),
            Maps = dbReader.GetAllMaps(),
            Pins = dbReader.GetAllPins(),
            TypesLocation = dbReader.GetTableTyping("Location"),
          //  CreatureTypes = dbReader.GetTableTyping("Creature"),
        };
        Debug.Log("Parsed world: " + worldName + " with " + world.Pins.Count + " Pins");
        Debug.Log("TypesLocation has subtypescount: " +  world.TypesLocation[0].Subtypes.Count);
   
        return world;
    }
    private DBWriter dbWriter; 
       public void StoreWorld(World world)
       {
           Debug.Log("Writing " + world + " to file");
             dbWriter = RootControl.DBWriter;

          // Create a backup 
           dbWriter.CopyActiveToTempTable();
           List<Element.Table> typeTables = new List<Element.Table>(); 
        // Iterate over all properties
        foreach (var prop in world.GetType().GetProperties())
        { 
            // Maps and Pins are not Properties so not handled here
            
            var propName = prop.Name;

            if (propName.StartsWith("Types")) // Typing table
            { 
                Element.Table table = (Element.Table)Enum.Parse(typeof(Element.Table), propName.Substring(5, propName.Length - 5));
                typeTables.Add( table); 
            }
            else // Standard table
            {
                if (propName.EndsWith("s"))
                    propName = propName.Substring(0, propName.Length - 1);

                // Get the list of elements from the property
                var elements = (IList)prop.GetValue(world); 
 
                Element.Table tableTable;
                Enum.TryParse<Element.Table>(propName, out tableTable); 
  
                dbWriter.FlushTable(tableTable);

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
