using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/ToonOutline")]
public sealed class ToonOutline : CustomPostProcessVolumeComponent, IPostProcessComponent
{

    [Tooltip("Controls the Thickness of the effect.")]
    public ClampedFloatParameter Thickness = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Controls the DepthStrength of the effect.")]
    public ClampedFloatParameter DepthStrength = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Controls the DepthThickness of the effect.")]
    public ClampedFloatParameter DepthThickness = new ClampedFloatParameter(0f, 0f, 10f);

    [Tooltip("Controls the DepthThreshold of the effect.")]
    public ClampedFloatParameter DepthThreshold = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Pick the color of the outline.")]
    public ColorParameter OutlineColor = new ColorParameter(Color.black);

    Material m_Material;

    public bool IsActive() => m_Material != null;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/ToonOutline";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume ToonOutline is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;


        m_Material.SetTexture("_InputTexture", source);
        m_Material.SetFloat("_Thickness", Thickness.value);
        m_Material.SetFloat("_DepthStrength", DepthStrength.value);
        m_Material.SetFloat("_DepthThickness", DepthThickness.value);
        m_Material.SetFloat("_DepthThreshold", DepthThreshold.value);
        m_Material.SetColor("_Color", OutlineColor.value);

        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
