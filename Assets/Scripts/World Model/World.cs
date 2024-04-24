using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using World_Model.Elements;
using Event = World_Model.Elements.Event;
using Object = World_Model.Elements.Object;

// Unity data object that represents a loaded .db world database file

// todo-refactor Consider breaking it up or encapsulating its behavior further. For instance, database-related operations could be separated from the data definition.

public class World
{
    [JsonProperty("Character")]
    public List<Character> Characters { get; set; }

    [JsonProperty("Location")]
    public List<Location> Locations { get; set; }
    
    
    
    public string ID = "DefaultID";

    public string Name;
    
    
    public List<Character> CharacterList { get; set; }
    public List<Phenomenon> PhenomenonList { get; set; }
    public List<Event> EventList { get; set; }
    public List<Relation> RelationList { get; set; }
    public List<Collective> CollectiveList { get; set; }
    public List<Construct> ConstructList { get; set; }
    public List<Creature> CreatureList { get; set; }
    public List<Location> LocationList { get; set; }
    public List<Object> ObjectList { get; set; }
    public List<Institution> InstitutionList { get; set; }
    public List<Territory> TerritoryList { get; set; }
    public List<Title> TitleList { get; set; }
    public List<Species> SpeciesList { get; set; }
    public List<Family> FamilyList { get; set; }
    public List<Family> TraitList { get; set; }
    public List<Law> LawList { get; set; }
    public List<Language> LanguageList { get; set; }
    public List<Ability> AbilityList { get; set; }

    public List<Map> Maps = new List<Map>();
    public List<Pin> Pins = new List<Pin>();

    public List<TableTyping> TypesLocation { get; set; }    // eg Settlement, Productive, Defensive etc. 
    public List<TableTyping> TypesCharacter{ get; set; }   
    public List<TableTyping> TypesCreature;
        
    public World()
    {
        
    }

}
