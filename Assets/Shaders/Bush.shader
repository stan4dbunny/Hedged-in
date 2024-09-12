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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
           

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
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
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float currLayerIndex = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _CurrLayerIndex);
                o.uv = v.uv;
                float trnsl = currLayerIndex*(_Height/_Layers); //calculates translation of the vertex
                o.height = trnsl;
                
                //set interpolator stuff used for lighting
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                //translate 
                o.worldPos += float4(o.worldNormal,0)*trnsl;

                float droopStrength = (currLayerIndex/_Layers)*(currLayerIndex/_Layers)*_DroopStrength;
                float3 droopAtRest = float3(0,-1,0)*droopStrength;

                o.worldPos += float4(droopAtRest, 0);
               
                //transform from model to clip space?
                o.pos = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0));
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float currLayerIndex = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _CurrLayerIndex);
                //return float4(currLayerIndex, currLayerIndex, currLayerIndex, currLayerIndex);
                //generates random nr 0-1  
                float randFloat = frac(sin(dot(trunc(i.uv*_Resolution), float2(12.9898, 78.233))) * 43758.5453); //trunc gets the int part which groups the pixels together to one strand

                float4 pixelColor = float4(0,1,0,0);

                //centered pixel coordinates
                float2 cntrdPixlCoord = frac(_Resolution * i.uv) * 2.0 - 1.0;
                //distance to center of pixel
                float dist = length(cntrdPixlCoord);

               if (randFloat < _MinHeight) randFloat = lerp(_MinHeight, _Height, randFloat);
                
                //discard pixels below height threshold, z
                if (randFloat  < (currLayerIndex/_Layers)) discard;

                //discard pixels outside cylinder, x and y
                if (dist > _Thickness * (randFloat - (currLayerIndex/_Layers))) discard; //the higher we get in the layers the thinner the strand should be
                
                else
                {
                    //float4 albedo = tex2D(_AlbedoTex, i.uv);
                    float4 albedo = float4(0,1,0,0);
                    //_WorldSpaceLightPos0 built in variable, is a direction if directional light
                    float3 viewAngleDir = WorldSpaceViewDir(float4(i.worldPos,0));
                    float3 halfAngleDir = normalize(normalize(_WorldSpaceLightPos0) + normalize(viewAngleDir));

                    //no negative
                    float cosAngle = max(0.0, dot(halfAngleDir, i.worldNormal));
                    float4 specular = _LightColor0 * _SpecularColor * pow(cosAngle, _SpecularPower)*_SpecularStrength;
                    //float4 ambient = albedo * _AmbientColor;
                    float4 ambient = float4(ShadeSH9(float4(i.worldNormal,1)),0);
                    float4 diffuse = albedo * _LightColor0 * dot(i.worldNormal, _WorldSpaceLightPos0);
                    pixelColor = specular  + diffuse;
                }
                return pixelColor * (currLayerIndex/_Layers);
            }
            ENDCG
        }
    }
}