Shader "URP_Custom/Standard (Specular setup)_URP"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5

        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _GlossMapScale("Smoothness Factor", Range(0, 1)) = 1
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2, 1)
        _SpecGlossMap("Specular", 2D) = "white" {}
        _SpecularHighlights("Specular Highlights", Float) = 1
        _GlossyReflections("Glossy Reflections", Float) = 1

        _BumpScale("Scale", Float) = 1
        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0, 1)) = 1
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0, 0, 0, 1)
        _EmissionMap("Emission", 2D) = "white" {}

        [HideInInspector] _Mode("__mode", Float) = 0
        [HideInInspector] _SrcBlend("__src", Float) = 1
        [HideInInspector] _DstBlend("__dst", Float) = 0
        [HideInInspector] _ZWrite("__zw", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap); SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SpecGlossMap_ST;
                float4 _BumpMap_ST;
                float4 _OcclusionMap_ST;
                float4 _EmissionMap_ST;
                half4 _Color;
                half _Cutoff;
                half _Glossiness;
                half _GlossMapScale;
                half _SmoothnessTextureChannel;
                half4 _SpecColor;
                half _SpecularHighlights;
                half _GlossyReflections;
                half _BumpScale;
                half _OcclusionStrength;
                half4 _EmissionColor;
                half _Mode;
                half _SrcBlend;
                half _DstBlend;
                half _ZWrite;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 tangentWS : TEXCOORD2;
                float2 uvMain : TEXCOORD3;
                float2 uvSpec : TEXCOORD4;
                float2 uvBump : TEXCOORD5;
                float2 uvOcclusion : TEXCOORD6;
                float2 uvEmission : TEXCOORD7;
                half fogFactor : TEXCOORD8;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.tangentWS = half4(normalInputs.tangentWS, input.tangentOS.w);
                output.uvMain = TRANSFORM_TEX(input.uv, _MainTex);
                output.uvSpec = TRANSFORM_TEX(input.uv, _SpecGlossMap);
                output.uvBump = TRANSFORM_TEX(input.uv, _BumpMap);
                output.uvOcclusion = TRANSFORM_TEX(input.uv, _OcclusionMap);
                output.uvEmission = TRANSFORM_TEX(input.uv, _EmissionMap);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half3 GetNormalWS(Varyings input)
            {
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uvBump), _BumpScale);
                half3 normalWS = normalize(input.normalWS);
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = cross(normalWS, tangentWS) * input.tangentWS.w;
                return normalize(TransformTangentToWorld(normalTS, half3x3(tangentWS, bitangentWS, normalWS)));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvMain) * _Color;
                clip(baseSample.a - _Cutoff);

                half3 normalWS = GetNormalWS(input);
                half4 specSample = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, input.uvSpec);
                half smoothnessFromTex = lerp(specSample.a, baseSample.a, saturate(_SmoothnessTextureChannel));
                half smoothness = saturate(_Glossiness * _GlossMapScale * smoothnessFromTex);
                half3 specColor = _SpecColor.rgb * specSample.rgb * _SpecularHighlights;
                half occlusion = lerp(1.0h, SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, input.uvOcclusion).g, _OcclusionStrength);
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uvEmission).rgb * _EmissionColor.rgb;

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));

                half3 color = baseSample.rgb * SampleSH(normalWS) * occlusion;
                color += baseSample.rgb * mainLight.color * ndotl * mainLight.shadowAttenuation * occlusion;

                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half specPower = lerp(16.0h, 256.0h, smoothness);
                half spec = pow(saturate(dot(normalWS, halfDir)), specPower) * mainLight.shadowAttenuation;
                color += specColor * mainLight.color * spec;

                #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    half lightNdotL = saturate(dot(normalWS, light.direction));
                    color += baseSample.rgb * light.color * lightNdotL * light.distanceAttenuation * light.shadowAttenuation * occlusion;
                }
                #endif

                color = MixFog(color + emission, input.fogFactor);
                return half4(color, baseSample.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SpecGlossMap_ST;
                float4 _BumpMap_ST;
                float4 _OcclusionMap_ST;
                float4 _EmissionMap_ST;
                half4 _Color;
                half _Cutoff;
                half _Glossiness;
                half _GlossMapScale;
                half _SmoothnessTextureChannel;
                half4 _SpecColor;
                half _SpecularHighlights;
                half _GlossyReflections;
                half _BumpScale;
                half _OcclusionStrength;
                half4 _EmissionColor;
                half _Mode;
                half _SrcBlend;
                half _DstBlend;
                half _ZWrite;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvMain : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uvMain = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvMain).a * _Color.a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
