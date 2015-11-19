using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Object = UnityEngine.Object;

public static class ViewPlaneCorners
{
    public const int LowerLeft = 0;
    public const int UpperLeft = 1;
    public const int UpperRight = 2;
}

public struct SCISSTexturedVertex
{
    float x, y, z;
    float tx, ty, tz;

    //public SCISSTexturedVertex()
    //{
    //    //x = y = z = tx = ty = tz = 0;
    //}

    public Vector3 UV()
    {
        return new Vector2(tx, ty);
    }

    public Vector3 Position()
    {
        return new Vector3(x, y, z);
    }
};

public struct SCISSViewData
{
    public float qx, qy, qz, qw; // Rotation quaternion
    public float x, y, z;        // Position of view (currently unused in Uniview)
    public float fovUp, fovDown, fovLeft, fovRight;

    public Quaternion GetRotation()
    {
        return new Quaternion(qx, qy, qz, qw);
    }

    public Vector3 GetPosition()
    {
        return new Vector3(x, y, z);
    }

    public override string ToString()
    {
        var toString = "Position - x: " + x + " y: " + y + " z: " + z;
        toString += " Rotation - qx: " + qx + " qy: " + qy + " qz: " + qz + " qw: " + qw;
        toString += " Fov up: " + fovUp + " Fov down: " + fovDown + " Fov left: " + fovLeft + " Fov right: " + fovRight;
        return toString;
    }
};

public class MyUtility
{


    //public static void LoadConfigData()
    //{
    //    StreamReader reader = new StreamReader(Application.dataPath + "/../config/dome_alpha.xml", System.Text.Encoding.GetEncoding("utf-8"));
    //    configData = reader.ReadToEnd();
    //    reader.Close();
    //}

    //public static List<List<Vector3>> GetAllFrustrumCorners()
    //{
    //    if (string.IsNullOrEmpty(configData))
    //    {
    //        LoadConfigData();
    //    }

    //    List<List<Vector3>> frustrumPoints = new List<List<Vector3>>();

    //    XmlDocument xmlDoc = new XmlDocument();
    //    xmlDoc.LoadXml(configData);
    //    XmlNodeList config = xmlDoc.GetElementsByTagName("Viewport");

    //    if (config.Count > 0)
    //    {
    //        foreach (XmlNode a in config)
    //        {
    //            foreach (XmlNode b in a.ChildNodes)
    //            {
    //                if (b.Name == "Viewplane")
    //                {
    //                    var corners = new List<Vector3>();

    //                    foreach (XmlNode c in b.ChildNodes)
    //                    {
    //                        if (c.NodeType == XmlNodeType.Comment) continue;
    //                        var corner = new Vector3(float.Parse(c.Attributes[0].Value), float.Parse(c.Attributes[1].Value), float.Parse(c.Attributes[2].Value));
    //                        corner.z = -corner.z; // z-up to y-up conversion
    //                        corners.Add(corner);
    //                    }

    //                    if (frustrumPoints.Count != 0)
    //                    {
    //                        if (frustrumPoints.Last()[0] != corners[0] && frustrumPoints.Last()[1] != corners[1] && frustrumPoints.Last()[2] != corners[2])
    //                        {
    //                            frustrumPoints.Add(corners);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        frustrumPoints.Add(corners);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    //var topPlane = new Plane(frustrumPoints.Last()[0], frustrumPoints.Last()[1], frustrumPoints.Last()[2]);
    //    //var rotation = Quaternion.FromToRotation(topPlane.normal, -Vector3.up);

    //    //for (int i = 0; i < frustrumPoints.Count; i++)
    //    //{
    //    //    frustrumPoints[i][0] = rotation * frustrumPoints[i][0];
    //    //    frustrumPoints[i][1] = rotation * frustrumPoints[i][1];
    //    //    frustrumPoints[i][2] = rotation * frustrumPoints[i][2];
    //    //}

    //    //for (int i = 0; i < frustrumPoints.Count; i++)
    //    //{
    //    //    frustrumPoints[i][0] = Quaternion.AngleAxis(90, Vector3.right) * frustrumPoints[i][0];
    //    //    frustrumPoints[i][1] = Quaternion.AngleAxis(90, Vector3.right) * frustrumPoints[i][1];
    //    //    frustrumPoints[i][2] = Quaternion.AngleAxis(90, Vector3.right) * frustrumPoints[i][2];
    //    //}

    //    return frustrumPoints;
    //}

    public static string ConfigPath = Application.dataPath + "/../Config/";

    public static T StructFromBytes<T>(byte[] source) where T : new()
    {
        T str = new T();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(source, 0, ptr, size);

        str = (T)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);

        return str;
    }

    public static T[] StructArrayFromByteArray<T>(byte[] source) where T : struct
    {
        T[] destination = new T[source.Length / Marshal.SizeOf(typeof(T))];
        GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(source, 0, pointer, source.Length);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
    
    public static Matrix4x4 CorrectD3DProjectionMatrix(Matrix4x4 mat)
    {
        var matrix = mat;

        bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
        if (d3d)
        {
            // Invert Y for rendering to a render texture
            for (int i = 0; i < 4; i++)
            {
                matrix[1, i] = -matrix[1, i];
            }
            // Scale and bias from OpenGL -> D3D depth range
            for (int i = 0; i < 4; i++)
            {
                matrix[2, i] = matrix[2, i] * 0.5f + matrix[3, i] * 0.5f;
            }
        }

        return matrix;
    }

    public static Quaternion MatrixToRotation(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }

    public static List<Vector3> LoadSgcViewPlane(int nodeId)
    {
        string meshPath = Application.dataPath + "/../config/channel" + nodeId + "_planar.sgc";

        if (!File.Exists(meshPath))
        {
            throw new Exception("File not found");
        }

        using (var b = new BinaryReader(File.Open(meshPath, FileMode.Open)))
        {
            var fileID = b.ReadChars(3);
            if (new string(fileID) != "SGC")
            {
                throw new Exception("File not valid");
            }

            var fileVersion = (int)b.ReadChar();
            var mappingType = b.ReadUInt32();

            var viewData = MyUtility.StructFromBytes<SCISSViewData>(b.ReadBytes(Marshal.SizeOf(typeof(SCISSViewData))));
            //Debug.Log(viewData.ToString());

            return GetViewPlaneCoordsUsingFOVs(viewData.fovUp, viewData.fovDown, viewData.fovLeft, viewData.fovRight, viewData.GetRotation());
        }
    }

    private static List<Vector3> GetViewPlaneCoordsUsingFOVs(float up, float down, float left, float right, Quaternion rot, float dist = 10.0f)
    {
        var viewPlanePoints = new Vector3[3];

        viewPlanePoints[ViewPlaneCorners.LowerLeft].x = dist * Mathf.Tan(Mathf.Deg2Rad * left);
        viewPlanePoints[ViewPlaneCorners.LowerLeft].y = dist * Mathf.Tan(Mathf.Deg2Rad * down);
        viewPlanePoints[ViewPlaneCorners.LowerLeft].z = -dist;
        viewPlanePoints[ViewPlaneCorners.LowerLeft] = rot * viewPlanePoints[ViewPlaneCorners.LowerLeft];
        viewPlanePoints[ViewPlaneCorners.LowerLeft].z *= -1;

        viewPlanePoints[ViewPlaneCorners.UpperLeft].x = dist * Mathf.Tan(Mathf.Deg2Rad * left);
        viewPlanePoints[ViewPlaneCorners.UpperLeft].y = dist * Mathf.Tan(Mathf.Deg2Rad * up);
        viewPlanePoints[ViewPlaneCorners.UpperLeft].z = -dist;
        viewPlanePoints[ViewPlaneCorners.UpperLeft] = rot * viewPlanePoints[ViewPlaneCorners.UpperLeft];
        viewPlanePoints[ViewPlaneCorners.UpperLeft].z *= -1;

        viewPlanePoints[ViewPlaneCorners.UpperRight].x = dist * Mathf.Tan(Mathf.Deg2Rad * right);
        viewPlanePoints[ViewPlaneCorners.UpperRight].y = dist * Mathf.Tan(Mathf.Deg2Rad * up);
        viewPlanePoints[ViewPlaneCorners.UpperRight].z = -dist;
        viewPlanePoints[ViewPlaneCorners.UpperRight] = rot * viewPlanePoints[ViewPlaneCorners.UpperRight];
        viewPlanePoints[ViewPlaneCorners.UpperRight].z *= -1;

        return viewPlanePoints.ToList();
    }

    public static Quaternion GetCameraRotation(List<Vector3> frustumCorners)
    {
        var lowerLeftCorner = frustumCorners[ViewPlaneCorners.LowerLeft];
        var upperLeftCorner = frustumCorners[ViewPlaneCorners.UpperLeft];
        var upperRightCorner = frustumCorners[ViewPlaneCorners.UpperRight];

        var plane_x = (upperRightCorner - upperLeftCorner).normalized;
        var plane_y = (upperLeftCorner - lowerLeftCorner).normalized;
        var plane_z = Vector3.Cross(plane_x, plane_y).normalized;

        var DCM = Matrix4x4.identity;

        DCM[0, 0] = Vector3.Dot(plane_x, Vector3.right);
        DCM[1, 0] = Vector3.Dot(plane_x, Vector3.up);
        DCM[2, 0] = Vector3.Dot(plane_x, Vector3.forward);

        DCM[0, 1] = Vector3.Dot(plane_y, Vector3.right);
        DCM[1, 1] = Vector3.Dot(plane_y, Vector3.up);
        DCM[2, 1] = Vector3.Dot(plane_y, Vector3.forward);

        DCM[0, 2] = Vector3.Dot(plane_z, Vector3.right);
        DCM[1, 2] = Vector3.Dot(plane_z, Vector3.up);
        DCM[2, 2] = Vector3.Dot(plane_z, Vector3.forward);

        // Get camera rotation
        return MatrixToRotation(DCM);
    }
    
    public static Matrix4x4 GetCameraProjection(List<Vector3> frustumCorners, Quaternion rotation, float near, float far)
    {
        var lowerLeftCorner = frustumCorners[ViewPlaneCorners.LowerLeft];
        var upperRightCorner = frustumCorners[ViewPlaneCorners.UpperRight];

        var quatRotation_inv = Quaternion.Inverse(rotation);
        var lowerLeftCorner_inv = quatRotation_inv * lowerLeftCorner;
        var upperRightCorner_inv = quatRotation_inv * upperRightCorner;
        
        //nearFactor = near clipping plane / focus plane dist
        float nearFactor = near / lowerLeftCorner_inv.z;
        if (nearFactor < 0) nearFactor = -nearFactor;

        var left = lowerLeftCorner_inv.x * nearFactor;
        var right = upperRightCorner_inv.x *nearFactor;
        var bottom = lowerLeftCorner_inv.y *nearFactor;
        var top = upperRightCorner_inv.y * nearFactor;

        return PerspectiveOffCenter(left, right, bottom, top, near, far);
    }

    public static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }

    public static Mesh LoadDistortionMesh2(int nodeId)
    {
        var meshPath = Application.dataPath + "/../config/channel" + nodeId + "_planar.sgc";

        if (!File.Exists(meshPath)) { throw new Exception("File not found"); }

        var b = new BinaryReader(File.Open(meshPath, FileMode.Open));
        
        var fileID = b.ReadChars(3);
        if (new string(fileID) != "SGC")
        {
            throw new Exception("File not valid");
        }

        Debug.Log("File version: " + (int) b.ReadChar());
        Debug.Log("Mapping type: " + b.ReadUInt32());

        var viewData = MyUtility.StructFromBytes<SCISSViewData>(b.ReadBytes(Marshal.SizeOf(typeof (SCISSViewData))));
        Debug.Log(viewData.ToString());

        //viewPlanes.Add(GetViewPlaneCoordsUsingFOVs(viewData.fovUp, viewData.fovDown, viewData.fovLeft, viewData.fovRight, viewData.GetRotation()));

        var numVertices = b.ReadUInt32()*b.ReadUInt32();
        Debug.Log("Num vertices: " + numVertices);

        var verticesBytes = b.ReadBytes(Marshal.SizeOf(typeof (SCISSTexturedVertex))*(int) numVertices);
        var verticesArray = MyUtility.StructArrayFromByteArray<SCISSTexturedVertex>(verticesBytes);

        //*****//

        var vertices = new List<Vector3>();
        var uvs = new List<Vector3>();
        var indices = new List<int>();

        foreach (var vertex in verticesArray)
        {
            uvs.Add(vertex.UV());
        }

        foreach (var vertex in verticesArray)
        {
            vertices.Add(vertex.Position()); //) - new Vector3(.5f,.5f,.5f));
        }

        var meshSize = (int) Mathf.Sqrt(vertices.Count);
        for (var r = 0; r < meshSize - 1; r++)
        {
            for (var c = 0; c < meshSize - 1; c++)
            {
                indices.Add(r + c*meshSize);
                indices.Add(r + c*meshSize + 1);
                indices.Add(r + c*meshSize + meshSize);

                indices.Add(r + c*meshSize + 1);
                indices.Add(r + c*meshSize + meshSize + 1);
                indices.Add(r + c*meshSize + meshSize);
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        b.Close();

        return mesh;
    }

    public static Mesh LoadCorrectionMesh(int nodeId)
    {
        var meshPath = Application.dataPath + "/../config/correctionmesh" + nodeId + ".txt";

        if (!File.Exists(meshPath))
        {
            throw new Exception("File not found");
        }

        var vertices = new List<Vector3>();
        var uvs = new List<Vector3>();
        var indices = new List<int>();

        var sr = new StreamReader(meshPath);
        
        string line;

        while ((line = sr.ReadLine()) != null)
        {
            if (line == "HEADER_END")
            {
                var numVertices = int.Parse(sr.ReadLine());
                //Debug.Log(numVertices);

                for (var j = 0; j < numVertices; j++)
                {
                    var vertex = new Vector3(float.Parse(sr.ReadLine()), float.Parse(sr.ReadLine()), 0);
                    vertex = vertex * 0.5f + new Vector3(0.5f, 0.5f, 0);
                    var uv = new Vector3(float.Parse(sr.ReadLine()), float.Parse(sr.ReadLine()), 0);

                    vertices.Add(vertex);
                    uvs.Add(uv);

                    //Debug.Log(vertices.Last());
                    //Debug.Log(uvs.Last());
                }

                //var numIndices = int.Parse(sr.ReadLine());
                //Debug.Log(numIndices);

                //for (int i = 0; i < numIndices; i++)
                //{
                //    var index = int.Parse(sr.ReadLine());
                //    indices.Add(index);

                //    int a = 0;
                //    //Debug.Log(indices.Last());
                //}
            }
        }

        var meshSize = (int)Mathf.Sqrt(vertices.Count);
        for (var r = 0; r < meshSize - 1; r++)
        {
            for (var c = 0; c < meshSize - 1; c++)
            {
                indices.Add(r + c * meshSize);
                indices.Add(r + c * meshSize + meshSize);
                indices.Add(r + c * meshSize + 1);

                indices.Add(r + c * meshSize + 1);
                indices.Add(r + c * meshSize + meshSize);
                indices.Add(r + c * meshSize + meshSize + 1);
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        return mesh;
    }

    public static Texture2D LoadOpacityMask(int nodeId)
    {
        var maskPath = Application.dataPath + "/../config/channel" + nodeId + ".png";
        if (!File.Exists(maskPath)) { throw new Exception("File not found"); }

        var bytes = File.ReadAllBytes(maskPath);

        var maskTexture = new Texture2D(1900, 1200, TextureFormat.ARGB32, false);
        maskTexture.LoadImage(bytes);
        maskTexture.Apply();

        return maskTexture;
    }

    public static Texture2D BakeStencilMask(int nodeId)
    {
        var opacityMask = MyUtility.LoadOpacityMask(nodeId);
        var correctionMesh = MyUtility.LoadCorrectionMesh(nodeId);

        var material = new Material(Resources.Load<Shader>("BakeStencilMask"));
        material.hideFlags = HideFlags.HideAndDontSave;
        
        var stencilMask = new RenderTexture(opacityMask.width, opacityMask.height, 24, RenderTextureFormat.ARGB32);
        stencilMask.enableRandomWrite = true;
        stencilMask.Create();
        
        Graphics.SetRenderTarget(stencilMask);
        GL.Clear(true, true, Color.black);
        
        var temp = RenderTexture.GetTemporary(opacityMask.width, opacityMask.height, 24, RenderTextureFormat.ARGB32);
        Graphics.SetRenderTarget(temp);
        GL.Clear(true, true, Color.black);

        Graphics.SetRandomWriteTarget(1, stencilMask);
        MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

        // Draw correction mesh
        material.SetInt("_Width", opacityMask.width);
        material.SetInt("_Height", opacityMask.height);
        material.SetTexture("_OpacityMaskTex", opacityMask);
        material.SetMatrix("_OrthoMatrix", MyUtility.CorrectD3DProjectionMatrix(Matrix4x4.Ortho(0, 1, 0, 1, 0, 1)));
        material.SetPass(0);
        Graphics.DrawMeshNow(correctionMesh, Matrix4x4.identity);
        Graphics.ClearRandomWriteTargets();

        // Copy render texture to texture 2D
        RenderTexture.active = stencilMask;
        var stencilMaskTexture = new Texture2D(opacityMask.width, opacityMask.height, TextureFormat.ARGB32, false);
        stencilMaskTexture.ReadPixels(new Rect(0, 0, opacityMask.width, opacityMask.height), 0, 0);
        stencilMaskTexture.Apply();
        RenderTexture.active = null;
        
        RenderTexture.ReleaseTemporary(temp);
        stencilMask.Release();
        Object.DestroyImmediate(material);

        return stencilMaskTexture;
    }

    public static void DummyBlit()
    {
        var dummy1 = RenderTexture.GetTemporary(8, 8, 24, RenderTextureFormat.ARGB32);
        var dummy2 = RenderTexture.GetTemporary(8, 8, 24, RenderTextureFormat.ARGB32);
        var active = RenderTexture.active;

        Graphics.Blit(dummy1, dummy2);
        RenderTexture.active = active;

        dummy1.Release();
        dummy2.Release();
    }
}



