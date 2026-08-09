Shader "URP_Custom/Surface2_URP"
{
    Properties
    {
        _Albedo("Albedo", 2D) = "white" {}
        _Alpha("Alpha", 2D) = "white" {}
        _Cutoff("Mask Clip Value", Float) = 0
        _Specular("Specular", Range(0, 3)) = 0
        _Smoothness("Smoothness", Range(0, 3)) = 0
        _Normal("Normal", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
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

            TEXTURE2D(_Albedo); SAMPLER(sampler_Albedo);
            TEXTURE2D(_Alpha); SAMPLER(sampler_Alpha);
            TEXTURE2D(_Normal); SAMPLER(sampler_Normal);

            CBUFFER_START(UnityPerMaterial)
                float4 _Albedo_ST;
                float4 _Alpha_ST;
                float4 _Normal_ST;
                half _Cutoff;
                half _Specular;
                half _Smoothness;
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
                float2 uvAlbedo : TEXCOORD3;
                float2 uvAlpha : TEXCOORD4;
                float2 uvNormal : TEXCOORD5;
                half fogFactor : TEXCOORD6;
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
                output.uvAlbedo = TRANSFORM_TEX(input.uv, _Albedo);
                output.uvAlpha = TRANSFORM_TEX(input.uv, _Alpha);
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
                half alphaMask = SAMPLE_TEXTURE2D(_Alpha, sampler_Alpha, input.uvAlpha).r;
                clip(alphaMask - _Cutoff);

                half3 albedo = SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo, input.uvAlbedo).rgb;
                half3 normalWS = GetNormalWS(input);
                half smoothness = saturate(_Smoothness / 3.0h);
                half specularStrength = saturate(_Specular / 3.0h);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 color = albedo * SampleSH(normalWS);
                color += albedo * mainLight.color * ndotl * mainLight.shadowAttenuation;

                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), lerp(8.0h, 128.0h, smoothness)) * specularStrength;
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

                color = MixFog(color, input.fogFactor);
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

            TEXTURE2D(_Alpha); SAMPLER(sampler_Alpha);

            CBUFFER_START(UnityPerMaterial)
                float4 _Albedo_ST;
                float4 _Alpha_ST;
                float4 _Normal_ST;
                half _Cutoff;
                half _Specular;
                half _Smoothness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvAlpha : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uvAlpha = TRANSFORM_TEX(input.uv, _Alpha);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half alphaMask = SAMPLE_TEXTURE2D(_Alpha, sampler_Alpha, input.uvAlpha).r;
                clip(alphaMask - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
