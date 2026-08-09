// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:0,nrsp:0,vomd:0,spxs:False,tesm:1,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:8757,x:36098,y:32911,varname:node_8757,prsc:2|diff-8632-RGB,emission-9001-OUT,alpha-4368-OUT,refract-4366-OUT,voffset-4780-OUT;n:type:ShaderForge.SFN_Color,id:8632,x:35270,y:32677,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4779412,c2:0.7839757,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:7236,x:34337,y:33174,ptovrint:False,ptlb:Noise Texture,ptin:_NoiseTexture,varname:_NoiseTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:1,isnm:False|UVIN-7958-UVOUT;n:type:ShaderForge.SFN_ComponentMask,id:5916,x:34508,y:33174,varname:node_5916,prsc:2,cc1:0,cc2:0,cc3:-1,cc4:-1|IN-7236-RGB;n:type:ShaderForge.SFN_Slider,id:9479,x:34296,y:33432,ptovrint:False,ptlb:Refraction,ptin:_Refraction,varname:_Refraction,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:0.4700855,max:1;n:type:ShaderForge.SFN_Multiply,id:4366,x:34735,y:33309,varname:node_4366,prsc:2|A-5916-OUT,B-9479-OUT;n:type:ShaderForge.SFN_Panner,id:7958,x:34085,y:33135,varname:node_7958,prsc:0,spu:0.1,spv:0.1|UVIN-6266-UVOUT,DIST-722-OUT;n:type:ShaderForge.SFN_TexCoord,id:6266,x:33879,y:33135,varname:node_6266,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:6843,x:33751,y:33280,varname:node_6843,prsc:2;n:type:ShaderForge.SFN_Multiply,id:722,x:34018,y:33374,varname:node_722,prsc:2|A-6843-T,B-6770-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6770,x:33687,y:33494,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:_Speed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Time,id:194,x:30668,y:33931,varname:node_194,prsc:2;n:type:ShaderForge.SFN_FragmentPosition,id:1236,x:30845,y:34216,varname:node_1236,prsc:2;n:type:ShaderForge.SFN_Add,id:2593,x:31118,y:34234,varname:node_2593,prsc:2|A-1236-X,B-6018-OUT;n:type:ShaderForge.SFN_Sin,id:6551,x:31469,y:34234,varname:node_6551,prsc:2|IN-3009-OUT;n:type:ShaderForge.SFN_Multiply,id:1879,x:31652,y:34234,varname:node_1879,prsc:2|A-6551-OUT,B-8440-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8440,x:31469,y:34409,ptovrint:False,ptlb:WaveSpread,ptin:_WaveSpread,varname:_WaveSpread,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:1237,x:31836,y:34234,varname:node_1237,prsc:2|A-1879-OUT,B-3490-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3490,x:31652,y:34409,ptovrint:False,ptlb:WaveWidth,ptin:_WaveWidth,varname:_WaveWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_Multiply,id:3009,x:31301,y:34234,varname:node_3009,prsc:2|A-2593-OUT,B-7347-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7347,x:31118,y:34398,ptovrint:False,ptlb:WaveCount,ptin:_WaveCount,varname:_WaveCount,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_RemapRange,id:3351,x:32017,y:34234,varname:node_3351,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-1237-OUT;n:type:ShaderForge.SFN_Append,id:618,x:32619,y:34185,varname:node_618,prsc:2|A-6796-OUT,B-1607-OUT;n:type:ShaderForge.SFN_Append,id:4780,x:32823,y:34185,varname:node_4780,prsc:2|A-618-OUT,B-4246-OUT;n:type:ShaderForge.SFN_Vector1,id:6796,x:32420,y:34127,varname:node_6796,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:4246,x:32619,y:34326,varname:node_4246,prsc:2,v1:0;n:type:ShaderForge.SFN_Multiply,id:1607,x:32420,y:34246,varname:node_1607,prsc:2|A-5729-OUT,B-3672-OUT;n:type:ShaderForge.SFN_Slider,id:3672,x:31860,y:34436,ptovrint:False,ptlb:WaveHeight,ptin:_WaveHeight,varname:_WaveHeight,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.073,max:1;n:type:ShaderForge.SFN_Multiply,id:5729,x:32239,y:34168,varname:node_5729,prsc:2|A-8349-R,B-3351-OUT;n:type:ShaderForge.SFN_TexCoord,id:7816,x:31380,y:33914,varname:node_7816,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Panner,id:216,x:31723,y:34017,varname:node_216,prsc:2,spu:0.5,spv:0.5|UVIN-7816-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:8349,x:31996,y:33969,ptovrint:False,ptlb:WaveDistortionNoise,ptin:_WaveDistortionNoise,varname:_WaveDistortionNoise,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-216-UVOUT;n:type:ShaderForge.SFN_ValueProperty,id:2252,x:30682,y:34123,ptovrint:False,ptlb:WaveSpeed,ptin:_WaveSpeed,varname:_WaveSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_Multiply,id:6018,x:30896,y:34040,varname:node_6018,prsc:2|A-194-TSL,B-2252-OUT;n:type:ShaderForge.SFN_NormalVector,id:2150,x:32087,y:32651,prsc:2,pt:False;n:type:ShaderForge.SFN_Fresnel,id:6542,x:32353,y:32796,varname:node_6542,prsc:2|NRM-2150-OUT,EXP-9156-OUT;n:type:ShaderForge.SFN_Slider,id:9156,x:31996,y:32904,ptovrint:False,ptlb:Fresnel Amount,ptin:_FresnelAmount,varname:_FresnelAmount,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:7403,x:32142,y:33045,ptovrint:False,ptlb:Intensity,ptin:_Intensity,varname:_Intensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:4368,x:33103,y:32885,varname:node_4368,prsc:2|A-6542-OUT,B-7403-OUT;n:type:ShaderForge.SFN_Color,id:4832,x:34611,y:32304,ptovrint:False,ptlb:Emission Color 1,ptin:_EmissionColor1,varname:_EmissionColor1,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4779412,c2:0.7839757,c3:1,c4:1;n:type:ShaderForge.SFN_Color,id:9605,x:34517,y:32529,ptovrint:False,ptlb:Emission Color 2,ptin:_EmissionColor2,varname:_EmissionColor2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4779412,c2:0.7839757,c3:1,c4:1;n:type:ShaderForge.SFN_Lerp,id:9001,x:34866,y:32668,varname:node_9001,prsc:2|A-4832-RGB,B-9605-RGB,T-3331-OUT;n:type:ShaderForge.SFN_Clamp01,id:3331,x:34552,y:32780,varname:node_3331,prsc:2|IN-9555-OUT;n:type:ShaderForge.SFN_Add,id:9555,x:34313,y:32751,varname:node_9555,prsc:2|A-6751-OUT,B-9905-OUT;n:type:ShaderForge.SFN_Slider,id:6751,x:33947,y:32533,ptovrint:False,ptlb:Gradient,ptin:_Gradient,varname:_Gradient,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_ComponentMask,id:9905,x:33925,y:32737,varname:node_9905,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-3592-V;n:type:ShaderForge.SFN_TexCoord,id:3592,x:33679,y:32654,varname:node_3592,prsc:2,uv:0,uaff:False;proporder:8632-7236-9479-6770-8440-3490-7347-3672-8349-2252-9156-7403-4832-9605-6751;pass:END;sub:END;*/

Shader "Animmal/Water_ShaderMobileWebGL" {
    Properties {
        _Color ("Color", Color) = (0.4779412,0.7839757,1,1)
        _NoiseTexture ("Noise Texture", 2D) = "gray" {}
        _Refraction ("Refraction", Range(-1, 1)) = 0.4700855
        _Speed ("Speed", Float ) = 0
        _WaveSpread ("WaveSpread", Float ) = 0
        _WaveWidth ("WaveWidth", Float ) = 10
        _WaveCount ("WaveCount", Float ) = 10
        _WaveHeight ("WaveHeight", Range(0, 1)) = 0.073
        _WaveDistortionNoise ("WaveDistortionNoise", 2D) = "white" {}
        _WaveSpeed ("WaveSpeed", Float ) = 10
        _FresnelAmount ("Fresnel Amount", Range(0, 1)) = 0
        _Intensity ("Intensity", Range(0, 1)) = 0
        _EmissionColor1 ("Emission Color 1", Color) = (0.4779412,0.7839757,1,1)
        _EmissionColor2 ("Emission Color 2", Color) = (0.4779412,0.7839757,1,1)
        _Gradient ("Gradient", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform float4 _Color;
            uniform sampler2D _NoiseTexture; uniform float4 _NoiseTexture_ST;
            uniform float _Refraction;
            uniform float _Speed;
            uniform float _WaveSpread;
            uniform float _WaveWidth;
            uniform float _WaveCount;
            uniform float _WaveHeight;
            uniform sampler2D _WaveDistortionNoise; uniform float4 _WaveDistortionNoise_ST;
            uniform float _WaveSpeed;
            uniform float _FresnelAmount;
            uniform float _Intensity;
            uniform float4 _EmissionColor1;
            uniform float4 _EmissionColor2;
            uniform float _Gradient;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 projPos : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_8160 = _Time;
                float2 node_216 = (o.uv0+node_8160.g*float2(0.5,0.5));
                float4 _WaveDistortionNoise_var = tex2Dlod(_WaveDistortionNoise,float4(TRANSFORM_TEX(node_216, _WaveDistortionNoise),0.0,0));
                float4 node_194 = _Time;
                v.vertex.xyz += float3(float2(0.0,((_WaveDistortionNoise_var.r*(((sin(((mul(unity_ObjectToWorld, v.vertex).r+(node_194.r*_WaveSpeed))*_WaveCount))*_WaveSpread)+_WaveWidth)*0.5+0.5))*_WaveHeight)),0.0);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 node_6843 = _Time;
                fixed2 node_7958 = (i.uv0+(node_6843.g*_Speed)*float2(0.1,0.1));
                float4 _NoiseTexture_var = tex2D(_NoiseTexture,TRANSFORM_TEX(node_7958, _NoiseTexture));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (_NoiseTexture_var.rgb.rr*_Refraction);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 diffuseColor = _Color.rgb;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 emissive = lerp(_EmissionColor1.rgb,_EmissionColor2.rgb,saturate((_Gradient+i.uv0.g.r)));
/// Final Color:
                float3 finalColor = diffuse + emissive;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,(pow(1.0-max(0,dot(i.normalDir, viewDirection)),_FresnelAmount)*_Intensity)),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform float4 _Color;
            uniform sampler2D _NoiseTexture; uniform float4 _NoiseTexture_ST;
            uniform float _Refraction;
            uniform float _Speed;
            uniform float _WaveSpread;
            uniform float _WaveWidth;
            uniform float _WaveCount;
            uniform float _WaveHeight;
            uniform sampler2D _WaveDistortionNoise; uniform float4 _WaveDistortionNoise_ST;
            uniform float _WaveSpeed;
            uniform float _FresnelAmount;
            uniform float _Intensity;
            uniform float4 _EmissionColor1;
            uniform float4 _EmissionColor2;
            uniform float _Gradient;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 projPos : TEXCOORD3;
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_9028 = _Time;
                float2 node_216 = (o.uv0+node_9028.g*float2(0.5,0.5));
                float4 _WaveDistortionNoise_var = tex2Dlod(_WaveDistortionNoise,float4(TRANSFORM_TEX(node_216, _WaveDistortionNoise),0.0,0));
                float4 node_194 = _Time;
                v.vertex.xyz += float3(float2(0.0,((_WaveDistortionNoise_var.r*(((sin(((mul(unity_ObjectToWorld, v.vertex).r+(node_194.r*_WaveSpeed))*_WaveCount))*_WaveSpread)+_WaveWidth)*0.5+0.5))*_WaveHeight)),0.0);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 node_6843 = _Time;
                fixed2 node_7958 = (i.uv0+(node_6843.g*_Speed)*float2(0.1,0.1));
                float4 _NoiseTexture_var = tex2D(_NoiseTexture,TRANSFORM_TEX(node_7958, _NoiseTexture));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (_NoiseTexture_var.rgb.rr*_Refraction);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 diffuseColor = _Color.rgb;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor * (pow(1.0-max(0,dot(i.normalDir, viewDirection)),_FresnelAmount)*_Intensity),0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float _WaveSpread;
            uniform float _WaveWidth;
            uniform float _WaveCount;
            uniform float _WaveHeight;
            uniform sampler2D _WaveDistortionNoise; uniform float4 _WaveDistortionNoise_ST;
            uniform float _WaveSpeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                float4 node_4552 = _Time;
                float2 node_216 = (o.uv0+node_4552.g*float2(0.5,0.5));
                float4 _WaveDistortionNoise_var = tex2Dlod(_WaveDistortionNoise,float4(TRANSFORM_TEX(node_216, _WaveDistortionNoise),0.0,0));
                float4 node_194 = _Time;
                v.vertex.xyz += float3(float2(0.0,((_WaveDistortionNoise_var.r*(((sin(((mul(unity_ObjectToWorld, v.vertex).r+(node_194.r*_WaveSpeed))*_WaveCount))*_WaveSpread)+_WaveWidth)*0.5+0.5))*_WaveHeight)),0.0);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
