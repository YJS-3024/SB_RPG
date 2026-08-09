Shader "URP_Custom/Vertical Fog_URP"
{
    Properties
    {
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        _EmissionColor("Emission Color", Color) = (1, 1, 1, 1)
        _Float0("Bottom Height", Range(0, 1)) = 0
        _Float1("Top Height", Range(0, 1)) = 1
        _Color0("Color", Color) = (0, 0, 0, 0.5)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _AlphaCutoff;
                half4 _EmissionColor;
                half _Float0;
                half _Float1;
                half4 _Color0;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half minHeight = min(_Float0, _Float1);
                half maxHeight = max(_Float0, _Float1);
                half rangeHeight = max(maxHeight - minHeight, 0.0001h);
                half mask = saturate((input.positionWS.y - minHeight) / rangeHeight);
                half alpha = _Color0.a * mask;
                clip(alpha - _AlphaCutoff);

                half3 color = _Color0.rgb * _EmissionColor.rgb;
                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
