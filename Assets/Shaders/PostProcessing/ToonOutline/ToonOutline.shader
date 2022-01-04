Shader "Hidden/Shader/ToonOutline"
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

    // List of properties to control your post process effect
    float _Thickness;
    float _DepthStrength;
    float _DepthThickness;
    float _DepthThreshold;
    float3 _Color;
    TEXTURE2D_X(_InputTexture);

    float GetDepth(float2 uv)
    {
        float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
        return Linear01Depth(rawDepth, _ZBufferParams);
    }

    static float2 sobelSamplePoints[9] = {
        float2(-1,1),float2(0,1), float2(1,1),	
        float2(-1,0),float2(0,0), float2(1,0),
        float2(-1,-1),float2(0, -1), float2(1,-1),
    };


    static float sobelXMatrix[9] = {
        1,0,-1,
        2,0,-2,
        1,0,-1
    };

    static float sobelYMatrix[9] = {
        1,2,1,
        0,0,0,
        -1,-2,-1
    };

    float DepthSobel(float UV, float Thickness){
        float2 sobel = 0;
        [unroll] for(int i = 0; i < 9; i++){
            float depth = LoadCameraDepth(UV + sobelSamplePoints[i] * Thickness);
            sobel += depth * float2(sobelXMatrix[i], sobelYMatrix[i]);

        }

        return length(sobel);

    };

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
   
        float2 uv = input.texcoord;
            
        float dpSol =  DepthSobel(input.positionCS.xy, _Thickness);
        float sm = smoothstep(0,1, _DepthThreshold);
        float powered = pow(sm, _DepthThickness);

        float end = powered * _DepthStrength;
        //float3 blended =  outColor * (1 - end) + _Color * end;
        float3 blended = lerp(outColor,_Color, end);
        return float4(blended, 1);

    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "ToonOutline"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
