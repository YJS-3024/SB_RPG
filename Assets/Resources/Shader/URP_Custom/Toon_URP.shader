Shader "URP_Custom/Toon_URP"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _MainTex("Main Tex", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Color("Color", Color) = (1, 1, 1, 1)

        _1st_ShadeMap("1st Shade Map", 2D) = "white" {}
        _2nd_ShadeMap("2nd Shade Map", 2D) = "white" {}
        [Toggle(_)] _Use_BaseAs1st("Use Base Map as 1st Shade Map", Integer) = 1
        [Toggle(_)] _Use_1stAs2nd("Use 1st Shade Map as 2nd Shade Map", Integer) = 1
        _1st_ShadeColor("1st Shade Color", Color) = (0.75, 0.75, 0.75, 1)
        _2nd_ShadeColor("2nd Shade Color", Color) = (0.55, 0.55, 0.55, 1)
        _BaseTo1st_ShadeStart("Base to 1st Shade Start", Range(0, 1)) = 0.55
        _BaseTo1st_ShadeFeather("Base to 1st Shade Feather", Range(0.001, 1)) = 0.05
        _1stTo2nd_ShadeStart("1st to 2nd Shade Start", Range(0, 1)) = 0.25
        _1stTo2nd_ShadeFeather("1st to 2nd Shade Feather", Range(0.001, 1)) = 0.05
        _1st_ShadeColor_Step("1st Shade Step", Range(0, 1)) = 0.55
        _1st_ShadeColor_Feather("1st Shade Feather", Range(0.001, 1)) = 0.05
        _2nd_ShadeColor_Step("2nd Shade Step", Range(0, 1)) = 0.25
        _2nd_ShadeColor_Feather("2nd Shade Feather", Range(0.001, 1)) = 0.05

        _Clipping_Level("Clipping Level", Range(0, 1)) = 0
        _ClippingMask("Clipping Mask", 2D) = "white" {}

        _Emissive_Tex("Emissive Texture", 2D) = "white" {}
        _Emissive_Color("Emissive Color", Color) = (0, 0, 0, 1)
        _EmissiveIntensity("Emissive Intensity", Float) = 1

        _2DLightStrength("2D Light Strength", Range(0, 1)) = 1
        [Toggle(_)] _DirectionalLight_Use("Use Directional Light", Integer) = 0
        _DirectionalLight_Direction("Directional Light Direction", Vector) = (0, -1, 0, 0)
        _DirectionalLight_Color("Directional Light Color", Color) = (1, 1, 1, 1)
        _DirectionalLight_Intensity("Directional Light Intensity", Float) = 0.5
        _DirectionalLight_DiffuseStrength("Directional Light Diffuse Strength", Range(0, 1)) = 0.5
        _DirectionalLight_ViewPosition("Directional Light View Position", Vector) = (0, 0, 1, 0)
        _HighlightTex("Highlight Map", 2D) = "white" {}
        _HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
        _DirectionalLight_HighlightMode("Directional Light Highlight Mode", Integer) = 0
        _DirectionalLight_HighlightStrength("Directional Light Highlight Strength", Range(0, 1)) = 0.5
        _DirectionalLight_HighlightSize("Directional Light Highlight Size", Range(0, 1)) = 0.3
        _NormalMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0, 1)) = 1

        _OutlineMode("Outline Mode", Integer) = 0
        _OutlineWidth("Outline Width", Float) = 5
        _OutlineWidthMap("Outline Width Map", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0.1, 0.1, 0.1, 1)
        _OutlineTex("Outline Map", 2D) = "white" {}
        _Outline_BaseColorBlend("Blend Base Color to Outline", Range(0, 1)) = 0.5
        _Outline_LightColorBlend("Blend Light Color to Outline", Range(0, 1)) = 0.5
        _OutlineOffsetZ("Outline Z Offset", Float) = 0.75
        _OutlineNear("Outline Near", Float) = 0.5
        _OutlineFar("Outline Far", Float) = 100
        [Toggle(_)] _Outline_UseNormalMap("Use Outline Normal Map", Integer) = 0
        _Outline_NormalMap("Outline Normal Map", 2D) = "bump" {}

        [IntRange] _StencilRef("Stencil Reference", Range(0, 255)) = 128
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Compare", Integer) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOpPass("Stencil Pass Op", Integer) = 2
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOpFail("Stencil Pass Op Fail", Integer) = 0

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
                int _Use_BaseAs1st;
                int _Use_1stAs2nd;
                half _BaseTo1st_ShadeStart;
                half _BaseTo1st_ShadeFeather;
                half _1stTo2nd_ShadeStart;
                half _1stTo2nd_ShadeFeather;
                half _1st_ShadeColor_Step;
                half _1st_ShadeColor_Feather;
                half _2nd_ShadeColor_Step;
                half _2nd_ShadeColor_Feather;
                half _Clipping_Level;
                half4 _Emissive_Color;
                half _EmissiveIntensity;
                half _OutlineWidth;
                half4 _OutlineColor;
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
                half toonLight = saturate(ndotl * mainLight.shadowAttenuation);

                half firstBand = smoothstep(_BaseTo1st_ShadeStart - _BaseTo1st_ShadeFeather, _BaseTo1st_ShadeStart + _BaseTo1st_ShadeFeather, toonLight);
                half secondBand = smoothstep(_1stTo2nd_ShadeStart - _1stTo2nd_ShadeFeather, _1stTo2nd_ShadeStart + _1stTo2nd_ShadeFeather, toonLight);
                half3 toon = lerp(shade2, shade1, secondBand);
                toon = lerp(toon, baseColor, firstBand);

                half3 lit = toon * mainLight.color;
                lit += toon * SampleSH(normalWS);

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

                half3 shadeMap1 = SAMPLE_TEXTURE2D(_1st_ShadeMap, sampler_1st_ShadeMap, input.uv).rgb;
                shadeMap1 = lerp(shadeMap1, baseSample.rgb, saturate(_Use_BaseAs1st));
                half3 shadeMap2 = SAMPLE_TEXTURE2D(_2nd_ShadeMap, sampler_2nd_ShadeMap, input.uv).rgb;
                shadeMap2 = lerp(shadeMap2, shadeMap1, saturate(_Use_1stAs2nd));
                half3 shade1 = shadeMap1 * _1st_ShadeColor.rgb;
                half3 shade2 = shadeMap2 * _2nd_ShadeColor.rgb;
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
                half _OutlineWidth;
                half4 _OutlineColor;
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
                float3 positionOS = input.positionOS.xyz + normalize(input.normalOS) * (_OutlineWidth * 0.001);
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
                half3 color = MixFog(_OutlineColor.rgb, input.fogFactor);
                return half4(color, _OutlineColor.a);
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
                half _OutlineWidth;
                half4 _OutlineColor;
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

    CustomEditor "UnityEditor.Rendering.Toon.UnityToon3Das2DGUI"

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
