Shader "Hidden/FX/CameraCompositor"
{
    Properties
    {
        // This property is necessary to make the CommandBuffer.Blit bind the source texture to _MainTex
        _MainTex("Main Texture", 2DArray) = "grey" {}
        _TextureA("Texture A", 2D) = "white" {} 
        _TextureB("Texture B", 2D) = "white" {}
        _TextureKey("Texture Key", 2D) = "white" {} 
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

    TEXTURE2D_X(_MainTex);

    TEXTURE2D(_TextureA); 
    SAMPLER(sampler_TextureA); 

    TEXTURE2D(_TextureB); 
    SAMPLER(sampler_TextureB); 

    TEXTURE2D(_TextureKey); 
    SAMPLER(sampler_TextureKey); 

    float _Brightness;

    float3 RGBtoHSV(float3 rgb)
{
    float R = rgb.r;
    float G = rgb.g;
    float B = rgb.b;
    float minRGB = min(R, min(G, B));
    float maxRGB = max(R, max(G, B));
    float deltaRGB = maxRGB - minRGB;

    float H = 0.0;
    float S = 0.0;
    float V = maxRGB;

    if (maxRGB > 0.0)
    {
        S = deltaRGB / maxRGB;
    }

    if (S > 0.0)
    {
        if (R == maxRGB)
            H = (G - B) / deltaRGB;
        else if (G == maxRGB)
            H = 2.0 + (B - R) / deltaRGB;
        else if (B == maxRGB)
            H = 4.0 + (R - G) / deltaRGB;

        H *= 60.0;
        if (H < 0.0)
            H += 360.0;
    }

    return float3(H, S, V);
}


    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Note that if HDUtils.DrawFullScreen is used to render the post process, use ClampAndScaleUVForBilinearPostProcessTexture(input.texcoord.xy) to get the correct UVs

        float3 colorA = SAMPLE_TEXTURE2D(_TextureA, sampler_TextureA, input.texcoord).xyz;
        float3 colorB = SAMPLE_TEXTURE2D(_TextureB, sampler_TextureB, input.texcoord).xyz;
        float3 colorKey = SAMPLE_TEXTURE2D(_TextureKey, sampler_TextureKey, input.texcoord).xyz;


        //float threshold = 0.3; 
        //bool isKeyPink = (colorKey.r == 1.0) && (colorKey.g == 0.0) && (colorKey.b == 1.0);
        //
        //float3 pinkColor = float3(1.0, 0.0, 1.0); // Target pink color
        //float tolerance = 0.4; // Define a tolerance for color comparison
        //
        //// Calculate the absolute difference between the colorB and the target pink color
        //float3 diff = abs(colorB - pinkColor);
        //
        //// Check if the color is within the tolerance for all components
        //bool isBPink = (diff.r <= tolerance) && (diff.g <= tolerance) && (diff.b <= tolerance);

        float3 targetColor = float3(1.0, 0.0, 1.0); // Pink
        float hueTolerance = 15.0; // Adjust as needed
        float saturationTolerance = 0.9; // Adjust as needed
        float brightnessTolerance = 1.0; // Adjust as needed

        // Convert both target and current pixel color to HSV
        float3 targetHSV = RGBtoHSV(targetColor);
        float3 pixelHSV = RGBtoHSV(colorB); // Assume colorB is the current pixel color

        bool isKeyed = abs(targetHSV.x - pixelHSV.x) < hueTolerance && 
        abs(targetHSV.y - pixelHSV.y) < saturationTolerance &&
        abs(targetHSV.z - pixelHSV.z) < brightnessTolerance;


        //bool isBPink = (colorB.r == 1.0) && (colorB.g == 0.0) && (colorB.b == 1.0);


        float3 outputColor = isKeyed ? colorA : colorB;

        outputColor *= _Brightness;

        return float4(outputColor, 1.0);
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
