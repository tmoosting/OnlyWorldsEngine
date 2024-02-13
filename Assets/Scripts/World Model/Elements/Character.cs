namespace World_Model.Elements
{
  public class Character : Element
  {

    public int Age { get; set; }
    public int Charisma { get; set; }
    public string Traits { get; set; }

    public Character()
    {
      this.category = Element.Category.Character;
    }

  }
}
