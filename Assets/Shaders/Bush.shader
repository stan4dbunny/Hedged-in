Shader "Unlit/FurShader"
{
    Properties
    {
        _DroopStrength("DroopStrength", float) = 0.0
        _SpecularColor("SpecularColor", Color) = (0,0,0,0)
        _SpecularPower("SpecularPower", Range(0.0, 500.0)) = 0.0
        _SpecularStrength("SpecularStrength", Range(0.0, 1.0)) = 0.0
        _AmbientColor("AmbientColor", Color) = (0,0,0,0)
        _Resolution("Resolution", int) = 0
        _Thickness("Thickness", float) = 0.0
        _Height("Height", float) = 0.0
        _Layers("Layers", int) = 0
        _MinHeight("MinHeight", float) = 0.0
        [PerRendererData] _CurrLayerIndex("CurrLayerIndex", float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Tags { "LightMode"="UniversalForward"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            //#include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "UnityLightingCommon.cginc"
           

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float height : TEXCOORD1; 
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_INSTANCING_BUFFER_START(PerInstance)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurrLayerIndex)
            UNITY_INSTANCING_BUFFER_END(PerInstance)

            float4 _MainTex_ST;
            int  _Resolution;
            float _Thickness;
            float _Height;
            int _Layers;
            float _MinHeight;
            float _DroopStrength;

            float4 _SpecularColor;
            float _SpecularPower;
            float _SpecularStrength;
            float4 _AmbientColor;



            Interpolators vert (MeshData v)
            {
                Interpolators o;

                UNITY_SETUP_INSTANCE_ID(v); 
                ZERO_INITIALIZE(Interpolators, o); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float currLayerIndex = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _CurrLayerIndex);
                o.uv = v.uv;
                float trnsl = _Height*(currLayerIndex/_Layers); //calculates translation of the vertex
                o.height = trnsl;
                
                VertexPositionInputs posInfo = GetVertexPositionInputs(v.vertex.xyz);

                //set interpolator stuff used for lighting
                o.worldPos = posInfo.positionWS;
                o.worldNormal = GetVertexNormalInputs(v.normal, v.tangent).normalWS;

                //translate 
                o.worldPos += o.worldNormal * trnsl;

                float droopStrength = (currLayerIndex/_Layers)*(currLayerIndex/_Layers)*_DroopStrength;
                float3 droopAtRest = float3(0,-1,0)*droopStrength;

                o.worldPos += float4(droopAtRest, 0);

                o.pos = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float hash12(float2 p)
            {
	            float3 p3  = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float4 frag (Interpolators i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float currLayerIndex = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _CurrLayerIndex);
                //return float4(currLayerIndex, currLayerIndex, currLayerIndex, currLayerIndex);
                //generates random nr 0-1  
                float PHI = 1.61803398874989484820459;
                //float randFloat = frac(sin(dot(trunc(i.uv*_Resolution), float2(12.9898, 78.233))) * 43758.5453);
                //float randFloat = frac(tan(distance(trunc(i.uv*_Resolution)*PHI, trunc(i.uv*_Resolution))*0.5)*trunc(i.uv.x*_Resolution));
                float randFloat = hash12(trunc(i.uv*_Resolution));
                float randHeight = lerp(_MinHeight, _Height, randFloat);
                float4 pixelColor = float4(1,0,0,0);

                //centered pixel coordinates
                float2 cntrdPixlCoord = frac(_Resolution * i.uv) * 2.0 - 1.0;
                //distance to center of pixel
                float dist = length(cntrdPixlCoord);

                float currLayerHeight = lerp(_MinHeight, _Height, currLayerIndex/_Layers);
                
                //discard pixels below height threshold, z
                if (randHeight  < currLayerHeight) discard;

                //discard pixels outside cylinder, x and y
                if (dist > _Thickness * (randHeight - currLayerHeight)) discard; //the higher we get in the layers the thinner the strand should be
                else
                {
                    //float4 albedo = tex2D(_AlbedoTex, i.uv);
                    float4 albedo = float4(0.1,0.9,0.1,0);
                    //_WorldSpaceLightPos0 built in variable, is a direction if directional light
                    float3 viewAngleDir = GetWorldSpaceNormalizeViewDir(i.worldPos);
                    float3 lightDir = GetMainLight().direction;
                    float4 lightColor = float4(GetMainLight().color, 0.0);
                    float3 halfAngleDir = normalize(normalize(lightDir) + normalize(viewAngleDir));

                    //no negative
                    float cosAngle = max(0.0, dot(halfAngleDir, i.worldNormal));
                    float4 specular = lightColor * _SpecularColor * pow(cosAngle, _SpecularPower)*_SpecularStrength;
                    float4 diffuse = albedo * lightColor * max(dot(i.worldNormal, lightDir), 0);
                    float4 ambient = albedo * float4(SampleSH(i.worldNormal),0);
                    pixelColor = specular + ambient + diffuse;
                }
                return pixelColor * lerp(0.2, 1.0, (currLayerIndex/_Layers));
            }
            ENDHLSL
        }
    }
}