using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

#if UNITY_EDITOR

using UnityEditor.Rendering.HighDefinition;
using UnityEditor;

[CustomPassDrawerAttribute(typeof(EdgeDetection))]
class EdgeDetectionEditor : CustomPassDrawer
{
    private class Styles
    {
        public static float defaultLineSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public static GUIContent edgeThreshold = new GUIContent("Edge Threshold", "Edge detect effect threshold.");
        public static GUIContent edgeRadius = new GUIContent("Edge Radius", "Radius of the edge detect effect.");
        public static GUIContent glowColor = new GUIContent("Color", "Color of the effect");
    }

    SerializedProperty edgeDetectThreshold;
    SerializedProperty edgeRadius;
    SerializedProperty glowColor;

    protected override void Initialize(SerializedProperty customPass)
    {
        edgeDetectThreshold = customPass.FindPropertyRelative("edgeDetectThreshold");
        edgeRadius = customPass.FindPropertyRelative("edgeRadius");
        glowColor = customPass.FindPropertyRelative("glowColor");
    }

    // We only need the name to be displayed, the rest is controlled by the TIPS effect
    protected override PassUIFlag commonPassUIFlags => PassUIFlag.Name;

    protected override void DoPassGUI(SerializedProperty customPass, Rect rect)
    {
        edgeDetectThreshold.floatValue = EditorGUI.Slider(rect, Styles.edgeThreshold, edgeDetectThreshold.floatValue, 0.1f, 5f);
        rect.y += Styles.defaultLineSpace;
        edgeRadius.intValue = EditorGUI.IntSlider(rect, Styles.edgeRadius, edgeRadius.intValue, 1, 6);
        rect.y += Styles.defaultLineSpace;
        glowColor.colorValue = EditorGUI.ColorField(rect, Styles.glowColor, glowColor.colorValue, true, false, true);
    }

    protected override float GetPassHeight(SerializedProperty customPass) => Styles.defaultLineSpace * 3;
}

#endif

class EdgeDetection : CustomPass
{
    public float edgeDetectThreshold = 1;
    public int edgeRadius = 1;
    public Color glowColor = Color.white;

    Material fullscreenMaterial;
    RTHandle tipsBuffer; // additional render target for compositing the custom and camera color buffers

    int compositingPass;

    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        fullscreenMaterial = CoreUtils.CreateEngineMaterial("FullScreen/EdgeDetection");
        tipsBuffer = RTHandles.Alloc(Vector2.one, TextureXR.slices, dimension: TextureXR.dimension, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, useDynamicScale: true, name: "TIPS Buffer");

        compositingPass = fullscreenMaterial.FindPass("Compositing");
        targetColorBuffer = TargetBuffer.Custom;
        targetDepthBuffer = TargetBuffer.Custom;
        clearFlags = ClearFlag.All;
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (fullscreenMaterial == null)
            return;

        ctx.propertyBlock.SetTexture("_TIPSBuffer", tipsBuffer);
        ctx.propertyBlock.SetFloat("_EdgeDetectThreshold", edgeDetectThreshold);
        ctx.propertyBlock.SetColor("_GlowColor", glowColor);
        ctx.propertyBlock.SetFloat("_EdgeRadius", (float)edgeRadius);

        CoreUtils.SetRenderTarget(ctx.cmd, tipsBuffer, ClearFlag.Color);
        CoreUtils.DrawFullScreen(ctx.cmd, fullscreenMaterial, shaderPassId: compositingPass, properties: ctx.propertyBlock);

        CoreUtils.DrawFullScreen(ctx.cmd, fullscreenMaterial, ctx.cameraColorBuffer, properties: ctx.propertyBlock);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(fullscreenMaterial);
        tipsBuffer.Release();
    }
}