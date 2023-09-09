 using System;
using System.Collections.Generic;
 using UnityEngine.Serialization;

 // Base model that represents any part of a world

[Serializable]
public class Element
{
    [Flags]
    public enum Table
    { 
        Location = 1 << 0,
        Character = 1 << 1,
        Matter = 1 << 2,
        Creature = 1 << 3,
        Concept = 1 << 4,
        God = 1 << 5,
        Event = 1 << 6,
        Relation = 1 << 7,
        Collective = 1 << 8,
        Territory = 1 << 9,
        Title = 1 << 10,
        Institution = 1 << 11,
        Race = 1 <<12,
        Family = 1 <<13,
        Trait = 1 <<14,
        Law = 1 <<15,
        Language = 1 <<16,
        Ability = 1 <<17,
    }

    public Table table;
    public string ID { get; set; }
    public string Name { get; set; } 
    public string Supertype { get; set; }
    public string Subtype { get; set; }
    public string Description{ get; set; }
  
    
    public static readonly Dictionary<string, Type> tableTypes = new Dictionary<string, Type>
    {
        { "Location", typeof(Location) },
        { "Character", typeof(Character) },
        { "Matter", typeof(Matter) },
        { "Creature", typeof(Creature) },
        { "Concept", typeof(Concept) },
        { "God", typeof(God) },
        { "Event", typeof(Event) },
        { "Relation", typeof(Relation) },
        { "Collective", typeof(Collective) },
        { "Territory", typeof(Territory) },
        { "Title", typeof(Title) },
        { "Institution", typeof(Institution) },
        { "Race", typeof(Race) },
        { "Family", typeof(Family) },
        { "Trait", typeof(Trait) },
        { "Law", typeof(Law) },
        { "Language", typeof(Language) },
        { "Ability", typeof(Ability) }
    }; 
}