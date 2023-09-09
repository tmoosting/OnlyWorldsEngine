using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Element
{

  public int Age { get; set; }
  public int Charisma { get; set; }
  public string Traits { get; set; }

  public Character()
  {
    this.table = Element.Table.Character;
  }

}
