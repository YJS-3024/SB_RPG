Shader "URP_Custom/Mobile_BumpedDiffuse_URP"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BumpMap("Normalmap", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 150

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                half4 _Color;
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
                float2 uvBump : TEXCOORD4;
                half fogFactor : TEXCOORD5;
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
                output.uvBump = TRANSFORM_TEX(input.uv, _BumpMap);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half3 GetNormalWS(Varyings input)
            {
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uvBump));
                half3 normalWS = normalize(input.normalWS);
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = cross(normalWS, tangentWS) * input.tangentWS.w;
                return normalize(TransformTangentToWorld(normalTS, half3x3(tangentWS, bitangentWS, normalWS)));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvMain).rgb * _Color.rgb;
                half3 normalWS = GetNormalWS(input);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));

                half3 color = albedo * SampleSH(normalWS);
                color += albedo * mainLight.color * ndotl * mainLight.shadowAttenuation;

                #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    half lightNdotL = saturate(dot(normalWS, light.direction));
                    color += albedo * light.color * lightNdotL * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                color = MixFog(color, input.fogFactor);
                return half4(color, _Color.a);
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
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
