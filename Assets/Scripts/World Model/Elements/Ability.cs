using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : Element
{
    
    public int IsMagic { get; set; }
    
    public Ability()
    {
        this.table = Element.Table.Ability;
    }
}
