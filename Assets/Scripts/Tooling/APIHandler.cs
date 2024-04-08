using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEditor;


[CreateAssetMenu(fileName = "APIHandler", menuName = "ScriptableObjects/APIHandler", order = 0)]
public class APIHandler : ScriptableObject 
{
    
    private const string apiUrl = "https://www.onlyworlds.com/api/worlddata/"; 
    
    public void FetchWorldWithKey(string apiKey)
    {
        if (apiKey.Length != 8)
        {
            Debug.Log("API key is wrong length: " + apiKey.Length  + " Please try again with 8 digits: " + apiKey.Length);
            return;
        }
        FetchAndUpdateDataAsync(apiKey);
    }
    public void SendWorldWithKey(string apiKey)
    {
        if (apiKey.Length != 8)
        {
            Debug.Log("API key is wrong length: " + apiKey.Length  + " Please try again with 8 digits: " + apiKey.Length);
            return;
        }
        SendDataAsync(apiKey);
    }

    
    private async UniTaskVoid FetchAndUpdateDataAsync(string apiKey)
    {
    
        Debug.Log("[APIHandler] Starting data fetch...");
        var webRequest = CreateWebRequest(apiUrl+apiKey);

        try
        {
            await webRequest.SendWebRequest();

            Debug.Log("result: " +webRequest.result );
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[APIHandler] Error fetching data: {webRequest.error}");
            }
            else
            { 
                RootControl.DBWriter.ImportWorldFromJSON(webRequest.downloadHandler.text);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[APIHandler] Exception occurred: {ex.Message}");
        }
        finally
        {
            webRequest.Dispose();
        }
    } 
    public async UniTaskVoid SendDataAsync(string apiKey)
    {
        World world = RootControl.World;

        // Serialize the world object to JSON
        string worldDataJson = JsonConvert.SerializeObject(world);

        // Construct the full JSON structure expected by your API 
        string jsonDataToSend = "{\"worldKey\":\"" + apiKey + "\", \"worldData\":" + worldDataJson + "}";
 
        string apiAddress = apiUrl  ;

        Debug.Log("apiAddress   " + apiAddress);
        Debug.Log("jsonDataToSend   " + jsonDataToSend);
        // Create a new UnityWebRequest for posting the JSON data
        UnityWebRequest webRequest = new UnityWebRequest(apiAddress, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonDataToSend);
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        try
        {
            // Await the response from the web request
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[EditorDataManager] Error sending world data: {webRequest.error}");
            }
            else
            {
                // Handle successful send here
                Debug.Log("[EditorDataManager] World data successfully sent to the API.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EditorDataManager] Exception occurred while sending world data: {ex.Message}");
        }
        finally
        {
            webRequest.Dispose();
        }
    }
    
    
    
    
    
    private UnityWebRequest CreateWebRequest(string url)
    {
        var webRequest = UnityWebRequest.Get(url); 
        return webRequest;
    }
    [JsonObject]
    public class ApiResponse
    {
        // Assuming the JSON within "worldData" is a raw JSON string
        [JsonProperty("worldData")]
        public string JSONBody { get; set; }
    
        public string Message { get; set; }
        public int Status { get; set; }
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
