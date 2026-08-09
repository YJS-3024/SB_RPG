// URP-compatible fallback for materials that previously used UTS Toon(Tessellation).
// Tessellation stages are intentionally omitted for URP/mobile compatibility.
Shader "URP_Custom/Toon(Tessellation)_URP"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _MainTex("Main Tex", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Color("Color", Color) = (1, 1, 1, 1)

        _1st_ShadeMap("1st Shade Map", 2D) = "white" {}
        _2nd_ShadeMap("2nd Shade Map", 2D) = "white" {}
        _1st_ShadeColor("1st Shade Color", Color) = (0.75, 0.75, 0.75, 1)
        _2nd_ShadeColor("2nd Shade Color", Color) = (0.55, 0.55, 0.55, 1)
        _1st_ShadeColor_Step("1st Shade Step", Range(0, 1)) = 0.55
        _1st_ShadeColor_Feather("1st Shade Feather", Range(0.001, 1)) = 0.05
        _2nd_ShadeColor_Step("2nd Shade Step", Range(0, 1)) = 0.25
        _2nd_ShadeColor_Feather("2nd Shade Feather", Range(0.001, 1)) = 0.05

        _Clipping_Level("Clipping Level", Range(0, 1)) = 0
        _ClippingMask("Clipping Mask", 2D) = "white" {}

        _Emissive_Tex("Emissive Texture", 2D) = "white" {}
        _Emissive_Color("Emissive Color", Color) = (0, 0, 0, 1)
        _EmissiveIntensity("Emissive Intensity", Float) = 1

        _Outline_Width("Outline Width", Range(0, 10)) = 0
        _Outline_Color("Outline Color", Color) = (0, 0, 0, 1)

        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual

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

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_1st_ShadeMap); SAMPLER(sampler_1st_ShadeMap);
            TEXTURE2D(_2nd_ShadeMap); SAMPLER(sampler_2nd_ShadeMap);
            TEXTURE2D(_ClippingMask); SAMPLER(sampler_ClippingMask);
            TEXTURE2D(_Emissive_Tex); SAMPLER(sampler_Emissive_Tex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _MainTex_ST;
                half4 _BaseColor;
                half4 _Color;
                half4 _1st_ShadeColor;
                half4 _2nd_ShadeColor;
                half _1st_ShadeColor_Step;
                half _1st_ShadeColor_Feather;
                half _2nd_ShadeColor_Step;
                half _2nd_ShadeColor_Feather;
                half _Clipping_Level;
                half4 _Emissive_Color;
                half _EmissiveIntensity;
                half _Outline_Width;
                half4 _Outline_Color;
                half _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half3 ApplyToonLighting(half3 baseColor, half3 shade1, half3 shade2, half3 normalWS, float3 positionWS)
            {
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction) * 0.5h + 0.5h);

                half firstBand = smoothstep(_1st_ShadeColor_Step - _1st_ShadeColor_Feather, _1st_ShadeColor_Step + _1st_ShadeColor_Feather, ndotl);
                half secondBand = smoothstep(_2nd_ShadeColor_Step - _2nd_ShadeColor_Feather, _2nd_ShadeColor_Step + _2nd_ShadeColor_Feather, ndotl);
                half3 toon = lerp(shade2, shade1, secondBand);
                toon = lerp(toon, baseColor, firstBand);

                half3 lit = toon * mainLight.color * mainLight.shadowAttenuation;
                lit += baseColor * SampleSH(normalWS);

                #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, positionWS);
                    half lightNdotL = saturate(dot(normalWS, light.direction));
                    lit += baseColor * light.color * lightNdotL * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                return lit;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor * _Color;
                half clipMask = SAMPLE_TEXTURE2D(_ClippingMask, sampler_ClippingMask, input.uv).r;
                clip(baseSample.a * clipMask - _Clipping_Level);

                half3 shade1 = baseSample.rgb * _1st_ShadeColor.rgb * SAMPLE_TEXTURE2D(_1st_ShadeMap, sampler_1st_ShadeMap, input.uv).rgb;
                half3 shade2 = baseSample.rgb * _2nd_ShadeColor.rgb * SAMPLE_TEXTURE2D(_2nd_ShadeMap, sampler_2nd_ShadeMap, input.uv).rgb;
                half3 color = ApplyToonLighting(baseSample.rgb, shade1, shade2, normalize(input.normalWS), input.positionWS);

                half3 emission = SAMPLE_TEXTURE2D(_Emissive_Tex, sampler_Emissive_Tex, input.uv).rgb * _Emissive_Color.rgb * _EmissiveIntensity;
                color = MixFog(color + emission, input.fogFactor);
                return half4(color, baseSample.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_ClippingMask); SAMPLER(sampler_ClippingMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _MainTex_ST;
                half4 _BaseColor;
                half4 _Color;
                half4 _1st_ShadeColor;
                half4 _2nd_ShadeColor;
                half _1st_ShadeColor_Step;
                half _1st_ShadeColor_Feather;
                half _2nd_ShadeColor_Step;
                half _2nd_ShadeColor_Feather;
                half _Clipping_Level;
                half4 _Emissive_Color;
                half _EmissiveIntensity;
                half _Outline_Width;
                half4 _Outline_Color;
                half _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz + normalize(input.normalOS) * (_Outline_Width * 0.001);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.positionCS = positionInputs.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * SAMPLE_TEXTURE2D(_ClippingMask, sampler_ClippingMask, input.uv).r;
                clip(alpha - _Clipping_Level);
                half3 color = MixFog(_Outline_Color.rgb, input.fogFactor);
                return half4(color, _Outline_Color.a);
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
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_ClippingMask); SAMPLER(sampler_ClippingMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _MainTex_ST;
                half4 _BaseColor;
                half4 _Color;
                half4 _1st_ShadeColor;
                half4 _2nd_ShadeColor;
                half _1st_ShadeColor_Step;
                half _1st_ShadeColor_Feather;
                half _2nd_ShadeColor_Step;
                half _2nd_ShadeColor_Feather;
                half _Clipping_Level;
                half4 _Emissive_Color;
                half _EmissiveIntensity;
                half _Outline_Width;
                half4 _Outline_Color;
                half _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                half3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * SAMPLE_TEXTURE2D(_ClippingMask, sampler_ClippingMask, input.uv).r;
                clip(alpha - _Clipping_Level);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
