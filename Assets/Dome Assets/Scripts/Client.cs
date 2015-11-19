using System;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    private bool _initFlag;
    private NetworkClient _myClient;

    private Vector3 _serverCameraPos;
    private Quaternion _serverCameraRot;

    private static Vector3 _cameraInitPosition;
    private static Quaternion _cameraInitRotation;

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            EndClient();
            return;
        }

        if (_initFlag)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, _cameraInitPosition + _serverCameraPos, 0.5f);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, _serverCameraRot * _cameraInitRotation, 0.5f);
        }
    }

    void OnGUI()
    {
        var style = new GUIStyle {normal = {textColor = Color.red}};
        GUI.Label(new Rect(50 , Screen.height - 20, 100, 20), "- Node: " + _nodeId + " - Status: " + (_initFlag ? "Connected" : "Not Connected"), style);
    }

    //*****//

    private string _serverIP;
    private int _port;
    private int _nodeId;

    // Create a client and connect to the server port
    public void Setup(string serverIP, int port, int nodeID)
    {
        if (nodeID < 1 || nodeID > 6)
        {
            EndClient();
            throw new Exception("Invalid node ID");
        }

        Debug.Log("Setup client - server IP: " + serverIP + " port: " +port + " node ID:" + nodeID);

        _initFlag = false;
        _port = port;
        _nodeId = nodeID;
        _serverIP = serverIP;

        _myClient = new NetworkClient();
        _myClient.RegisterHandler(MsgType.Connect, OnClientConnect);
        _myClient.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
        _myClient.RegisterHandler(MyMsgTypes.INIT_CAMERA_MESSAGE, OnInitClientCamera);
        _myClient.RegisterHandler(MyMsgTypes.UPDATE_CAMERA_MESSAGE, OnUpdateClientCamera);
        _myClient.Connect(_serverIP, _port);

        Camera.main.transform.rotation = Quaternion.identity;
    }

    private void OnClientConnect(NetworkMessage netMsg)
    {
        var msg = new NodeInfoMessage {NodeId = _nodeId};
        _myClient.Send(MyMsgTypes.INIT_NODE_MESSAGE, msg);
    }

    private void OnClientDisconnect(NetworkMessage netMsg)
    {
        gameObject.GetComponent<NetworkManager>().EndClient();
    }

    private void OnInitClientCamera(NetworkMessage netMsg)
    {
        Debug.Log("Init client camera");
        InitCamera();
        _initFlag = true;
    }

    private void OnUpdateClientCamera(NetworkMessage netMsg)
    {
        if (!_initFlag) return;

        var updateCameraMessage = netMsg.ReadMessage<UpdateCameraMessage>();    
        _serverCameraPos = updateCameraMessage.ServerCameraPosition;
        _serverCameraRot = updateCameraMessage.ServerCameraRotation;
    }

    private void EndClient()
    {
        Debug.Log("End client");
        if(_myClient != null)_myClient.Disconnect();
        gameObject.GetComponent<NetworkManager>().EndClient();
    }

    private void InitCamera()
    {
        var vertices = MyUtility.LoadSgcViewPlane(_nodeId);

        var nearClipPlane = Camera.main.gameObject.GetComponent<Camera>().nearClipPlane;
        var farClipPlane = Camera.main.gameObject.GetComponent<Camera>().farClipPlane;

        var rotation = MyUtility.GetCameraRotation(vertices);
        var projection = MyUtility.GetCameraProjection(vertices, rotation, nearClipPlane, farClipPlane);
        
        _cameraInitPosition = Vector3.zero;
        _cameraInitRotation = rotation;

        Camera.main.transform.rotation = rotation;
        Camera.main.projectionMatrix = projection;
        
        //var meshPath = "Correction Meshes/CorrectionMesh" + _nodeId;
        //var mesh = Resources.Load<Mesh>(meshPath);

        //var maskPath = "Opacity Masks/OpacityMask" + _nodeId;
        //var mask = Resources.Load<Texture2D>(maskPath);

        var mesh = MyUtility.LoadCorrectionMesh(_nodeId);
        var mask = MyUtility.LoadOpacityMask(_nodeId);

        Camera.main.gameObject.GetComponent<CameraCorrection>().CorrectionMesh = mesh;
        Camera.main.gameObject.GetComponent<CameraCorrection>().OpacityMask = mask;
        Camera.main.gameObject.GetComponent<CameraCorrection>().enabled = true;
    }
}