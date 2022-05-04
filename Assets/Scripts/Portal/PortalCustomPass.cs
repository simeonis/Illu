using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Reflection;

public class PortalCustomPass : CustomPass
{
    [Header("Input")]
    public Camera portalCamera;

    [Header("Output")]
    public RenderTexture outputPortalTexture;
    
    [Header("Options")]
    [SerializeField] bool overrideDepthState = false;
    [SerializeField] CompareFunction depthCompareFunction = CompareFunction.LessEqual;
    public bool render = true;

    FieldInfo cullingResultField;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Temporary hack for culling override in HDRP 10.x
        cullingResultField = typeof(CustomPassContext).GetField(nameof(CustomPassContext.cullingResults));
    }

    void DrawOutlineMeshes(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hDCamera, CullingResults cullingResults)
    {

    }
    
    protected override void Execute(CustomPassContext ctx)
    {
        if (!render || ctx.hdCamera.camera == portalCamera || portalCamera == null || ctx.hdCamera.camera.cameraType == CameraType.SceneView)
        {
            return;
        }

        portalCamera.TryGetCullingParameters(out var cullingParameters);
        cullingParameters.cullingOptions = CullingOptions.OcclusionCull;
        cullingResultField.SetValueDirect(__makeref(ctx), ctx.renderContext.Cull(ref cullingParameters));
        
        var overrideDepthTest = new RenderStateBlock(RenderStateMask.Depth) 
        { 
            depthState = new DepthState(overrideDepthState, depthCompareFunction) 
        };

        portalCamera.aspect = ctx.hdCamera.camera.aspect;
        portalCamera.pixelRect = ctx.hdCamera.camera.pixelRect;

        if (outputPortalTexture != null)
        {
            SyncRenderTextureAspect(outputPortalTexture, ctx.hdCamera.camera);
            CoreUtils.SetRenderTarget(ctx.cmd, outputPortalTexture, ClearFlag.All);
            CustomPassUtils.RenderFromCamera(ctx, portalCamera, portalCamera.cullingMask, overrideRenderState: overrideDepthTest);
        }
    }

    void SyncRenderTextureAspect(RenderTexture rt, Camera camera)
    {
        float aspect = rt.width / (float)rt.height;

        if (!Mathf.Approximately(aspect, camera.aspect))
        {
            rt.Release();
            rt.width = camera.pixelWidth;
            rt.height = camera.pixelHeight;
            rt.Create();
        }
    }
}
