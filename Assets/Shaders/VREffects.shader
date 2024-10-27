Shader "Unlit/VREffects"
{
    Properties // input data
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipline" = "UniversalPipeline" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION; // vertex position
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
                // float3 normals : NORMAL;
                // float4 tangent : TANGENT;
                // float4 color : COLOR;
                // float4 uv0 : TEXCOORD0; // uv0 diffuse/normal map textures
                // float4 uv1 : TEXCOORD1; // uv1 coordinates lightmap coordinates

            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0; // 
                float4 vertex : SV_POSITION; // clip space position

                UNITY_VERTEX_OUTPUT_STEREO //Insert
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            Interpolators vert (MeshData v)
            {
                Interpolators o;

                UNITY_SETUP_INSTANCE_ID(v); //Insert
                UNITY_INITIALIZE_OUTPUT(Interpolators, o); //Insert
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

                o.vertex = UnityObjectToClipPos(v.vertex); // converts local space to clip space
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // bool 0 1
            // int 
            // float4 (32 bit float)
            // half (16 bit float)
            // fixed (lower precision) -1 to 1 
            // float4 -> half4 -> fixed4
            // float4x4 -> half4x4 (C#: Matrix4x4)

            //UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); //Insert

            fixed4 frag (Interpolators i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
    
                fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv); //Insert
                float gray = dot(col.rgb, float3(0.3, 0.59, 0.11));
                col.rgb = lerp(col.rgb, gray, 0.7);

                float2 uvR = i.uv + 0.5 * float2(0.005, 0.005);
                col.r = tex2D(_MainTex, uvR).r;
                

                return col;
            }
            ENDCG
        }
    }
}
