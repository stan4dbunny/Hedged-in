Shader "Unlit/cloudShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

             // vertex input: position, UV
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };
            
            v2f vert (appdata v) {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;
                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
                // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return output;
            }

            float4 params;


            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            Texture3D<float4> ShapeNoise;
            Texture3D<float4> DetailNoise;
            Texture2D<float4> BlueNoise;
            SamplerState samplerShapeNoise;
            SamplerState samplerDetailNoise;
            SamplerState samplerBlueNoise;

            float3 BoundsMin;
            float3 BoundsMax;
            float3 CloudOffset;
            float4 ShapeNoiseWeights;
            float3 DetailNoiseWeights;
            float2 PhaseParams;
            float CloudScale;
            float DensityMultiplier;
            float LightAbsorptionTowardSun;
            float DarknessThreshold;
            float LightAbsorptionThroughClouds;
            float TransmittanceCutoff;
            float PhaseWeight;
            int NumSteps;
            int NumStepsLight;

            float4 _LightColor0;

            //https://www.diva-portal.org/smash/get/diva2:1223894/FULLTEXT01.pdf
            //Function A.2
            float reMap(float val, float oldLow, float oldHigh, float newLow, float newHigh) {
                return newLow + (val-oldLow) * (newHigh - newLow) / (oldHigh-oldLow);
            }
            
            float henyeyGreenstein(float g, float ang){
                //https://oceanopticsbook.info/view/scattering/level-2/the-henyey-greenstein-phase-function
                float g2 = g*g;
                return (1-g2) / (4*3.1415*pow(1+g2-2*g*(ang), 1.5));
            }

            float phaseFunction(float ang){
                //Linear combination of HG phase function for imptoving fit at large and small values for ang
                //https://oceanopticsbook.info/view/scattering/level-2/the-henyey-greenstein-phase-function
                float hg = henyeyGreenstein(PhaseParams.x, ang) * (1-PhaseWeight) + henyeyGreenstein(PhaseParams.y, ang) * PhaseWeight;
                return 0.3 + hg*0.4;
            }

            float2 squareUV(float2 uv){
                float w =  _ScreenParams.x;
                float h = _ScreenParams.y;
                float scale = 1000;
                float x = uv.x * w;
                float y = uv.y * h;

                return float2(x/scale, y/scale);
            }

             // Returns (dstToBox, dstInsideBox). If ray misses box, dstInsideBox will be zero)
             float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 rayDir) {
                // From http://jcgt.org/published/0007/03/04/
                // via https://medium.com/@bromanz/another-view-on-the-classic-ray-aabb-intersection-algorithm-for-bvh-traversal-41125138b525
                float3 t0 = (boundsMin - rayOrigin) / rayDir;
                float3 t1 = (boundsMax - rayOrigin) / rayDir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                // CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
                // dstA is dst to nearest intersection, dstB dst to far intersection

                // CASE 2: ray intersects box from inside (dstA < 0 < dstB)
                // dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

                // CASE 3: ray misses box (dstA > dstB)

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            float sampleDensity(float3 position) {
                float3 uvw = position * CloudScale * 0.001 + CloudOffset * 0.01;
                float3 detailuvw = position * 0.001 + 0.25 * 0.1;
                float3 size = BoundsMax - BoundsMin;

                //falloff along edges of cloud container
                float dstFromEdgeX = min(45, min(position.x - BoundsMin.x, BoundsMax.x - position.x));
                float dstFromEdgeY = min(45, min(position.y - BoundsMin.y, BoundsMax.y - position.y));
                float dstFromEdgeZ = min(45, min(position.z - BoundsMin.z, BoundsMax.z - position.z));
                float edgeWeight = min(dstFromEdgeZ,dstFromEdgeX)/45;

                float heightPercent = (position.y - BoundsMin.y) / size.y;
                float heightGradient = saturate(reMap(heightPercent, 0.0, 0.2, 0, 1)) * saturate(reMap(heightPercent, 1, 0.7, 0, 1));
                heightGradient *= edgeWeight;

                float4 shape = ShapeNoise.SampleLevel(samplerShapeNoise, uvw, 0);
                
                float4 normalizedShapeWeights = normalize(ShapeNoiseWeights);
                float shapeFBM = dot(shape, normalizedShapeWeights) * heightGradient;

                if(shapeFBM > 0){
                    float4 detail = DetailNoise.SampleLevel(samplerDetailNoise, detailuvw, 0);
                    float3 normalizedDetailWeights = normalize(DetailNoiseWeights);
                    float detailFBM = dot(detail, normalizedDetailWeights);

                    float oneMinusShape = 1 - shapeFBM;
                    float detailErode = oneMinusShape * oneMinusShape * oneMinusShape;
                    float density = shapeFBM - (1 - detailFBM) * detailErode * DetailNoiseWeights;

                    return density * DensityMultiplier * 0.1;
                }

                return 0;
            }

            float lightmarch(float3 position){
                float3 dirToLight = _WorldSpaceLightPos0.xyz;
                float dstInsideBox = rayBoxDst(BoundsMin, BoundsMax, position, dirToLight).y;

                float stepSize = dstInsideBox/NumStepsLight;
                float totDensity = 0;

                for(int step = 0; step < NumStepsLight; step++){
                    position += dirToLight * stepSize;
                    totDensity += max(0, sampleDensity(position) * stepSize);
                }

                float transmittance = exp(-(totDensity * LightAbsorptionTowardSun)); //Beer's law
                return DarknessThreshold + transmittance * (1 - DarknessThreshold);
            }
        
            float4 frag (v2f input) : SV_Target
            {
                float4 col = tex2D(_MainTex, input.uv);
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(input.viewVector);
                
                //Depth texture
                float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv);
                float depth = LinearEyeDepth(nonLinearDepth) * length(input.viewVector);

                float2 rayBoxInfo = rayBoxDst(BoundsMin, BoundsMax, rayOrigin, rayDir);
                float dstToBox = rayBoxInfo.x;
                float dstInsideBox = rayBoxInfo.y;

                // Phase function makes clouds brighter around sun
                float cosAngle = dot(rayDir, _WorldSpaceLightPos0.xyz);
                float phaseVal = phaseFunction(cosAngle);
                
                //allows us to raymarch less steps and still get nice results by making lower resolution results noisy instead of jagged
                float offset = BlueNoise.SampleLevel(samplerBlueNoise, squareUV(input.uv * 3), 0);
                float dstTravelled = offset;

                float transmittance = 1;
                float4 lightEnergy = 0;
                float stepSize = dstInsideBox / NumSteps;
                float dstLimit = min(depth - dstToBox, dstInsideBox);

                // March through volume:
                float totalDensity = 0;
                while (dstTravelled < dstLimit) {
                    float3 rayPos = rayOrigin + rayDir * (dstToBox + dstTravelled);
                    float density = sampleDensity(rayPos);

                    if(density > 0){
                        float lightTransmittance = lightmarch(rayPos);
                        lightEnergy += density * stepSize * transmittance * lightTransmittance * phaseVal;
                        transmittance *= exp(-density * stepSize * LightAbsorptionThroughClouds);

                        if(transmittance < TransmittanceCutoff){
                            break;
                        }
                    }

                    dstTravelled += stepSize;
                }

                float4 cloudCol = lightEnergy * _LightColor0;
                float3 col0 = col * transmittance + cloudCol;
                return float4(lerp(col, col0, smoothstep(10000, 2500, dstToBox + dstInsideBox)), 0);
            }
            ENDCG
        }
    }
}
