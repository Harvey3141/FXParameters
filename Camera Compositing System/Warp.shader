Shader "Hidden/FX/Warp"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "grey" {} 
        _TextureWarp("Texture Warp", 2D) = "white" {} 
        _TextureBlend("Texture Blend", 2D) = "white" {}
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

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

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);

    TEXTURE2D(_TextureWarp); 
    SAMPLER(sampler_TextureWarp); 

    TEXTURE2D(_TextureBlend); 
    SAMPLER(sampler_TextureBlend); 

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float4 warpColor = _TextureWarp.Sample(sampler_TextureWarp, input.texcoord);
        float2 texCoord = warpColor.xy;
        float2 blendTexCoord = input.texcoord.xy;
        float4 mainColor = _MainTex.Sample(sampler_MainTex, texCoord);
        float4 blendColor = _TextureBlend.Sample(sampler_TextureBlend, blendTexCoord);
        float4 outputColor = mainColor * blendColor;


        return _MainTex.Sample(sampler_MainTex, input.texcoord); // Show input texture
        //return float4(texCoord, 0.0, 1.0); // Show warp texture coordinates
        //return _TextureBlend.Sample(sampler_TextureBlend, input.texcoord); // Show blend texture
        

        //return outputColor;
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "PP"

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
