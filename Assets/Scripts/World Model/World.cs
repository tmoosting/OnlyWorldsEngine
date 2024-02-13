using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using World_Model.Elements;
using Event = World_Model.Elements.Event;
using Object = World_Model.Elements.Object;

// Unity data object that represents a loaded .db world database file

// todo-refactor Consider breaking it up or encapsulating its behavior further. For instance, database-related operations could be separated from the data definition.

public class World
{
    public string ID = "DefaultID";

    public string Name;
    
    
    public List<Character> Characters { get; set; }
    public List<Force> Forces { get; set; }
    public List<Event> Events { get; set; }
    public List<Relation> Relations { get; set; }
    public List<Collective> Collectives { get; set; }
    public List<Construct> Concepts { get; set; }
    public List<Creature> Creatures { get; set; }
    public List<Location> Locations { get; set; }
    public List<Object> Objects { get; set; }
    public List<Institution> Institutions { get; set; }
    public List<Territory> Territorys { get; set; }
    public List<Title> Titles { get; set; }
    public List<Species> Races { get; set; }
    public List<Family> Familys { get; set; }
    public List<Family> Traits { get; set; }
    public List<Law> Laws { get; set; }
    public List<Language> Languages { get; set; }
    public List<Ability> Abilitys { get; set; }

    public List<Map> Maps = new List<Map>();
    public List<Pin> Pins = new List<Pin>();

    public List<TableTyping> TypesLocation { get; set; }    // eg Settlement, Productive, Defensive etc. 
    public List<TableTyping> TypesCharacter{ get; set; }   
    public List<TableTyping> TypesCreature;
        
    public World()
    {
        
    }

}
