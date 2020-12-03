
#ifndef CATACUMBA_SHADOWS
#define CATACUMBA_SHADOWS

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

TEXTURE2D_SHADOW(_ShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_Shadows)
	float4x4 _ShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

float SampleShadowAtlas(float3 posSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_ShadowAtlas, SHADOW_SAMPLER, posSTS);
}

float GetShadowAttenuation(int tileIndex, float3 posWS)
{
    float3 posSTS = mul(_ShadowMatrices[tileIndex], float4(posWS, 1.0)).xyz;
    return SampleShadowAtlas(posSTS);
}

#endif