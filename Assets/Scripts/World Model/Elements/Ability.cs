namespace World_Model.Elements
{
    public class Ability : Element
    {
    
        public int IsMagic { get; set; }
    
        public Ability()
        {
            this.category = Element.Category.Ability;
        }
    }
}
