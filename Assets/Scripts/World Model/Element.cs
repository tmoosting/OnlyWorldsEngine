using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using World_Model.Elements;
using Object = World_Model.Elements.Object;

// Base model that represents any part of a world
 

namespace World_Model
{
    [Serializable]
    public class Element
    {
        [Flags]
        public enum Category
        { 
            Location = 1 << 0,
            Character = 1 << 1,
            Object = 1 << 2,
            Creature = 1 << 3,
            Construct = 1 << 4,
            Phenomenon = 1 << 5,
            Event = 1 << 6,
            Relation = 1 << 7,
            Collective = 1 << 8,
            Territory = 1 << 9,
            Title = 1 << 10,
            Institution = 1 << 11,
            Species = 1 <<12,
            Family = 1 <<13,
            Trait = 1 <<14,
            Law = 1 <<15,
            Language = 1 <<16,
            Ability = 1 <<17,
        }

        public Category category; 
        public string ID { get; set; } 
        public string Name { get; set; }  
        public string Supertype { get; set; } 
        public string Subtype { get; set; } 
        public string Description{ get; set; } 
        public int CreationTimeStamp { get; set; }
    
        public static readonly Dictionary<string, Type> tableTypes = new Dictionary<string, Type>
        {
            { "Location", typeof(Location) },
            { "Character", typeof(Character) },
            { "Object", typeof(Object) },
            { "Creature", typeof(Creature) },
            { "Concept", typeof(Construct) },
            { "Phenomenon", typeof(Phenomenon) },
            { "Event", typeof(Event) },
            { "Relation", typeof(Relation) },
            { "Collective", typeof(Collective) },
            { "Territory", typeof(Territory) },
            { "Title", typeof(Title) },
            { "Institution", typeof(Institution) },
            { "Race", typeof(Species) },
            { "Family", typeof(Family) },
            { "Trait", typeof(Trait) },
            { "Law", typeof(Law) },
            { "Language", typeof(Language) },
            { "Ability", typeof(Ability) }
        }; 
    }
}