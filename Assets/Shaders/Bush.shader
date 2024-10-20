Shader "Unlit/FoliageShader"
{
    Properties
    {
        _BaseColor("BaseColor", Color) = (0,0,0,0)
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Smoothness("Smoothness", Range(0, 1)) = 0.0

        _DroopStrength("DroopStrength", float) = 0.0
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
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"


            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord : TEXCOORD6;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float height : TEXCOORD1; 
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float4 pos : SV_POSITION;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord : TEXCOORD6;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_INSTANCING_BUFFER_START(PerInstance)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurrLayerIndex)
            UNITY_INSTANCING_BUFFER_END(PerInstance)

            float4 _BaseColor;
            float _Metallic;
            float _Smoothness;

            float4 _MainTex_ST;
            int  _Resolution;
            float _Thickness;
            float _Height;
            int _Layers;
            float _MinHeight;
            float _DroopStrength;

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

                o.worldPos += float3(droopAtRest);

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    posInfo.positionWS = o.worldPos;
                    o.shadowCoord = GetShadowCoord(posInfo);
                #endif

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
                //generates random nr 0-1  
                float randFloat = hash12(trunc(i.uv*_Resolution));
                float randHeight = lerp(_MinHeight, _Height, randFloat);
                float4 pixelColor = float4(1,0,0,0);

                //centered pixel coordinates
                float2 cntrdPixlCoord = frac(_Resolution * i.uv) * 2.0 - 1.0;
                //distance to center of pixel
                float dist = length(cntrdPixlCoord);

                float currLayerHeight = lerp(_MinHeight, _Height, currLayerIndex/_Layers);
                
                float4 color = float4(1,0,0,0);

                //discard pixels below height threshold, z
                if (randHeight  < currLayerHeight) discard;

                //discard pixels outside cylinder, x and y
                if (dist > _Thickness * (randHeight - currLayerHeight)) discard; //the higher we get in the layers the thinner the strand should be
                else
                {
                   //initialise inputdata
                    InputData inputData = (InputData)0;
                    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                        inputData.positionWS = i.worldPos;
                    #endif

                    inputData.normalWS = normalize(i.worldNormal);
                    inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.worldPos);

                    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                        inputData.shadowCoord = i.shadowCoord;
                    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                    #else
                        inputData.shadowCoord = float4(0, 0, 0, 0);
                    #endif

                    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.pos);

                    //input surfaceData
                    SurfaceData surfaceData = (SurfaceData)0;
                    surfaceData.alpha = 1.0;
                    surfaceData.albedo = _BaseColor.rgb * lerp(0.2, 1.0, (currLayerIndex/_Layers));
                    surfaceData.metallic = _Metallic;
                    surfaceData.specular = float3(0.0, 0.0, 0.0);
                    surfaceData.smoothness = _Smoothness;
                    surfaceData.normalTS = float3(0,0,1);
                    surfaceData.occlusion = 1.0;
                    surfaceData.emission = float3(0,0,0);
                    surfaceData.clearCoatMask = half(0.0);
                    surfaceData.clearCoatSmoothness = half(0.0);

                   color = UniversalFragmentPBR(inputData, surfaceData);
                }
                return color;
            }
            ENDHLSL
        }
    }
}