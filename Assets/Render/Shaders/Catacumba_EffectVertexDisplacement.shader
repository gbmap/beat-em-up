Shader "Catacumba/Effect (Vertex Displacement)"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.0, 0.0, 0.0 ,0.0)
        _HitFactor ("Hit Factor", Range(0, 1)) = 0 
        _Selected ("Selected", Range(0, 1)) = 0 

        _DisplacementTex("Displacement Texture", 2D) = "white" {}
        _SampleScale("Sample Scale", Range(0, 10)) = 0
        _DisplacementScale("Displacement Scale", Range(0, 10)) = 0
        _TimeScale("Time Scale", Range(0, 10)) = 0
    }
    SubShader
    {
        Pass
        {
            Tags { 
                "LightMode"="Entity" 
                "Queue"="Opaque"
            }

            HLSLPROGRAM
            #define VERT_DISPLACE
            #define TINT_COLOR
            #pragma vertex vert
            #pragma fragment frag
            #include "Catacumba_Entity.hlsl"
            ENDHLSL
        }

        Pass 
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ColorMask 0

            HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex shadow_caster_vert
			#pragma fragment shadow_caster_frag
			#include "Catacumba_ShadowCasterPass.hlsl"
			ENDHLSL
        }
    }
}
