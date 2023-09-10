using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DBReader", menuName = "ScriptableObjects/DBReader", order = 0)]
public class DBReader : ScriptableObject
{ 
    private IDbConnection _dbconn;
    private IDbCommand _dbcmd;
    private IDataReader _reader;
    private  string dbActivePath = "URI=file:" + Application.dataPath + "/DefaultWorld.db";


    public string GetElementMetaDescription(Element.Table table) // todo fill this
    { 
        switch (table)
        {
            case Element.Table.Character:
                return "Sentient being, probably humanoid";
            case Element.Table.God:
                return "Does it matter if it's real";
            case Element.Table.Event:
                return "";
            case Element.Table.Relation:
                return "";
            case Element.Table.Collective:
                return "";
            case Element.Table.Concept:
                return "";
            case Element.Table.Creature:
                return "";
            case Element.Table.Location:
                return "";
            case Element.Table.Matter:
                return "";
            case Element.Table.Institution:
                return "";
            case Element.Table.Territory:
                return "";
            case Element.Table.Title:
                return "";
            case Element.Table.Race:
                return "";
            case Element.Table.Family:
                return "";
            case Element.Table.Trait:
                return "";
            case Element.Table.Law:
                return "";
            case Element.Table.Language:
                return "";  
            case Element.Table.Ability:
                return "";
            default:
                Debug.LogError($"Unsupported table: {table}");
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
                            TypeCustom = _reader["TypesCustom"].ToString(),
                            // Initialize Subtypes and SubtypeCustoms with a single empty string
                       //     Subtypes = new List<string> { "" },
                            SubtypeCustoms = new List<string> { "" }
                        };
                        tblType.SetSupertype(_reader["Types"].ToString());
                        tblType.SetSubtype(new List<string> { "" });
                        
                        string subtypes = _reader["Subtypes"].ToString();
                        if (!string.IsNullOrEmpty(subtypes))
                        {
                            tblType.SetSubtype( subtypes.Split(',').ToList());
                            //     tblType.Subtypes = subtypes.Split(',').ToList();
                        }

                        string subtypeCustoms = _reader["SubtypesCustom"].ToString();
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


    
    
    // Uses the column-per-Type for subtypes that I never quite got proper working
    /*
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
                Dictionary<int, string> typesDict = new Dictionary<int, string>();
                Dictionary<int, string> customTypesDict = new Dictionary<int, string>();

                int typeIndex = 1;
                int customTypeIndex = 1;

                while (_reader.Read())
                {
                    string type = _reader["Types"].ToString();
                    if(!string.IsNullOrEmpty(type))
                    {
                        typesDict[typeIndex] = type;
                        typeIndex++;
                    }
                    
                    string customType = _reader["TypesCustom"].ToString();
                    if(!string.IsNullOrEmpty(customType))
                    {
                        customTypesDict[customTypeIndex] = customType;
                    }
                    else
                    {
                        customTypesDict[customTypeIndex] = null;
                    }
                    customTypeIndex++;
                }
                
               
                // Reset the reader
                _reader.Close();
                _dbcmd.CommandText = query;
                _reader = _dbcmd.ExecuteReader();

                foreach(var kvp in typesDict)
                {
                    TableTyping tblType = new TableTyping
                    {
                        Supertype = kvp.Value, 
                        Subtypes = new List<string>(),
                        SubtypeCustoms = new List<string>()
                    };

                    if (customTypesDict[kvp.Key] != null)
                        tblType.TypeCustom = customTypesDict[kvp.Key];
                    while (_reader.Read())
                    {
                     

                        var subtypeValue = _reader[$"Subtypes{kvp.Key}"]?.ToString();
                        if (!string.IsNullOrEmpty(subtypeValue))
                            tblType.Subtypes.Add(subtypeValue);

                        var subtypeCustomValue = _reader[$"SubtypesCustom{kvp.Key}"]?.ToString();
                     //   if (!string.IsNullOrEmpty(subtypeCustomValue))
                            tblType.SubtypeCustoms.Add(subtypeCustomValue);
                    }
                    // Reset the reader for the next type
                    _reader.Close();
                    _dbcmd.CommandText = query;
                    _reader = _dbcmd.ExecuteReader();

                    tableTypes.Add(tblType);
                }
            }
        }
        _dbconn.Close();
    }

    return tableTypes;
}
*/



 

    public List<T> GetAllElementsOfType<T>(Element.Table table) where T : Element
    {
        List<Element> elements = new List<Element>();
        foreach (string id in GetIDValuesForTable(table))
        { 
            Dictionary<string, string> fieldNamesAndValues = new Dictionary<string, string>();
            foreach (string field in GetFieldNamesForTable(table))            
                fieldNamesAndValues.Add(field, GetEntryForTableAndFieldWithID(table, field, id));            
            elements.Add(AssembleElementOfTypeFromFields(table, fieldNamesAndValues));
        }
        return ConvertList<T>(elements);
    }
    public List<string> GetFieldNamesForTable(Element.Table elementTable)
    {
        List<string> returnList = new List<string>();
        string query = "SELECT * FROM " + elementTable;
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

    public List<string> GetIDValuesForTable(Element.Table table)
    {
        string query = "SELECT ID FROM " + table;
        return GetList(query);
    }

    public string GetEntryForTableAndFieldWithID(Element.Table table, string fieldName, string ID )
    { 
        string query = "SELECT " + fieldName + " FROM " + table + " WHERE ID='" + ID + "'";
        return GetScalarValue(query);
    }

    private Element AssembleElementOfTypeFromFields(Element.Table table, Dictionary<string, string> dict)
    {
        // Convert the enum type to string
        string typeName = table.ToString();

        // Get the assembly that the class is in
        var assembly = Assembly.GetExecutingAssembly();

        // Get the Type object from the assembly
        var elementType = assembly.GetType(typeName);

        // Instantiate the type
        var element = Activator.CreateInstance(elementType) as Element;

        if (element != null)
        {
            foreach (KeyValuePair<string, string> field in dict)
            {
                PropertyInfo property = elementType.GetProperty(field.Key);
                if (property != null && property.CanWrite)
                {
                    Type propertyType = property.PropertyType;
                    object value = null;
                    // Handle conversion for different types
                    if (propertyType == typeof(int))
                    {
                        int intValue;
                        if (int.TryParse(field.Value, out intValue))
                            value = intValue;
                    }
                    else if (propertyType == typeof(double))
                    {
                        double doubleValue;
                        if (double.TryParse(field.Value, out doubleValue))
                            value = doubleValue;
                        
                    }
                    else if (propertyType == typeof(bool) && Attribute.IsDefined(property, typeof(SQLiteBoolAttribute)))
                    {
                        value = field.Value == "1";
                    }
                    else if (propertyType == typeof(bool))
                    {
                        bool boolValue;
                        if (bool.TryParse(field.Value, out boolValue))
                            value = boolValue;
                    }
                    
                    else
                        value = field.Value; 
                    
                    if (value != null)
                        property.SetValue(element, value);
                }
            }
        }
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
                            ID = _reader["ID"].ToString(),
                            Name = _reader["Name"].ToString(), 
                            TypeString = _reader["TypeString"].ToString(), 
                            Subtype = _reader["Subtype"].ToString(), 
                            Description = _reader["Description"].ToString(), 
                            FileName = _reader["FileName"].ToString(), 
                            BackgroundColor = _reader["BackgroundColor"].ToString(), 
                            ParentMap = _reader["ParentMap"].ToString(), 
                            Hierarchy = Convert.ToInt32(_reader["Hierarchy"]), 

                        };
                        if (Enum.TryParse(map.TypeString, out Map.Type mapType))
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
                            ID = _reader["ID"].ToString(),
                            Name = _reader["Name"].ToString(), 
                            Description = _reader["Description"].ToString(), 
                            Type = _reader["Type"].ToString(), 
                            Map = _reader["Map"].ToString(), 
                            PinnedMap = _reader["PinnedMap"].ToString(), 
                            Element = _reader["Element"].ToString(), 
                            CoordX = Convert.ToSingle(_reader["CoordX"]),
                            CoordY = Convert.ToSingle(_reader["CoordY"]),
                            CoordZ = Convert.ToSingle(_reader["CoordZ"]),
                            Zoomscale = Convert.ToInt32(_reader["Zoomscale"]),
                            ToggleBase = ReadSQLiteBool(SafeGetValue(_reader, "ToggleBase"), true), 
                            ToggleColor = ReadSQLiteBool(SafeGetValue(_reader, "ToggleColor"), true), 
                            ToggleIcon = ReadSQLiteBool(SafeGetValue(_reader, "ToggleIcon"), true), 
                            ToggleName = ReadSQLiteBool(SafeGetValue(_reader, "ToggleName"), true),  
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
    /*public List<Element> GetAllElementsOfType(Element.Table table)
 {
     List<Element> returnList = new List<Element>();
     foreach (string id in GetIDValuesForTable(table))
     { 
         Dictionary<string, string> fieldNamesAndValues = new Dictionary<string, string>();
         foreach (string field in GetFieldNamesForTable(table))            
             fieldNamesAndValues.Add(field, GetEntryForTableAndFieldWithID(table, field, id));            
         returnList.Add(AssembleElementOfTypeFromFields(table, fieldNamesAndValues));
     }
     return returnList;
 }*/
    //todo-cleanup: replace above map and pin read functions to this one, except fix map having a TypeString property that mismatches with Table: Type
     /*public List<T> GetAllObjects<T>(string tableName) where T : new()
    {
        List<T> returnList = new List<T>();
        string query = $"SELECT * FROM {tableName}";

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
                        T obj = new T();
                        foreach (var prop in typeof(T).GetProperties())
                        {
                            if (!Equals(_reader[prop.Name], DBNull.Value))
                            {
                                if (prop.PropertyType == typeof(float))
                                {
                                    prop.SetValue(obj, Convert.ToSingle(_reader[prop.Name]));
                                }
                                else if (prop.PropertyType == typeof(int))
                                {
                                    prop.SetValue(obj, Convert.ToInt32(_reader[prop.Name]));
                                }
                                else
                                {
                                    prop.SetValue(obj, _reader[prop.Name].ToString());
                                }
                            }
                        }

                        // Special case for the Map type
                        if (typeof(T) == typeof(Map))
                        {
                            Map map = obj as Map;
                            if (Enum.TryParse(map.TypeString, out Map.Type mapType))
                            {
                                map.type = mapType;
                            }
                        }
                        returnList.Add(obj);
                    }
                }
            }
            _dbconn.Close();
        }
        return returnList;
    }*/
    
    
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
