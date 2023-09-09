using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin
{

    public enum Category
    {
        Element, // represents a world Element
        Descriptive, // not connected to an element - aesthetic or descriptive placement
        Map // access to an embedded map 
    }

    public string ID{ get; set; }
    public string Name{ get; set; }
    public string Type{ get; set; }
    public string Map{ get; set; } 
    public string Element{ get; set; }  // value for Element Pin
    public string PinnedMap{ get; set; }// value for Map Pin , stored by Name not ID
    public string Description{ get; set; } // value for Descriptive Pin
    public float CoordX{ get; set; }
    public float CoordY{ get; set; }
    public float CoordZ{ get; set; }
    public float Zoomscale{ get; set; }
    
    [SQLiteBool] public bool ToggleBase { get; set; }
    [SQLiteBool] public bool ToggleColor  { get; set; }
    [SQLiteBool] public bool ToggleIcon  { get; set; }
    [SQLiteBool] public bool ToggleName  { get; set; }
    
    public Category category;
    

    public Pin()
    {
        
    }
     
 

}
