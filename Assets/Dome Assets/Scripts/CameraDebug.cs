using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class CameraDebug : MonoBehaviour
{
    public int NodeId;
    public Matrix4x4 mat;

    // Use this for initialization
    void OnEnable()
    {
        var vertices = MyUtility.LoadSgcViewPlane(NodeId);
        var rotation = MyUtility.GetCameraRotation(vertices);
        var projection = MyUtility.GetCameraProjection(vertices, rotation, GetComponent<Camera>().nearClipPlane, GetComponent<Camera>().farClipPlane);

        GetComponent<Camera>().transform.rotation = rotation;
        GetComponent<Camera>().projectionMatrix = projection;

        mat = projection;
    }

    void Update()
    {
        GetComponent<Camera>().projectionMatrix = mat;
    }
}
