using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    public const int NumConnectionMax = 6;

    private int[] _nodeSlots = new int[NumConnectionMax];
    private Quaternion _initLookAtRotation;

    // Send update message every frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Exit server");
            NetworkServer.Reset();
            gameObject.GetComponent<NetworkManager>().EndServer();
        }

        var msg = new UpdateCameraMessage
        {
            ServerCameraPosition = Camera.main.transform.position,
            ServerCameraRotation = Camera.main.transform.rotation * Quaternion.Inverse(_initLookAtRotation)
        };

        // Use unreliable channel
        NetworkServer.SendByChannelToAll(MyMsgTypes.UPDATE_CAMERA_MESSAGE, msg, 1);
    }

    void OnGUI()
    {
        var numNodes = 0;
        for (var i = 0; i < NumConnectionMax; i++)
        {
            if (_nodeSlots[i] != -1)
            {
                numNodes ++;
            }
        }

        var style = new GUIStyle {normal = {textColor = Color.red}};
        GUI.Label(new Rect(70, Screen.height - 20, 100, 20), "- Num Nodes: " + numNodes, style);
    }

    //*****//

    // Create a server and listen on a port
    public void Setup(int port)
    {
        Debug.Log("Setup server on port: " + port);
        
        NetworkServer.maxDelay = 0;
        NetworkServer.Listen(port);
        NetworkServer.RegisterHandler(MyMsgTypes.INIT_NODE_MESSAGE, OnInitClientNode);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);

        for (var i = 0; i < NumConnectionMax; i++) _nodeSlots[i] = -1;
        
        _initLookAtRotation = Quaternion.AngleAxis(-90, new Vector3(1, 0, 0));
    }

    public void OnInitClientNode(NetworkMessage netMsg)
    {
        var nodeInfoMessage = netMsg.ReadMessage<NodeInfoMessage>();
        var nodeId = nodeInfoMessage.NodeId;

        Debug.Log("New Connection: " + netMsg.conn.connectionId + " - Node ID: " + nodeId);
        
        if (nodeId < 1 || nodeId > 6)
        {
            netMsg.conn.Disconnect();
            Debug.Log("Invalid node ID");
            return;
        }

        if (_nodeSlots[nodeId-1] != -1)
        {
            netMsg.conn.Disconnect();
            Debug.Log("Node slot already occupied");
            return;
        }

        _nodeSlots[nodeId-1] = netMsg.conn.connectionId;
        SendClientCameraInitMessage(netMsg.conn, nodeId);
    }

    public void OnClientDisconnect(NetworkMessage netMsg)
    {
        for (int i = 0; i < NumConnectionMax; i++)
        {
            if (_nodeSlots[i] == netMsg.conn.connectionId)
            {
                _nodeSlots[i] = -1;
                Debug.Log("Disconnect node: " + i+1);
            }
        }
    }

    private void SendClientCameraInitMessage(NetworkConnection conn, int nodeId)
    {
        Debug.Log("Send client: " + nodeId + " init message");
        var initCameraMessage = new InitCameraMessage();
        NetworkServer.SendToClient(conn.connectionId, MyMsgTypes.INIT_CAMERA_MESSAGE, initCameraMessage);
    }

    //private void SendClientInvalidNodeIDMessage(NetworkConnection conn)
    //{
    //    Debug.Log("Send invalid node ID message");

    //    var invalidNodeIdMessage = new InvalidNodeIdMessage();
    //    NetworkServer.SendToClient(conn.connectionId, MyMsgTypes.INIT_CAMERA_MESSAGE, invalidNodeIdMessage);
    //}
}