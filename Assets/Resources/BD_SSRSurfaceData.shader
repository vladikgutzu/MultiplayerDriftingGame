Shader "Hidden/BD/SSRSurfaceData"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _MetallicGlossMap("Metallic Smoothness", 2D) = "white" {}
        _SpecGlossMap("Specular Smoothness", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0,1)) = 0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            ZTest LEqual
            Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex SurfaceVert
            #pragma fragment SurfaceFrag
            #pragma multi_compile_instancing
            // No authored material uses this replacement shader directly: retain
            // variants needed by the original materials instead of stripping them.
            #pragma multi_compile_local _ _ALPHATEST_ON
            #pragma multi_compile_local _ _METALLICSPECGLOSSMAP
            #pragma multi_compile_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma multi_compile_local _ _SPECULAR_SETUP
            #pragma multi_compile_local _ _ENVIRONMENTREFLECTIONS_OFF
            // Keep the original URP Lit constant-buffer layout and sampling rules.
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings SurfaceVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            half SurfaceFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half alpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a;
                Alpha(alpha, _BaseColor, _Cutoff);
                #if defined(_ENVIRONMENTREFLECTIONS_OFF)
                    return 0;
                #else
                    return saturate(SampleMetallicSpecGloss(input.uv, alpha).a);
                #endif
            }
            ENDHLSL
        }
    }
    Fallback Off
}
