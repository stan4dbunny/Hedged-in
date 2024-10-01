Shader "Unlit/Wall"
{
    Properties
    {
        
        _SpecularColor("SpecularColor", Color) = (0,0,0,0)
        _SpecularPower("SpecularPower", Range(0.0, 500.0)) = 0.0
        _SpecularStrength("SpecularStrength", Range(0.0, 1.0)) = 0.0
        _AmbientColor("AmbientColor", Color) = (0,0,0,0)
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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


            float4 _MainTex_ST;

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

                
                o.uv = v.uv;
                
                VertexPositionInputs posInfo = GetVertexPositionInputs(v.vertex.xyz);

                //set interpolator stuff used for lighting
                o.worldPos = posInfo.positionWS;
                o.worldNormal = GetVertexNormalInputs(v.normal, v.tangent).normalWS;

                o.pos = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float4 pixelColor = float4(1,0,0,0);

                
                //float4 albedo = tex2D(_AlbedoTex, i.uv);
                float4 albedo = float4(0.1,0.9,0.1,0);
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
                
                return pixelColor*0.4;
            }
            ENDHLSL
        }
    }
}