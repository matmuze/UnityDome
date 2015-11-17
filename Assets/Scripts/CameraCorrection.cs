using UnityEngine;

[ExecuteInEditMode]
public class CameraCorrection : MonoBehaviour
{
    public Mesh CorrectionMesh;
    public Material CorrectionMaterial;

    public Texture2D OpacityMask;
    public Material OpacityMaskMaterial;
    
    public Matrix4x4 matDebug;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        var temp = RenderTexture.GetTemporary(src.width, src.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8);
        Graphics.SetRenderTarget(temp);
        GL.Clear(true, true, Color.black);
        
        var orthoMatrix = MyUtility.CorrectD3DProjectionMatrix(Matrix4x4.Ortho(0, 1, 0, 1, 0, 1));

        // Do distortion 
        CorrectionMaterial.SetTexture("_SourceTex", src);
        CorrectionMaterial.SetMatrix("_OrthoMatrix", orthoMatrix);
        CorrectionMaterial.SetPass(0);
        Graphics.DrawMeshNow(CorrectionMesh, Matrix4x4.identity);

        // Do alpha mask 
        OpacityMaskMaterial.SetTexture("_OpacityTex", OpacityMask);
        Graphics.Blit(temp, dest, OpacityMaskMaterial, 0);
        RenderTexture.ReleaseTemporary(temp);

        matDebug = Camera.main.projectionMatrix;
    }
}
