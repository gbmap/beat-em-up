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

TEXTURE2D(_NoiseTex);
SAMPLER(sampler_NoiseTex);

#ifdef VERT_DISPLACE
TEXTURE2D(_DisplacementTex);
SAMPLER(sampler_DisplacementTex);

float _SampleScale;
float _DisplacementScale;
float _TimeScale;
#endif 

#ifdef TINT_COLOR
float4 _Color;
#endif

float4 triplanarNoise(float3 v, float3 n)
{
    float3 x = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, v.xy);
    float3 y = SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex, v.zx);
    float3 z = SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex, v.xy);

    float3 t = pow(abs(n), 0.5);

    return float4((x * t.x + y * t.y + z * t.z), 1.0);
}

v2f vert(vertIN i) 
{
    float3 posW = TransformObjectToWorld(i.positionOS); 

    #ifdef VERT_DISPLACE
    posW += textureVertexDisplacement(_DisplacementTex, sampler_DisplacementTex, posW, i.normalOS, _SampleScale, _DisplacementScale, _TimeScale);
    #endif

    posW += healthEffectDisplacement(_HitFactor, _Time.y);

    float4 posCS = TransformWorldToHClip(posW);

    v2f o;
    o.positionCS = posCS;
    o.positionWS = posW;
    o.normalWS = TransformObjectToWorldNormal(i.normalOS);
    o.uv = i.uv;

    return o;
}

float4 frag(v2f i) : SV_TARGET
{
    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv); 
    #ifdef TINT_COLOR
    color *= _Color;
    #endif
    float light = 0.;
    color.xyz = lighting(color, i.normalWS, i.positionWS, light);
    color += healthEffectColor(_HitFactor);
    color += selectEffectColor(_Selected, i.positionCS, i.normalWS, _Time.y);


    float noise = (triplanarNoise(i.positionWS, i.normalWS)).r;
    noise = clamp(noise+.35, 0., 1.);
    color.rgb -= .07*noise*light;
    
    return  color * color.a;
    return color * color.a;  
}

#endif