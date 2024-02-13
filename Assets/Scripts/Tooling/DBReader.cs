using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;
using World_Model;

[CreateAssetMenu(fileName = "DBReader", menuName = "ScriptableObjects/DBReader", order = 0)]
public class DBReader : ScriptableObject
{ 
    private IDbConnection _dbconn;
    private IDbCommand _dbcmd;
    private IDataReader _reader;
    private  string dbActivePath = "URI=file:" + Application.dataPath + "/DefaultWorld.db";


    public string GetElementMetaDescription(Element.Category category) // todo fill this
    { 
        switch (category)
        {
            case Element.Category.Character:
                return "Sentient being, probably humanoid";
            case Element.Category.Force:
                return "Does it matter if it's real";
            case Element.Category.Event:
                return "";
            case Element.Category.Relation:
                return "";
            case Element.Category.Collective:
                return "";
            case Element.Category.Construct:
                return "";
            case Element.Category.Creature:
                return "";
            case Element.Category.Location:
                return "";
            case Element.Category.Object:
                return "";
            case Element.Category.Institution:
                return "";
            case Element.Category.Territory:
                return "";
            case Element.Category.Title:
                return "";
            case Element.Category.Species:
                return "";
            case Element.Category.Family:
                return "";
            case Element.Category.Trait:
                return "";
            case Element.Category.Law:
                return "";
            case Element.Category.Language:
                return "";  
            case Element.Category.Ability:
                return "";
            default:
                Debug.LogError($"Unsupported table: {category}");
                return null;
        }
    }
    
    public void SetDatabasePath(string worldName)
    {
        dbActivePath = "URI=file:" + Application.dataPath + "/" + RootControl.MonoLoader.worldPath + worldName + ".db"; 
    }
    
    private void ExecuteQuery(string query)
    {
        using (_dbconn = new SqliteConnection(dbActivePath))
        {
            _dbconn.Open();
            using (_dbcmd = _dbconn.CreateCommand())
            {
                _dbcmd.CommandText = query;
                _dbcmd.ExecuteNonQuery();
            }
            _dbconn.Close();
        }
    }

    private string GetScalarValue(string query)
    {
        string returnString = "NotFound";
        using (_dbconn = new SqliteConnection(dbActivePath))
        {
            _dbconn.Open();
            using (_dbcmd = _dbconn.CreateCommand())
            {
                _dbcmd.CommandText = query;
                using (_reader = _dbcmd.ExecuteReader())
                {
                    while (_reader.Read())
                        returnString = _reader.GetValue(0).ToString();
                }
            }
            _dbconn.Close();

        }
        return returnString;
    }

    private List<string> GetList(string query)
    { 
        var returnList = new List<string>();
        using (_dbconn = new SqliteConnection(dbActivePath))
        {
            _dbconn.Open();
            using (_dbcmd = _dbconn.CreateCommand())
            {
                _dbcmd.CommandText = query;
                using (_reader = _dbcmd.ExecuteReader())
                {
                    while (_reader.Read())
                        returnList.Add(_reader.GetValue(0).ToString());
                }
            }
            _dbconn.Close();
        }
        return returnList;
    }

   
    public List<T> ConvertList<T>(List<Element> elements) where T : Element
    {
        List<T> convertedList = new List<T>();

        foreach (Element element in elements)
        {
            if (element is T tElement)
                convertedList.Add(tElement);
        }

        return convertedList;
    }

    public List<TableTyping> GetTableTyping(string tableName)
    {
        List<TableTyping> tableTypes = new List<TableTyping>();
        string query = $"SELECT * FROM {tableName}Typing";

        using (_dbconn = new SqliteConnection(dbActivePath))
        {
            _dbconn.Open();
            using (_dbcmd = _dbconn.CreateCommand())
            {
                _dbcmd.CommandText = query;
                using (_reader = _dbcmd.ExecuteReader())
                {
                    while (_reader.Read())
                    {
                        TableTyping tblType = new TableTyping
                        {
                      //      Supertype = _reader["Types"].ToString(),
                            TypeCustom = _reader["types_custom"].ToString(),
                            // Initialize Subtypes and SubtypeCustoms with a single empty string
                       //     Subtypes = new List<string> { "" },
                            SubtypeCustoms = new List<string> { "" }
                        };
                        tblType.SetSupertype(_reader["types"].ToString());
                        tblType.SetSubtype(new List<string> { "" });
                        
                        string subtypes = _reader["subtypes"].ToString();
                        if (!string.IsNullOrEmpty(subtypes))
                        {
                            tblType.SetSubtype( subtypes.Split(',').ToList());
                            //     tblType.Subtypes = subtypes.Split(',').ToList();
                        }

                        string subtypeCustoms = _reader["subtypes_custom"].ToString();
                        if (!string.IsNullOrEmpty(subtypeCustoms))
                        {
                            tblType.SubtypeCustoms = subtypeCustoms.Split(',').ToList();
                        }

                        // Ensure SubtypeCustoms has the same number of elements as Subtypes
                        while (tblType.SubtypeCustoms.Count < tblType.Subtypes.Count)
                        {
                            tblType.SubtypeCustoms.Add("");
                        }
                        
                        tableTypes.Add(tblType);
                    }
                }
            }
            _dbconn.Close();
        }
        return tableTypes;
    }


      

    public List<T> GetAllElementsOfType<T>(Element.Category category) where T : Element
    {
        List<Element> elements = new List<Element>();
        foreach (string id in GetIDValuesForTable(category))
        { 
            Dictionary<string, string> fieldNamesAndValues = new Dictionary<string, string>();
            foreach (string field in GetFieldNamesForTable(category))            
                fieldNamesAndValues.Add(field, GetEntryForTableAndFieldWithID(category, field, id));            
            elements.Add(AssembleElementOfTypeFromFields(category, fieldNamesAndValues));
        }
        return ConvertList<T>(elements);
    }
    public List<string> GetFieldNamesForTable(Element.Category elementCategory)
    {
        List<string> returnList = new List<string>();
        string query = "SELECT * FROM " + elementCategory;
        using (_dbconn = new SqliteConnection(dbActivePath))
        {
            _dbconn.Open();
            using (_dbcmd = _dbconn.CreateCommand())
            {
                _dbcmd.CommandText = query;
                using (_reader = _dbcmd.ExecuteReader())
                {
                    for (int i = 0; i < _reader.FieldCount; i++)
                        returnList.Add(_reader.GetName(i)); 
                }
            }
            _dbconn.Close();
        }
        return returnList;
    }

    public List<string> GetIDValuesForTable(Element.Category category)
    {
        Debug.Log("QUERY table " + category); 
        string query = "SELECT id FROM " + category;
        return GetList(query);
    }

    public string GetEntryForTableAndFieldWithID(Element.Category category, string fieldName, string id )
    { 
        string query = "SELECT " + fieldName + " FROM " + category + " WHERE id='" + id + "'";
        return GetScalarValue(query);
    }

  private Element AssembleElementOfTypeFromFields(Element.Category category, Dictionary<string, string> dict)
{
    // Convert the enum type to string
    string typeName = $"World_Model.Elements.{category.ToString()}";

    // Get the assembly that the class is in
    var assembly = Assembly.GetExecutingAssembly();

    var elementType = assembly.GetType(typeName);
     
    var element = Activator.CreateInstance(elementType) as Element;

    if (element != null)
    {
        foreach (var property in elementType.GetProperties())
        { 
            string dbName = property.Name.ToSnakeCase();

            if (dict.TryGetValue(dbName, out string value))
            {
                Type propertyType = property.PropertyType;
                object convertedValue = null;

                if (propertyType == typeof(int))
                {
                    convertedValue = int.TryParse(value, out int intValue) ? intValue : default(int?);
                }
                else if (propertyType == typeof(double))
                {
                    convertedValue = double.TryParse(value, out double doubleValue) ? doubleValue : default(double?);
                }
                else if (propertyType == typeof(bool) && Attribute.IsDefined(property, typeof(SQLiteBoolAttribute)))
                {
                    convertedValue = value == "1";
                }
                else if (propertyType == typeof(bool))
                {
                    convertedValue = bool.TryParse(value, out bool boolValue) ? boolValue : default(bool?);
                }
                else
                {
                    convertedValue = value; // Directly assign string and other types
                }

                if (convertedValue != null)
                {
                    property.SetValue(element, convertedValue);
                }
            }
        }
    }

    if (string.IsNullOrEmpty(element.Supertype))
        element.Supertype = "None";
    if (string.IsNullOrEmpty(element.Subtype))
        element.Subtype = "None";
    return element;
}

   
    public List<Map> GetAllMaps()
    {
        List<Map> returnList = new List<Map>();

        string query = "SELECT * FROM Map";

        using (_dbconn = new SqliteConnection(dbActivePath))
        {
            _dbconn.Open();
            using (_dbcmd = _dbconn.CreateCommand())
            {
                _dbcmd.CommandText = query;
                using (_reader = _dbcmd.ExecuteReader())
                {
                    while (_reader.Read())
                    {
                        Map map = new Map
                        {
                            ID = _reader["id"].ToString(),
                            Name = _reader["name"].ToString(),   
                            Supertype = _reader["supertype"].ToString(), 
                            Subtype = _reader["subtype"].ToString(), 
                            Description = _reader["description"].ToString(), 
                            FileName = _reader["file_name"].ToString(), 
                            BackgroundColor = _reader["background_color"].ToString(), 
                            ParentMap = _reader["parent_map"].ToString(), 
                            Hierarchy = Convert.ToInt32(_reader["hierarchy"]), 

                        };
                        if (Enum.TryParse(map.Supertype, out Map.Type mapType))
                            map.type = mapType;
                        returnList.Add(map);
                    }
                }
            }
            _dbconn.Close();
        }
        return returnList;
    }
    public List<Pin> GetAllPins()
    {
        List<Pin> returnList = new List<Pin>();

        string query = "SELECT * FROM Pin";

        using (_dbconn = new SqliteConnection(dbActivePath))
        {
            _dbconn.Open();
            using (_dbcmd = _dbconn.CreateCommand())
            {
                _dbcmd.CommandText = query;
                using (_reader = _dbcmd.ExecuteReader())
                {
                    while (_reader.Read())
                    {
                        Pin pin = new Pin
                        {
                            ID = _reader["id"].ToString(),
                            Name = _reader["name"].ToString(), 
                            Description = _reader["description"].ToString(), 
                            Type = _reader["type"].ToString(), 
                            Map = _reader["map"].ToString(), 
                            PinnedMap = _reader["pinned_map"].ToString(), 
                            Element = _reader["element"].ToString(), 
                            CoordX = Convert.ToSingle(_reader["coord_x"]),
                            CoordY = Convert.ToSingle(_reader["coord_y"]),
                            CoordZ = Convert.ToSingle(_reader["coord_z"]),
                            Zoomscale = Convert.ToInt32(_reader["zoom_scale"]),
                            ToggleBase = ReadSQLiteBool(SafeGetValue(_reader, "toggle_base"), true), 
                            ToggleColor = ReadSQLiteBool(SafeGetValue(_reader, "toggle_color"), true), 
                            ToggleIcon = ReadSQLiteBool(SafeGetValue(_reader, "toggle_icon"), true), 
                            ToggleName = ReadSQLiteBool(SafeGetValue(_reader, "toggle_name"), true),  
                        };
                        pin.category = (Pin.Category)Enum.Parse(typeof(Pin.Category), pin.Type);
                        returnList.Add(pin);
                    }
                }
            }
            _dbconn.Close();
        }
        return returnList;
    }
    private object SafeGetValue(IDataReader reader, string columnName)
    {
        int index = reader.GetOrdinal(columnName);
        if (index == -1) return null;
        return reader.GetValue(index);
    }
    private bool ReadSQLiteBool(object sqliteValue, bool defaultValue)
    { 
        if (sqliteValue is DBNull || string.IsNullOrEmpty(sqliteValue.ToString()))
        {
            return defaultValue;
        }
    
        return Convert.ToInt32(sqliteValue) == 1;
    }
    public List<string> GetTableNames()
    {
        string query = "SELECT name FROM sqlite_master WHERE type = 'table'";
        return GetList(query);
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
        RootControl rootControl =  AssetDatabase.LoadAssetAtPath<RootControl>("Assets/Resources/Root Files/RootControl.asset");
        if (rootControl == null)
            Debug.LogWarning("! No RootControl found. Please re-load the tool from Launcher.");
        return rootControl;
    }
}
