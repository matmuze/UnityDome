using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class InitDepthBuffer : MonoBehaviour
{
    CommandBuffer _commands;

    public Texture2D StencilMaskTexture;
    public Material InitStencilBufferMaterial;

    void OnDisable()
    {
        if (Camera.main != null && _commands != null)
        {
            Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _commands);
            _commands = null;
        }
    }

    void OnEnable()
    {
        if (Camera.main != null && _commands == null)
        {
            _commands = new CommandBuffer();
            _commands.Clear();
            _commands.Blit(StencilMaskTexture, BuiltinRenderTextureType.CameraTarget, InitStencilBufferMaterial, 0);
            Camera.main.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commands);
        }
    }
}
