  
    using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using World_Model;

    [AttributeUsage(AttributeTargets.Property)]
    public class SQLiteBoolAttribute : Attribute { }
    
    
    [CreateAssetMenu(fileName = "DBWriter", menuName = "ScriptableObjects/DBWriter", order = 0)]
    public class DBWriter : ScriptableObject
    {
        private string dbActivePath;
        private string dbSourcePath;
        private string dbBackupPath;

        private Dictionary<Element.Category, string> tableNames = new Dictionary<Element.Category, string>
        {
            { Element.Category.Location, "Location" },
            { Element.Category.Character, "Character" },
            { Element.Category.Object, "Object" },
            { Element.Category.Creature, "Creature" },
            { Element.Category.Construct, "Concept" },
            { Element.Category.Force, "God" },
            { Element.Category.Event, "Event" },
            { Element.Category.Relation, "Relation" },
            { Element.Category.Collective, "Collective" },
            { Element.Category.Territory, "Territory" },
            { Element.Category.Title, "Title" },
            { Element.Category.Institution, "Institution" },
            { Element.Category.Species, "Race" },
            { Element.Category.Family, "Family" },
            { Element.Category.Trait, "Trait" },
            { Element.Category.Law, "Law" },
            { Element.Category.Language, "Language" },
            { Element.Category.Ability, "Ability" }, 
        };
        
        
        
    public void SetDatabasePath(string worldName)
    {
        dbActivePath = "URI=file:" + Application.dataPath + "/" + RootControl.MonoLoader.worldPath + worldName + ".db";  
        dbSourcePath =  Application.dataPath + "/" + RootControl.MonoLoader.worldPath + worldName + ".db";  
        dbBackupPath =  Application.dataPath + "/Resources/DatabaseFlushBackup.db";
    }
    
    /// <summary>
    /// Create a backup of the active World db file right before re-writing world back to it
    /// </summary>
    public void CopyActiveToTempTable()
    {
        File.Copy(dbSourcePath, dbBackupPath, true);
    }
 
 
    
    private void ExecuteQuery(string query, Dictionary<string, object> parameters = null)
    {
        using (var connection = new SqliteConnection(dbActivePath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;

                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> parameter in parameters)
                    {
                        command.Parameters.Add(new SqliteParameter(parameter.Key, parameter.Value));
                    }
                }
             //   if (query.Substring(0,6) != "DELETE")
                    Debug.Log("query:" + query); 
                command.ExecuteNonQuery();
            }
            connection.Close(); 
        }
    }
 
    
    
    
    public void WriteElement(Element element)
    {
        using (SqliteConnection connection = new SqliteConnection(dbActivePath))
        {
            connection.Open();

            string tableName;
            if (!tableNames.TryGetValue(element.category, out tableName))
            {
                Debug.LogError("Invalid Element.Category: " + element.category);
                return;
            }


            // Get column names from the table
            List<string> columnNames = new List<string>();
            using (SqliteCommand command = new SqliteCommand($"PRAGMA table_info({tableName})", connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnNames.Add(reader["name"].ToString());
                    }
                }
            }

            var properties = element.GetType().GetProperties()
                .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string));

            // Filter out properties that do not match column names in the database
            properties = properties.Where(p => columnNames.Contains(p.Name.ToSnakeCase()));

            if (properties.Any())
            {
                var columns = string.Join(", ", properties.Select(p => p.Name.ToSnakeCase()));
                var parameters = string.Join(", ", properties.Select(p => "@" + p.Name));

                var values = new Dictionary<string, object>();
                foreach (var prop in properties)
                {
                    object value = prop.GetValue(element);

                    if (prop.PropertyType == typeof(bool) && Attribute.IsDefined(prop, typeof(SQLiteBoolAttribute)))
                        value = (bool)value ? 1 : 0;

                    values.Add("@" + prop.Name, value); // The parameter name does not need conversion
                }

                ExecuteQuery($"INSERT INTO {tableName} ({columns}) VALUES ({parameters})", values);
            }
            else
            {
                Debug.LogError("No matching columns for properties in " + tableName);
            }
            connection.Close();
        }
    }
    
    public void WriteTypingTable(Element.Category category)
    { 
        using (SqliteConnection connection = new SqliteConnection(dbActivePath))
        {
            connection.Open();
            string tableName = category + "Typing";

            List<TableTyping> typingData = RootControl.WorldParser.GetTypingTablesForElementTable(category);

            foreach (var tblType in typingData)
            {
                // Prepare data for insert query
                List<string> columnsList = new List<string> { "types", "types_custom", "subtypes", "subtypes_custom" };
                List<string> parametersList = new List<string> { "@types", "@types_custom", "@subtypes", "@subtypes_custom" };
                /*if (tblType.Subtypes != null)
                    if (tblType.Subtypes.Count > 0)
                        if (tblType.Subtypes[0] != null)
                            Debug.Log("REGsubtype" + tblType.Subtypes[0]);
                if (tblType.SubtypeCustoms != null)
                 if (tblType.SubtypeCustoms.Count > 0)
                   if (tblType.SubtypeCustoms[0] != null)
                      Debug.Log("CUSTOMsubtype" + tblType.SubtypeCustoms[0]);*/
                var values = new Dictionary<string, object>
                {
                    { "@types", tblType.Supertype },
                    { "@types_custom", tblType.TypeCustom },
                    { "@subtypes", string.Join(",", tblType.Subtypes) },
                    { "@subtypes_custom", string.Join(",", tblType.SubtypeCustoms) }
                };

                string columns = string.Join(", ", columnsList);
                string parameters = string.Join(", ", parametersList);

//                Debug.Log($"Writing to database: Subtypes={string.Join(",", tblType.Subtypes)}, SubtypesCustom={string.Join(",", tblType.SubtypeCustoms)}");

                try
                {
                    ExecuteQuery($"INSERT INTO {tableName} ({columns}) VALUES ({parameters})", values);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error executing query with columns {columns} and values {string.Join(", ", values.Select(p => $"{p.Key} = {p.Value}"))}. Exception: {ex}");
                    throw;
                }
            }

            connection.Close();
        }
    }

 

 
private bool DoesColumnExist(string tableName, string columnName, SqliteConnection connection)
{
    string checkColumnExistsQuery = $"PRAGMA table_info({tableName})";
    DataTable tableInfo = new DataTable();
    using (var command = new SqliteCommand(checkColumnExistsQuery, connection))
    {
        using (var reader = command.ExecuteReader())
        {
            tableInfo.Load(reader);
        }
    }

    return tableInfo.Rows.Cast<DataRow>().Any(row => row["name"].ToString() == columnName);
}


    public void WriteNonElement(object obj, string tableName)
    {
        using (SqliteConnection connection = new SqliteConnection(dbActivePath))
        {
            connection.Open();

            // Get column names from the table
            List<string> columnNames = new List<string>();
            using (SqliteCommand command = new SqliteCommand($"PRAGMA table_info({tableName})", connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnNames.Add(reader["name"].ToString());
                    }
                }
            }

            // Get properties of the object
            var properties = obj.GetType().GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string));

            // Compare properties to column names and filter out the non-matching ones
            properties = properties.Where(p => columnNames.Contains(p.Name));

            if (properties.Any())
            {
                var columns = string.Join(", ", properties.Select(p => p.Name));
                var parameters = string.Join(", ", properties.Select(p => "@" + p.Name));

                var values = new Dictionary<string, object>();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(obj);
                    
                    if (Attribute.IsDefined(prop, typeof(SQLiteBoolAttribute)))
                        value = (bool)value ? 1 : 0;

                    // Check for empty string in ParentMap or PinnedMap and replace with null
                    if ((prop.Name == "parent_map" || prop.Name == "pinned_map") && string.IsNullOrEmpty(value as string))
                        value = null;

                    values.Add("@" + prop.Name, value);
                }

                ExecuteQuery($"INSERT INTO {tableName} ({columns}) VALUES ({parameters})", values);
            }
            else
            {
                Debug.LogError("No matching columns for properties in " + tableName);
            }
            connection.Close();
        }
    }
    public void FlushTable(Element.Category category)
    { 
        ExecuteQuery("DELETE FROM " + category);
    }
    public void FlushTable(string table)
    { 
        ExecuteQuery("DELETE FROM " + table);
    }
    
    
    private DBReader _dbReader;
    private DBReader Reader
    {
        get
        {
            if (_dbReader == null)
            {
                _dbReader = FindObjectOfType<DBReader>();
            }
            return _dbReader;
        }
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
