Shader "Hidden/Shader/SobelOutlineHLSL"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    // TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
    // TEXTURE2D_SAMPLER2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2);

    float _OutlineThickness;
    float _OutlineDepthMultiplier;
    float _OutlineDepthBias;
    float _OutlineNormalMultiplier;
    float _OutlineNormalBias;

    float4 _OutlineColor;

    float4 SobelSample(Texture2D t, SamplerState s, float2 uv, float3 offset)
    {
        float4 pixelCenter = t.Sample(s, uv);
        float4 pixelLeft = t.Sample(s, uv - offset.xz);
        float4 pixelRight = t.Sample(s, uv + offset.xz);
        float4 pixelUp = t.Sample(s, uv + offset.zy);
        float4 pixelDown = t.Sample(s, uv - offset.zy);

        return abs(pixelLeft - pixelCenter) +
            abs(pixelRight - pixelCenter) +
            abs(pixelUp - pixelCenter) +
            abs(pixelDown - pixelCenter);
    }

    float SobelDepth(float ldc, float ldl, float ldr, float ldu, float ldd)
    {
        return abs(ldl - ldc) +
            abs(ldr - ldc) +
            abs(ldu - ldc) +
            abs(ldd - ldc);
    }

    // float SobelSampleDepth(Texture2D t, SamplerState s, float2 uv, float3 offset)
    // {
    //     float pixelCenter = LinearEyeDepth(t.Sample(s, uv).r);
    //     float pixelLeft = LinearEyeDepth(t.Sample(s, uv - offset.xz).r);
    //     float pixelRight = LinearEyeDepth(t.Sample(s, uv + offset.xz).r);
    //     float pixelUp = LinearEyeDepth(t.Sample(s, uv + offset.zy).r);
    //     float pixelDown = LinearEyeDepth(t.Sample(s, uv - offset.zy).r);

    //     return SobelDepth(pixelCenter, pixelLeft, pixelRight, pixelUp, pixelDown);
    // }

     float4 CustomPostProcess(Varyings input) : SV_Target
    {
        // float3 offset = float3((1.0 / _ScreenParams.x), (1.0 / _ScreenParams.y), 0.0) * _OutlineThickness;
        // float3 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord).rgb;

        // float sobelDepth = SobelSampleDepth(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord.xy, offset);
        // sobelDepth = pow(saturate(sobelDepth) * _OutlineDepthMultiplier, _OutlineDepthBias);

        // float3 sobelNormalVec = SobelSample(_CameraGBufferTexture2, sampler_CameraGBufferTexture2, input.texcoord.xy, offset).rgb;
        // float sobelNormal = sobelNormalVec.x + sobelNormalVec.y + sobelNormalVec.z;
        // sobelNormal = pow(sobelNormal * _OutlineNormalMultiplier, _OutlineNormalBias);

        // float sobelOutline = saturate(max(sobelDepth, sobelNormal));

        // float3 outlineColor = lerp(sceneColor, _OutlineColor.rgb, _OutlineColor.a);
        //float3 color = lerp(sceneColor, outlineColor, sobelOutline);


        float3 sceneColor = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord).rgb;

        float3 color = float3(1.0,0.0,0.0); 

        return float4(sceneColor, 1);
    }
        ENDHLSL

        SubShader
    {
        Cull Off ZWrite Off ZTest Always

            Pass
        {
            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
}