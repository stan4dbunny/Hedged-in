Shader "Unlit/FurShader"
{
    Properties
    {
        _MainTex ("RandTexture", 2D) = "white" {}
        _AlbedoTex ("Texture", 2D) = "white" {}
        _DroopStrength("DroopStrength", float) = 0.0
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
           

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                //float4 vertex : SV_POSITION0;
                float height : TEXCOORD1;
                
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _AlbedoTex;
            float4 _MainTex_ST;

            float _PrevHeight;
            int  _Resolution;
            float _Thickness;
            float _CurrHeight;
            float _Height;
            int _Layers;
           
            float _DroopStrength;

            float4 _SpecularColor;
            float _SpecularPower;
            float _SpecularStrength;
            float4 _AmbientColor;

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); //scaling, offsetting uv coordinates
                float trnsl = _CurrHeight*(_Height/_Layers); //calculates translation of the vertex
                o.height = trnsl;
                
                //set interpolator stuff used for lighting
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                o.worldPos = o.worldPos + float4(o.worldNormal,0)*trnsl;

                float droopStrength = (_CurrHeight/_Layers)*(_CurrHeight/_Layers)*_DroopStrength;
                float3 droopAtRest = float3(0,-1,0)*droopStrength;

                o.worldPos += float4(droopAtRest, 0);
               
                o.pos = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0));
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
               
                // sample the texture
                //float4 rand = tex2D(_MainTex, i.uv);   
                //float randVal = rand.x;

                float randFloat = frac(sin(dot(i.uv*_Resolution, float2(12.9898, 78.233))) * 43758.5453);

                float4 pixelColor = float4(randFloat, randFloat, randFloat, randFloat);
                //return pixelColor;

                float2 cntrdPixlCoord = frac(_Resolution * i.uv) * 2.0 - 1.0;
                float dist = length(cntrdPixlCoord);
                //if (i.height < 0.001) discard;

                 //discard pixels below height threshold, z
                if (randFloat  < _PrevHeight) discard;
                //return float4(0,1,0,0)*_PrevHeight;
     
                //discard pixels outside cylinder, x and y
                if (dist > _Thickness * (randFloat - _PrevHeight)) discard;
     
               

  
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
                    //float4 ambient = float4(half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w),0);
                    float4 diffuse = albedo * _LightColor0 * dot(i.worldNormal, _WorldSpaceLightPos0);
                    pixelColor = specular  + diffuse;
                }
                
                return pixelColor * _PrevHeight;
            }
            
            ENDCG
        }
    }
}