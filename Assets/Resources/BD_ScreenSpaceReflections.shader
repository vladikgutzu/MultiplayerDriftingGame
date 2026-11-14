Shader "Hidden/BD/ScreenSpaceReflections"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "ScreenSpaceReflections"

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float _MaxDistance;
            float _Thickness;
            float _StepCount;
            float _EdgeFade;
            TEXTURE2D_X(_BDSSRSurfaceData);

            bool IsSky(float rawDepth)
            {
                #if UNITY_REVERSED_Z
                    return rawDepth <= 0.00001;
                #else
                    return rawDepth >= 0.99999;
                #endif
            }

            float SampleRayDepth(float2 uv)
            {
                uv = ClampAndScaleUVForBilinear(UnityStereoTransformScreenSpaceTex(uv), _CameraDepthTexture_TexelSize.xy);
                // Ray-march iterations diverge between pixels: implicit derivatives
                // inside the loop are undefined, even for a non-mipmapped depth map.
                return SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_PointClamp, uv, 0).r;
            }

            half4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                half4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                float smoothness = SAMPLE_TEXTURE2D_X_LOD(_BDSSRSurfaceData, sampler_PointClamp, uv, 0).r;
                // Rough surfaces retain their ordinary Lit lighting/probe response, not
                // a sharp mirrored image. Skip the expensive march entirely for them.
                float surfaceWeight = smoothstep(.2, .85, smoothness) * smoothness;
                if (surfaceWeight <= .001)
                    return source;
                float rawDepth = SampleSceneDepth(uv);
                if (IsSky(rawDepth))
                    return source;

                float3 positionWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                float3 normalWS = normalize(SampleSceneNormals(uv));
                float3 incidentWS = normalize(positionWS - GetCameraPositionWS());
                float3 reflectionWS = normalize(reflect(incidentWS, normalWS));

                // Rays pointing back toward the camera cannot produce a stable screen-space hit.
                if (dot(reflectionWS, incidentWS) > .98)
                    return source;

                int stepCount = max(8, (int)_StepCount);
                float stepLength = _MaxDistance / stepCount;
                float3 rayPosition = positionWS + normalWS * max(.03, _Thickness * .08);
                float2 hitUv = 0;
                float hit = 0;

                [loop]
                for (int stepIndex = 1; stepIndex <= stepCount; ++stepIndex)
                {
                    // A slightly accelerating march covers nearby detail without losing range.
                    float travel = stepLength * stepIndex * lerp(.45, 1.0, (float)stepIndex / stepCount);
                    float3 samplePositionWS = rayPosition + reflectionWS * travel;
                    float4 samplePositionCS = TransformWorldToHClip(samplePositionWS);
                    if (samplePositionCS.w <= 0)
                        break;

                    float2 sampleUv = samplePositionCS.xy / samplePositionCS.w * .5 + .5;
                    #if UNITY_UV_STARTS_AT_TOP
                        sampleUv.y = 1.0 - sampleUv.y;
                    #endif
                    if (any(sampleUv <= 0.001) || any(sampleUv >= .999))
                        break;

                    float sceneDepth = SampleRayDepth(sampleUv);
                    if (IsSky(sceneDepth))
                        continue;

                    float3 scenePositionWS = ComputeWorldSpacePosition(sampleUv, sceneDepth, UNITY_MATRIX_I_VP);
                    float rayEyeDepth = -TransformWorldToView(samplePositionWS).z;
                    float sceneEyeDepth = -TransformWorldToView(scenePositionWS).z;
                    float depthDelta = rayEyeDepth - sceneEyeDepth;
                    float allowedThickness = _Thickness * (1.0 + travel / max(_MaxDistance, .001));

                    if (depthDelta >= 0 && depthDelta <= allowedThickness)
                    {
                        hitUv = sampleUv;
                        hit = 1;
                        break;
                    }
                }

                if (hit == 0)
                    return source;

                half3 reflectedColor = SAMPLE_TEXTURE2D_X_LOD(
                    _BlitTexture, sampler_LinearClamp, hitUv, _BlitMipLevel).rgb;
                float2 edgeDistance = min(hitUv, 1.0 - hitUv);
                float edgeMask = saturate(min(edgeDistance.x, edgeDistance.y) / max(_EdgeFade, .001));
                float fresnel = pow(1.0 - saturate(dot(-incidentWS, normalWS)), 3.0);
                float reflectionAmount = saturate(_Intensity * surfaceWeight * edgeMask * lerp(.2, 1.0, fresnel));

                return half4(lerp(source.rgb, reflectedColor, reflectionAmount), source.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
