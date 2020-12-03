
#ifndef CATACUMBA_ENTITY
#define CATACUMBA_ENTITY

#include "Catacumba.hlsl"

struct vertIN {
	float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
};

struct v2f {
	float4 positionCS : SV_POSITION;
	float3 normalWS : VAR_NORMAL;
	float2 uv : VAR_BASE_UV;
    float3 positionWS : TEXCOORD0;
};

CBUFFER_START(UnityPerMaterial)
    float _HitFactor;
    float _Selected;
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float4 _Time;


v2f vert(vertIN i) 
{
    float3 posW = TransformObjectToWorld(i.positionOS); 
    posW += healthEffectDisplacement(_HitFactor, _Time.y);

    float4 posCS = TransformWorldToHClip(posW);

    v2f o;
    o.positionCS = posCS;
    o.positionWS = posW;
    o.normalWS = TransformObjectToWorld(i.normalOS);
    o.uv = i.uv;

    return o;
}

float4 frag(v2f i) : SV_TARGET
{
    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv); 
    color += healthEffectColor(_HitFactor);
    color.xyz = lighting(color, i.normalWS, i.positionWS);
    color += selectEffectColor(_Selected, i.positionCS, i.normalWS, _Time.y);
    return color;  
}

#endif