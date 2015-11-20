using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyMsgTypes
{
    public static short INIT_NODE_MESSAGE = 444;
    public static short INIT_CAMERA_MESSAGE = 445;
    public static short UPDATE_CAMERA_MESSAGE = 446;
    public static short INVALID_NODE_ID_MESSAGE = 447;
};

public class NodeInfoMessage : MessageBase
{
    public int NodeId;
}

public class InitCameraMessage : MessageBase
{
    public Vector3 CameraInitPosition;
    public Quaternion CameraInitRotation;
    public Matrix4x4 ProjectionMatrix;
}

public class UpdateCameraMessage : MessageBase
{
    public Vector3 ServerCameraPosition;
    public Quaternion ServerCameraRotation;
}

public class InvalidNodeIdMessage : MessageBase
{
    
}

public enum MyNetworkState
{
    Lobby = 0,
    Server = 1,
    Client = 2,
    InitError = 3
};

public class AppSettings
{
    public int NodeId;
    public int Port;
    public string IpAdress;
    public int ScreenWidth;
    public int ScreenHeight;
    public bool FullScreen;
    public bool IsServer;
    public bool BeginOnStartUp;
    public float MasterRotationOffsetX;
    public float MasterRotationOffsetY;
}

//Product product = new Product();
//product.Name = "Apple";
//product.Expiry = new DateTime(2008, 12, 28);
//product.Sizes = new string[] { "Small" };

//string json = JsonConvert.SerializeObject(product);

public class NetworkManager : MonoBehaviour
{   
    private int DefaultPort = 8888;
    private string Localhost = "127.0.0.1";
    private static MyNetworkState NetworkCurrentState = MyNetworkState.Lobby;
    
    //**** UI stuffs ****//
    public Canvas Canvas;
    public InputField ServerPortField;
    public InputField ClientIpAddressField;
    public InputField ClientPortField;
    public InputField ClientNodeIdField;
    //*****//

    public CameraController CameraControler;
    public static AppSettings AppSettings = new AppSettings();

    void Start()
    {
        var configFile = MyUtility.ConfigPath + "config.txt";
        if (!File.Exists(configFile))
        {
            CameraControler.enabled = false;
            NetworkCurrentState = MyNetworkState.InitError;
            throw new Exception("Config file not found");
        }

        var json = File.ReadAllText(configFile);
        AppSettings = JsonConvert.DeserializeObject<AppSettings>(json);

        // Reserialze and write to disk in case changes have been made in the class
        json = JsonConvert.SerializeObject(AppSettings);
        File.WriteAllText(configFile, json);

        Debug.Log("Start Lobby");
        Application.runInBackground = true;
        Screen.SetResolution(AppSettings.ScreenWidth, AppSettings.ScreenHeight, AppSettings.FullScreen);
        
        ServerPortField.text = AppSettings.Port.ToString(); 
        ClientPortField.text = AppSettings.Port.ToString();
        ClientNodeIdField.text = AppSettings.NodeId.ToString();
        ClientIpAddressField.text = AppSettings.IpAdress;

        if (AppSettings.BeginOnStartUp)
        {
            if(AppSettings.IsServer) StartServer();
            else StartClient();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Screen.SetResolution(Screen.width, Screen.height, !Screen.fullScreen);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            var resolution = Screen.currentResolution;
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }

    void OnGUI()
    {
        var style = new GUIStyle { normal = { textColor = Color.red } };
        switch (NetworkCurrentState)
        {
            case MyNetworkState.Lobby:
                GUI.Label(new Rect(10, Screen.height - 20, 100, 20), "Lobby", style);
                break;
            case MyNetworkState.Server:
                GUI.Label(new Rect(10, Screen.height - 20, 100, 20), "Server", style);
                break;
            case MyNetworkState.Client:
                GUI.Label(new Rect(10, Screen.height - 20, 100, 20), "Client", style);
                break;
            case MyNetworkState.InitError:
                style.fontSize = 30;
                GUI.Label(new Rect(Screen.width * 0.5f -200, Screen.height * 0.5f -50, 100, 20), "Init Error: Config file not found", style);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void StartServer()
    {
        var port = string.IsNullOrEmpty(ServerPortField.text) ? DefaultPort : int.Parse(ServerPortField.text);
        gameObject.AddComponent<Server>().Setup(port, AppSettings.MasterRotationOffsetX, AppSettings.MasterRotationOffsetY);

        HideCanvas();
        CameraControler.enabled = true; // Enable camera controls if server
        NetworkCurrentState = MyNetworkState.Server;
        Camera.main.GetComponent<CameraCorrection>().enabled = false;
        Camera.main.GetComponent<InitDepthBuffer>().enabled = false;
    }

    public void StartClient()
    {
        var ipAddress = string.IsNullOrEmpty(ClientIpAddressField.text) ? Localhost : ClientIpAddressField.text;
        var port = string.IsNullOrEmpty(ClientPortField.text) ? DefaultPort : int.Parse(ClientPortField.text);
        var nodeId = string.IsNullOrEmpty(ClientNodeIdField.text) ? 1 : int.Parse(ClientNodeIdField.text);
        gameObject.AddComponent<Client>().Setup(ipAddress, port, nodeId);

        HideCanvas();
        Cursor.visible = false;
        CameraControler.enabled = false; // Disable camera controls if server
        NetworkCurrentState = MyNetworkState.Client;
    }

    public void EndClient()
    {
        ShowCanvas();
        NetworkCurrentState = MyNetworkState.Lobby;
        Destroy(gameObject.GetComponent<Client>());

        Cursor.visible = true;
        CameraControler.enabled = false;
        Camera.main.GetComponent<CameraCorrection>().enabled = false;
        Camera.main.GetComponent<InitDepthBuffer>().enabled = false;
    }

    public void EndServer()
    {
        ShowCanvas();
        NetworkCurrentState = MyNetworkState.Lobby;
        Destroy(gameObject.GetComponent<Server>());
        CameraControler.enabled = false;
    }

    private void HideCanvas()
    {
        Canvas.enabled = false;
    }

    private void ShowCanvas()
    {
        Canvas.enabled = true;
    }
}