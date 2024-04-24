using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using World_Model;
using World_Model.Elements;
using Event = World_Model.Elements.Event;
using Object = World_Model.Elements.Object;

[CreateAssetMenu(fileName = "RootEdit", menuName = "ScriptableObjects/RootEdit", order = 0)]

public class RootEdit : ScriptableObject
{



    [Header("Settings")] 
    public float entryDelay = 0.2f;
    [Header("Colors")] 
    public Color backgroundColor;


    public void MakeTableTypingChange(TableTyping tableTyping, string propertyName, string value, int index = -1)
    { 
        
       Debug.Log(tableTyping.Supertype + " has propertyName: " + propertyName + " getting value: " + value + " index: " + index);

 
        if (propertyName == "SubtypeCustoms" && index >= 0)
        {
            tableTyping.SubtypeCustoms[index] = value;
        }
        else
        {
            PropertyInfo propertyInfo = tableTyping.GetType().GetProperty(propertyName);
            propertyInfo.SetValue(tableTyping, value);
        } 
        

        RootControl.RegisterTableTypingValueChange();
    }


    public void MakeDirectActiveElementChange(string propertyName, string value)
    {
        PropertyInfo propertyInfo = RootControl.Element.GetType().GetProperty(propertyName);
    
        if (propertyInfo == null)
        {
            Debug.LogError($"No property named '{propertyName}' found in Element class.");
            return;
        }
    
        if (propertyInfo.PropertyType != typeof(string))
        {
            Debug.LogError($"The property '{propertyName}' is not of type string.");
            return;
        }

        propertyInfo.SetValue(RootControl.Element, value);

        RootControl.RegisterElementValueChange();
    }

     public void MakeElementChangeFromEditField(EditField editField, Element element)
    {  
        string propertyToChange = editField.baseString;
        string newValue = editField.fieldValue;
        PropertyInfo property = element.GetType().GetProperty(propertyToChange);
        
        if (property != null && property.CanWrite)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                if (property.PropertyType.IsValueType)
                {
                    Debug.LogError($"Cannot set property {propertyToChange} to null since it's a value type.");
                    return;
                }
                property.SetValue(element, null, null);
            }
            else
            {
                if (property.PropertyType == typeof(int))
                {
                    if (int.TryParse(newValue, out int intResult))
                    {
                        property.SetValue(element, intResult, null);
                    }
                    else
                    {
                        Debug.LogError($"Failed to set property {propertyToChange} because the value '{newValue}' could not be parsed as an integer.");
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(element, newValue, null);
                }
                else    if (property.PropertyType == typeof(float))
                {
                    if (float.TryParse(newValue, out float result))
                    {
                        property.SetValue(element, result, null);
                    }
                    else
                    {
                        Debug.LogError($"Failed to set property {propertyToChange} because the value '{newValue}' could not be parsed as a float.");
                    }
                }
                //   handle other types if needed 
            }
        }
        else
        {
            Debug.LogError($"Property {propertyToChange} not found or not writable on element type {element.GetType().Name}.");
        }
        RootControl.RegisterElementValueChange(false);
    }
    
    
    
    public Element CreateElement(Element.Category elementCategory, string elementName)
    {
        Element returnElement;

        switch (elementCategory)
        {
            case Element.Category.Location:
                returnElement = CreateLocation(elementName);
                break;
            case Element.Category.Character:
                returnElement = CreateCharacter(elementName);
                break;
            case Element.Category.Object:
                returnElement = CreateObject(elementName);
                break;
            case Element.Category.Creature:
                returnElement = CreateCreature(elementName);
                break;
            case Element.Category.Construct:
                returnElement = CreateConcept(elementName);
                break;
            case Element.Category.Phenomenon:
                returnElement = CreatePhenomenon(elementName);
                break;
            case Element.Category.Event:
                returnElement = CreateEvent(elementName);
                break;
            case Element.Category.Relation:
                returnElement = CreateRelation(elementName);
                break;
            case Element.Category.Collective:
                returnElement = CreateCollective(elementName);
                break;
            case Element.Category.Territory:
                returnElement = CreateTerritory(elementName);
                break;
            case Element.Category.Title:
                returnElement = CreateTitle(elementName);
                break;
            case Element.Category.Institution:
                returnElement = CreateInstitution(elementName);
                break;
            case Element.Category.Species:
                returnElement = CreateRace(elementName);
                break;
            case Element.Category.Family:
                returnElement = CreateFamily(elementName);
                break;
            case Element.Category.Ability:
                returnElement = CreateAbility(elementName);
                break;
            default:
                throw new ArgumentException($"Unsupported element table: {elementCategory}");
        }
        returnElement.ID = Guid.NewGuid().ToString();
        returnElement.Name = elementName;  
        returnElement.CreationTimeStamp = (int) ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
        return returnElement;
    }

 
    private Location CreateLocation(string elementName)
    {
        Location location = new Location
        {
            Supertype = RootControl.World.TypesLocation[0].GetOriginalSupertype(),
            Subtype = RootControl.World.TypesLocation[0].GetOriginalSubtypes()[0],
            Map = RootControl.Map.Name
        }; 
        RootControl.World.LocationList.Add(location);
        return location;
    }

    private Character CreateCharacter(string elementName)
    {
        Character character = new Character()
        {
            Supertype = RootControl.World.TypesCharacter[0].GetOriginalSupertype(),
            Subtype = RootControl.World.TypesCharacter[0].GetOriginalSubtypes()[0], 
        }; 
  
        RootControl.World.CharacterList.Add(character);
        return character;
    }

    private Object CreateObject(string elementName)
    {
        Object obj = new Object(); 
        RootControl.World.ObjectList.Add(obj);
        return obj;
    }
    
    private Creature CreateCreature(string elementName)
{
    Creature creature = new Creature(); 
    RootControl.World.CreatureList.Add(creature);
    return creature;
}

private Construct CreateConcept(string elementName)
{
    Construct construct = new Construct(); 
    RootControl.World.ConstructList.Add(construct);
    return construct;
}

private Phenomenon CreatePhenomenon(string elementName)
{
    Phenomenon phenomenon = new Phenomenon(); 
    RootControl.World.PhenomenonList.Add(phenomenon);
    return phenomenon;
}

private Event CreateEvent(string elementName)
{
    Event newEvent = new Event(); 
    RootControl.World.EventList.Add(newEvent);
    return newEvent;
}

private Relation CreateRelation(string elementName)
{
    Relation relation = new Relation(); 
    RootControl.World.RelationList.Add(relation);
    return relation;
}

private Collective CreateCollective(string elementName)
{
    Collective collective = new Collective(); 
    RootControl.World.CollectiveList.Add(collective);
    return collective;
}

private Territory CreateTerritory(string elementName)
{
    Territory territory = new Territory(); 
    RootControl.World.TerritoryList.Add(territory);
    return territory;
}

private Title CreateTitle(string elementName)
{
    Title title = new Title(); 
    RootControl.World.TitleList.Add(title);
    return title;
}

private Institution CreateInstitution(string elementName)
{
    Institution institution = new Institution(); 
    RootControl.World.InstitutionList.Add(institution);
    return institution;
}

private Species CreateRace(string elementName)
{
    Species species = new Species(); 
    RootControl.World.SpeciesList.Add(species);
    return species;
}

private Family CreateFamily(string elementName)
{
    Family family = new Family(); 
    RootControl.World.FamilyList.Add(family);
    return family;
}
    
private Ability CreateAbility(string elementName)
{
    Ability ability = new Ability(); 
    RootControl.World.AbilityList.Add(ability);
    return ability;
}
    
    
    
       public Dictionary<string, string> GetPropertiesAndValues(Element element)
    {
        Dictionary<string, string> returnDict = new Dictionary<string, string>();
      

        // First, fetch properties from the base Element class
        foreach (PropertyInfo property in typeof(Element).GetProperties())
        {
            if (property.CanRead && property.GetMethod.IsPublic && property.Name != "ID")
            {
                object value = property.GetValue(element);
                returnDict[property.Name] = value?.ToString() ?? "";
            }
        }

        // Then, fetch properties specific to the derived type
        foreach (PropertyInfo property in element.GetType().GetProperties())
        {
            // To avoid adding properties from the Element class again, check if it's already in the dictionary
            if (returnDict.ContainsKey(property.Name))
                continue;

            if (property.CanRead && property.GetMethod.IsPublic && property.Name != "ID")
            {
                object value = property.GetValue(element);
                returnDict[property.Name] = value?.ToString() ?? "";
            }
        }

        return returnDict;
    }
    public void DeleteElement(Element rootControlElement)
    {
         
    }

    public bool IsElementChangeAllowed(EditField editField, Element element)
    {
        bool returnBool = true;
        string propertyToChange = editField.baseString;
        string newValue = editField.fieldValue;

        PropertyInfo property = element.GetType().GetProperty(propertyToChange);
        if (property != null && property.CanWrite)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                if (property.PropertyType.IsValueType)
                {
                    return false;
                } 
                return true;
            }
            else
            {
                if (property.PropertyType == typeof(int))
                {
                    if (int.TryParse(newValue, out int intResult))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    return true;
                }
                else    if (property.PropertyType == typeof(float))
                {
                    if (float.TryParse(newValue, out float result))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
            }
        }
        else
        {
           return false;
        }

        return returnBool;
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
