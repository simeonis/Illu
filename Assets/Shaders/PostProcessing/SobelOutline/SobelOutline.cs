using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/SobelOutline")]
public sealed class SobelOutline : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Thickness of the Sobel Outline")]
    public ClampedFloatParameter thickness = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Multiplier of the Depth-Component of the Sobel Outline")]
    public ClampedFloatParameter depthMultiplier = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Bias of the Depth-Component of the Sobel Outline")]
    public ClampedFloatParameter depthBias = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Multiplier of the Normal-Component of the Sobel Outline")]
    public ClampedFloatParameter normalMultiplier = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Bias of the Normal-Component of the Sobel Outline")]
    public ClampedFloatParameter normalBias = new ClampedFloatParameter(0f, 0f, 10f);

    [Tooltip("Color of the Sobel Outline")]
    public ColorParameter color = new ColorParameter(Color.black);


    Material m_Material;

    public bool IsActive() => m_Material != null;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

    const string kShaderName = "Hidden/Shader/SobelOutlineHLSL";

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


        m_Material.SetFloat("_OutlineThickness", thickness.value);
        m_Material.SetFloat("_OutlineDepthMultiplier", depthMultiplier.value);
        m_Material.SetFloat("_OutlineDepthBias", depthBias.value);
        m_Material.SetFloat("_OutlineNormalMultiplier", normalMultiplier.value);
        m_Material.SetFloat("_OutlineNormalBias", normalBias.value);
        m_Material.SetColor("_OutlineColor", color.value);

        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }


}