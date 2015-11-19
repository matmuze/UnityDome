using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;

public class AssetLoader : MonoBehaviour
{
    //[MenuItem("Debug/Make Torus")]
    //public static void Torus()
    //{
    //    float Pi = 3.14159f;

    //    float segmentRadius = 1f;
    //    float tubeRadius = 0.1f;
    //    int segments = 32;
    //    int tubes = 12;

    //    // Total vertices
    //    int totalVertices = segments * tubes;

    //    // Total primitives
    //    int totalPrimitives = totalVertices * 2;

    //    // Total indices
    //    int totalIndices = totalPrimitives * 3;

    //    // Init vertexList and indexList
    //    ArrayList verticesList = new ArrayList();
    //    ArrayList indicesList = new ArrayList();

    //    // Save these locally as floats
    //    float numSegments = segments;
    //    float numTubes = tubes;

    //    // Calculate size of segment and tube
    //    float segmentSize = 2 * Pi / numSegments;
    //    float tubeSize = 2 * Pi / numTubes;

    //    // Create floats for our xyz coordinates
    //    float x = 0;
    //    float y = 0;
    //    float z = 0;

    //    // Init temp lists with tubes and segments
    //    ArrayList segmentList = new ArrayList();
    //    ArrayList tubeList = new ArrayList();

    //    // Loop through number of tubes
    //    for (int i = 0; i < numSegments; i++)
    //    {
    //        tubeList = new ArrayList();

    //        for (int j = 0; j < numTubes; j++)
    //        {
    //            // Calculate X, Y, Z coordinates.
    //            x = (segmentRadius + tubeRadius * Mathf.Cos(j * tubeSize)) * Mathf.Cos(i * segmentSize);
    //            y = (segmentRadius + tubeRadius * Mathf.Cos(j * tubeSize)) * Mathf.Sin(i * segmentSize);
    //            z = tubeRadius * Mathf.Sin(j * tubeSize);

    //            // Add the vertex to the tubeList
    //            tubeList.Add(new Vector3(x, z, y));

    //            // Add the vertex to global vertex list
    //            verticesList.Add(new Vector3(x, z, y));
    //        }

    //        // Add the filled tubeList to the segmentList
    //        segmentList.Add(tubeList);
    //    }

    //    // Loop through the segments
    //    for (int i = 0; i < segmentList.Count; i++)
    //    {
    //        // Find next (or first) segment offset
    //        int n = (i + 1) % segmentList.Count;

    //        // Find current and next segments
    //        ArrayList currentTube = (ArrayList)segmentList[i];
    //        ArrayList nextTube = (ArrayList)segmentList[n];

    //        // Loop through the vertices in the tube
    //        for (int j = 0; j < currentTube.Count; j++)
    //        {
    //            // Find next (or first) vertex offset
    //            int m = (j + 1) % currentTube.Count;

    //            // Find the 4 vertices that make up a quad
    //            Vector3 v1 = (Vector3)currentTube[j];
    //            Vector3 v2 = (Vector3)currentTube[m];
    //            Vector3 v3 = (Vector3)nextTube[m];
    //            Vector3 v4 = (Vector3)nextTube[j];

    //            // Draw the first triangle
    //            indicesList.Add((int)verticesList.IndexOf(v1));
    //            indicesList.Add((int)verticesList.IndexOf(v2));
    //            indicesList.Add((int)verticesList.IndexOf(v3));

    //            // Finish the quad
    //            indicesList.Add((int)verticesList.IndexOf(v3));
    //            indicesList.Add((int)verticesList.IndexOf(v4));
    //            indicesList.Add((int)verticesList.IndexOf(v1));
    //        }
    //    }

    //    Mesh mesh = new Mesh();

    //    Vector3[] vertices = new Vector3[totalVertices];
    //    verticesList.CopyTo(vertices);
    //    int[] triangles = new int[totalIndices];
    //    indicesList.CopyTo(triangles);
    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;

    //    string path = "Assets/TorusMesh.asset";

    //    Mesh tmp = (Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
    //    if (tmp)
    //    {
    //        AssetDatabase.DeleteAsset(path);
    //        tmp = null;
    //    }

    //    AssetDatabase.CreateAsset(mesh, path);
    //    AssetDatabase.SaveAssets();
    //}

    [MenuItem("Debug/Bake Stencil Masks")]
    public static void BakeStencilMasks()
    {
        for (int i = 1; i <= 6; i++)
        {
            var stencilMaskTexture = MyUtility.BakeStencilMask(i);

            var path = "Assets/Dome Assets/Resources/Stencil Masks/StencilMask" + i + ".asset";
            AssetDatabase.CreateAsset(stencilMaskTexture, path);

            path = Application.dataPath + "/../Debug Stencil Masks/" + i + "/StencilMask" + i + ".png";
            File.WriteAllBytes(path, stencilMaskTexture.EncodeToPNG());
        }
    }

    //[MenuItem("Debug/Save Config File")]
    //public static void SaveConfigFile()
    //{
    //    var appSettings = new AppSettings();
    //    appSettings.Port = 1;
    //    appSettings.IpAdress = "hello world";
    //    appSettings.NodeId = 2;
    //    appSettings.ScreenHeight = 100;
    //    appSettings.ScreenWidth = 100;
    //    appSettings.FullScreen = false;

    //    var json = JsonConvert.SerializeObject(appSettings);
    //    var configFile = MyUtility.ConfigPath + "config.txt";
    //    File.WriteAllText(configFile, json);

    //    //var configFile = MyUtility.ConfigPath + "default_config.txt";
    //    //var jsonCfg = File.ReadAllText(configFile);
    //    //Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(jsonCfg);
    //}

[MenuItem("Debug/Load Frustum Mesh")]
    private static void LoadFrustumMesh()
    {
        var mesh = new Mesh();
        var indices = new List<int>();
        var vertices = new List<Vector3>();
        
        for(int i = 1; i <= 6; i++)
        {
            var plane = MyUtility.LoadSgcViewPlane(i);

            vertices.Add(plane[0]);
            vertices.Add(plane[1]);
            vertices.Add(plane[2]);
            vertices.Add(plane[2] + (plane[0] - plane[1]));
        }

        //foreach (var plane in MyUtility.GetAllFrustrumCorners())
        //{
        //    vertices.Add(plane[0]);
        //    vertices.Add(plane[1]);
        //    vertices.Add(plane[2]);
        //    vertices.Add(plane[2] + (plane[0] - plane[1]));
        //}

        for (int i = 0; i < vertices.Count; i++)
        {
            indices.Add(i);
        }

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
        //mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        var path = "Assets/Meshes/FrustumMesh.asset";
        AssetDatabase.CreateAsset(mesh, path);
    }

    [MenuItem("Debug/Load Opacity Masks")]
    private static void LoadOpacityMasks()
    {
        for (int i = 1; i <= 6; i++)
        {
            var texture = MyUtility.LoadOpacityMask(i);
            var path = "Assets/Dome Assets/Resources/Opacity Masks/OpacityMask" + i + ".asset";
            AssetDatabase.CreateAsset(texture, path);
        }
    }

    [MenuItem("Debug/Load Correction Meshes")]
    private static void LoadCorrectionMeshes()
    {
        for (int i = 1; i <= 6; i++)
        {
            var mesh = MyUtility.LoadCorrectionMesh(i);
            var path = "Assets/Dome Assets/Resources/Correction Meshes/CorrectionMesh" + i + ".asset";
            AssetDatabase.CreateAsset(mesh, path);
        }
    }

    [MenuItem("Debug/Load Correction Meshes2")]
    private static void LoadCorrectionMeshes2()
    {
        for (int i = 1; i <= 6; i++)
        {
            var mesh = MyUtility.LoadDistortionMesh2(i);
            var path = "Assets/Dome Assets/Resources/Correction Meshes/CorrectionMesh_dbg_" + i + ".asset";
            AssetDatabase.CreateAsset(mesh, path);
        }
    }
}

#endif
