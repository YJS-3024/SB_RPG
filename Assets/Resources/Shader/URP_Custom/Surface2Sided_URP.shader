Shader "URP_Custom/Surface 2 Sided_URP"
{
    Properties
    {
        _Emission("Emission", 2D) = "white" {}
        _Albedo("Albedo", 2D) = "white" {}
        _Specular("Specular ", 2D) = "white" {}
        _EmissionPower("Emission Power", Range(0, 3)) = 0
        _Specular_Intensity("Specular_Intensity", Range(0, 3)) = 0
        _Normal("Normal", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
            "IsEmissive" = "true"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
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

            TEXTURE2D(_Emission); SAMPLER(sampler_Emission);
            TEXTURE2D(_Albedo); SAMPLER(sampler_Albedo);
            TEXTURE2D(_Specular); SAMPLER(sampler_Specular);
            TEXTURE2D(_Normal); SAMPLER(sampler_Normal);

            CBUFFER_START(UnityPerMaterial)
                float4 _Emission_ST;
                float4 _Albedo_ST;
                float4 _Specular_ST;
                float4 _Normal_ST;
                half _EmissionPower;
                half _Specular_Intensity;
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
                float2 uvEmission : TEXCOORD3;
                float2 uvAlbedo : TEXCOORD4;
                float2 uvSpecular : TEXCOORD5;
                float2 uvNormal : TEXCOORD6;
                half fogFactor : TEXCOORD7;
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
                output.uvEmission = TRANSFORM_TEX(input.uv, _Emission);
                output.uvAlbedo = TRANSFORM_TEX(input.uv, _Albedo);
                output.uvSpecular = TRANSFORM_TEX(input.uv, _Specular);
                output.uvNormal = TRANSFORM_TEX(input.uv, _Normal);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half3 GetNormalWS(Varyings input)
            {
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, input.uvNormal));
                half3 normalWS = normalize(input.normalWS);
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = cross(normalWS, tangentWS) * input.tangentWS.w;
                return normalize(TransformTangentToWorld(normalTS, half3x3(tangentWS, bitangentWS, normalWS)));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo, input.uvAlbedo).rgb;
                half3 emission = SAMPLE_TEXTURE2D(_Emission, sampler_Emission, input.uvEmission).rgb * _EmissionPower;
                half specularMask = SAMPLE_TEXTURE2D(_Specular, sampler_Specular, input.uvSpecular).r;
                half specularStrength = saturate(specularMask * _Specular_Intensity / 3.0h);
                half3 normalWS = GetNormalWS(input);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));

                half3 color = albedo * SampleSH(normalWS);
                color += albedo * mainLight.color * ndotl * mainLight.shadowAttenuation;

                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), 64.0h) * specularStrength;
                color += mainLight.color * spec * mainLight.shadowAttenuation;

                #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    half lightNdotL = saturate(dot(normalWS, light.direction));
                    color += albedo * light.color * lightNdotL * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                color = MixFog(color + emission, input.fogFactor);
                return half4(color, 1);
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
