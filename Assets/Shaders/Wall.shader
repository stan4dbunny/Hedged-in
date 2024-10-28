Shader "Unlit/Wall"
{
    Properties
    {
        _BaseColor("BaseColor", Color) = (0,0,0,0)
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Smoothness("Smoothness", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            Tags { "LightMode"="UniversalForward" }

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
            #pragma multi_compile_fog
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
                float fogFactor : TEXCOORD5;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord : TEXCOORD6;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _BaseColor;
            float _Metallic;
            float _Smoothness;

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

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    posInfo.positionWS = o.worldPos;
                    o.shadowCoord = GetShadowCoord(posInfo);
                #endif

                o.pos = TransformWorldToHClip(o.worldPos);
                o.fogFactor = ComputeFogFactor(o.pos.z);
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
                surfaceData.albedo = _BaseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = float3(0.0, 0.0, 0.0);
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0,0,1);
                surfaceData.occlusion = 1.0;
                surfaceData.emission = float3(0,0,0);
                surfaceData.clearCoatMask = half(0.0);
                surfaceData.clearCoatSmoothness = half(0.0);

                float fogFactorFrag = InitializeInputDataFog(float4(i.worldPos, 1.0), i.fogFactor);
                float4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, fogFactorFrag);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
            // For Directional lights, _LightDirection is used when applying shadow Normal Bias.
            // For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
            float3 _LightDirection;
            float3 _LightPosition;

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half IsPointLight()
            {
                return _ShadowBias.z > 1.0 && _ShadowBias.z <= 2.0 ? 1 : 0;
            }
            
            float4 GetShadowPositionHClip(MeshData input)
            {
                float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normal);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    float clamped = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    float clamped = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                // The current implementation of vertex clamping in Universal RP is the same as in Unity Built-In RP.
                // This does not work well with Point Lights, which is why it is disabled in Built-In RP
                // (see: https://github.cds.internal.unity3d.com/unity/unity/blob/a9c916ba27984da43724ba18e70f51469e0c34f5/Runtime/Camera/Shadows.cpp#L1685-L1686)
                // We follow the same convention in Universal RP:
                positionCS.z = lerp(clamped, positionCS.z, IsPointLight());

                return positionCS;
            }

            Interpolators ShadowPassVertex(MeshData input)
            {
                Interpolators output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.pos = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Interpolators input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);

                return 0;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            ZWrite On


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Interpolators
            {
                float3 worldNormal : TEXCOORD3;
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;

                UNITY_SETUP_INSTANCE_ID(v); 
                ZERO_INITIALIZE(Interpolators, o); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                VertexPositionInputs posInfo = GetVertexPositionInputs(v.vertex.xyz);

                //set interpolator stuff used for lighting
                o.worldNormal = GetVertexNormalInputs(v.normal, v.tangent).normalWS;

                o.pos = posInfo.positionCS;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                return float4(i.worldNormal,0);
            }
            ENDHLSL
        }
    }
}