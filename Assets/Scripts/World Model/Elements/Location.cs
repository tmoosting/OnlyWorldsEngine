namespace World_Model.Elements
{
    public class Location  : Element
    {

        public string Map { get; set; } // use name not ID
    

        public Location()
        {
            this.category = Element.Category.Location;
        }
   
    }
}
