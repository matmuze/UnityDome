using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

public class AssetLoader
{
    [MenuItem("Asset Loader/Bake Stencil Masks")]
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
    
    [MenuItem("Asset Loader/Load Frustum Mesh")]
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

    [MenuItem("Asset Loader/Load Opacity Masks")]
    private static void LoadOpacityMasks()
    {
        for (int i = 1; i <= 6; i++)
        {
            var texture = MyUtility.LoadOpacityMask(i);
            var path = "Assets/Dome Assets/Resources/Opacity Masks/OpacityMask" + i + ".asset";
            AssetDatabase.CreateAsset(texture, path);
        }
    }

    [MenuItem("Asset Loader/Load Correction Meshes")]
    private static void LoadCorrectionMeshes()
    {
        for (int i = 1; i <= 6; i++)
        {
            var mesh = MyUtility.LoadCorrectionMesh(i);
            var path = "Assets/Dome Assets/Resources/Correction Meshes/CorrectionMesh" + i + ".asset";
            AssetDatabase.CreateAsset(mesh, path);
        }
    }

    //[MenuItem("Debug/Load Correction Meshes2")]
    //private static void LoadCorrectionMeshes2()
    //{
    //    for (int i = 1; i <= 6; i++)
    //    {
    //        var mesh = MyUtility.LoadDistortionMesh2(i);
    //        var path = "Assets/Dome Assets/Resources/Correction Meshes/CorrectionMesh_dbg_" + i + ".asset";
    //        AssetDatabase.CreateAsset(mesh, path);
    //    }
    //}
}

#endif
