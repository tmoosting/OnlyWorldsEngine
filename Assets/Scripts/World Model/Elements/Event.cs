


// --> has a Timestate, and one or more Elements, each with a list of properties and changes thereon (either only new value, or both old and new)

namespace World_Model.Elements
{
    public class Event  : Element
    {

        public Event()
        { 
            this.table = Element.Table.Event;
        }


    }
}
