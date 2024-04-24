using Newtonsoft.Json;

namespace World_Model.Elements
{
  public class Character : Element
  {

       [JsonProperty("age")] public int age;
    [JsonProperty("species")] public string species;
    [JsonProperty("traits")] public string traits;
    [JsonProperty("charisma")] public int charisma;
    [JsonProperty("coercion")] public int coercion;
    [JsonProperty("gender")] public string gender;
    [JsonProperty("height")] public int height;
    [JsonProperty("weight")] public int weight;
    [JsonProperty("physicality")] public string physicality;
    [JsonProperty("languages")] public string languages;
    [JsonProperty("abilities")] public string abilities;
    [JsonProperty("birthplace")] public string birthplace;
    [JsonProperty("appearance")] public string appearance;
    [JsonProperty("background")] public string background;
    [JsonProperty("motivations")] public string motivations;
    [JsonProperty("status")] public string status;
    [JsonProperty("capability")] public int capability;
    [JsonProperty("compassion")] public int compassion;
    [JsonProperty("creativity")] public int creativity;
    [JsonProperty("courage")] public int courage;
    [JsonProperty("corruption")] public int corruption;
    [JsonProperty("location")] public string location;
    [JsonProperty("titles")] public string titles;
    [JsonProperty("objects")] public string objects;
    [JsonProperty("relations")] public string relations;
    [JsonProperty("institutions")] public string institutions;
    [JsonProperty("events")] public string events;
    [JsonProperty("family")] public string family;
    [JsonProperty("friends")] public string friends;
    [JsonProperty("rivals")] public string rivals;
    [JsonProperty("hit_points")] public int hit_points;
    [JsonProperty("class")] public string classs;
    [JsonProperty("alignment")] public string alignment;
    [JsonProperty("inspirations")] public string inspirations;
    [JsonProperty("tt_str")] public int tt_str;
    [JsonProperty("tt_int")] public int tt_int;
    [JsonProperty("tt_con")] public int tt_con;
    [JsonProperty("tt_dex")] public int tt_dex;
    [JsonProperty("tt_wis")] public int tt_wis;
    [JsonProperty("tt_cha")] public int tt_cha;
    [JsonProperty("skill_stealth")] public int skill_stealth;
    [JsonProperty("equipment")] public string equipment;
    [JsonProperty("backpack")] public string backpack;
    [JsonProperty("proficiencies")] public string proficiencies;
    [JsonProperty("features")] public string features;
    [JsonProperty("spells")] public string spells;

    public Character()
    {
      this.category = Element.Category.Character;
    }

  }
}
