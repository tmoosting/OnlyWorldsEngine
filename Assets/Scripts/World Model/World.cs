using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Unity data object that represents a loaded .db world database file

// todo-refactor Consider breaking it up or encapsulating its behavior further. For instance, database-related operations could be separated from the data definition.

public class World
{
    public string ID = "DefaultID";

    public string Name;
    
    
    public List<Character> Characters { get; set; }
    public List<God> Gods { get; set; }
    public List<Event> Events { get; set; }
    public List<Relation> Relations { get; set; }
    public List<Collective> Collectives { get; set; }
    public List<Concept> Concepts { get; set; }
    public List<Creature> Creatures { get; set; }
    public List<Location> Locations { get; set; }
    public List<Matter> Matters { get; set; }
    public List<Institution> Institutions { get; set; }
    public List<Territory> Territorys { get; set; }
    public List<Title> Titles { get; set; }
    public List<Race> Races { get; set; }
    public List<Family> Familys { get; set; }
    public List<Family> Traits { get; set; }
    public List<Law> Laws { get; set; }
    public List<Language> Languages { get; set; }
    public List<Ability> Abilitys { get; set; }

    public List<Map> Maps = new List<Map>();
    public List<Pin> Pins = new List<Pin>();

    public List<TableTyping> TypesLocation { get; set; }    // eg Settlement, Productive, Defensive etc. 
    public List<TableTyping> TypesCreature;
        
    public World()
    {
        
    }

}

public class TableTyping
{
    public string Supertype { get; private set; }
    public string TypeCustom { get; set; }
    public List<string> Subtypes { get; private set; }  
    public List<string> SubtypeCustoms { get; set; }


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