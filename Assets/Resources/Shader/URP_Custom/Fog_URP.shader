Shader "Animmal/Fog_URP"
{
    Properties
    {
        _FogIntensity("FogIntensity", Range(0, 1)) = 0
        _FogMaxIntensity("FogMaxIntensity", Range(0, 1)) = 0
        _FogColor("FogColor", Color) = (0.1470588, 1, 0.634, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "ForceNoShadowCasting" = "True"
        }

        Pass
        {
            Name "FogDepthFade"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FogColor;
                half _FogIntensity;
                half _FogMaxIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = input.screenPos.xy / max(input.screenPos.w, 1e-6);
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceDepth = input.screenPos.w;

                half fogScale = lerp(0.1h, 0.4h, saturate(_FogIntensity));
                half alpha = saturate(abs(sceneDepth - surfaceDepth) * fogScale);
                alpha = min(alpha, _FogMaxIntensity);

                return half4(_FogColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
