Shader "Catacumba/Level"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _HitFactor ("Hit Factor", Range(0, 1)) = 0 
        _Selected ("Selected", Range(0, 1)) = 0 

        [MaterialToggle] _Displacement( "Displace", Float ) = 0
    }
    SubShader
    {
        Pass
        {
            Tags { 
                "LightMode"="Level" 
                "Queue"="Opaque"
            }

            HLSLPROGRAM
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

            //ColorMask 0

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
