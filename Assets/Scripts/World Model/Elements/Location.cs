using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location  : Element
{

    public string Map { get; set; } // use name not ID
    

    public Location()
    {
        this.table = Element.Table.Location;
    }
   
}
