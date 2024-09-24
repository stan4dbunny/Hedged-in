Shader "Unlit/SSS"
{
    Properties
    {
        _Wrap("Wrap", float) = 0.2
        _ScatterWidth("ScatterWidth", float) = 0.3
        _ScatterColor("ScatterColor", Color) = (0.15,0,0,1.0)
        _Shininess("Shininess", float) = 40.0

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

            UNITY_INSTANCING_BUFFER_START(PerInstance)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurrLayerIndex)
            UNITY_INSTANCING_BUFFER_END(PerInstance)
      
            float _Wrap;
            float _ScatterWidth;
            float4 _ScatterColor;
            float _Shininess;

            Interpolators vert (MeshData v)
            {
                Interpolators o;

                UNITY_SETUP_INSTANCE_ID(v); 
                ZERO_INITIALIZE(Interpolators, o); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float currLayerIndex = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _CurrLayerIndex);
                o.uv = v.uv;
                VertexPositionInputs posInfo = GetVertexPositionInputs(v.vertex.xyz);

                //set interpolator stuff used for lighting
                o.worldPos = posInfo.positionWS;
                o.worldNormal = GetVertexNormalInputs(v.normal, v.tangent).normalWS;

                //translate 
               //o.worldPos += o.worldNormal * trnsl;

                o.pos = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float currLayerIndex = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _CurrLayerIndex);
                
                float3 viewAngleDir = GetWorldSpaceNormalizeViewDir(i.worldPos);
                float3 lightDir = GetMainLight().direction;
                //float4 lightColor = float4(GetMainLight().color, 0.0);
                float3 halfAngleDir = normalize(normalize(lightDir) + normalize(viewAngleDir));

                float NDotL = max(0 , dot(i.worldNormal, lightDir));
                float NDotH = max(0, dot(i.worldNormal, halfAngleDir));

                float NDotLWrap = (NDotL + _Wrap) / (1 + _Wrap); //wrap Lighting
                float diffuse = max(NDotLWrap, 0.0);

                //fake gaussian distribution, transition light to dark
                float scatter = smoothstep(0.0, _ScatterWidth, NDotLWrap) * smoothstep(_ScatterWidth * 2, _ScatterWidth, NDotLWrap);
                float specular = pow(NDotH, _Shininess);

                if (NDotLWrap <= 0) specular = 0; //why?
                float4 clr = float4(diffuse + scatter * _ScatterColor);
                clr.a = specular;

                return clr;
            }
            ENDHLSL
        }
    }
}