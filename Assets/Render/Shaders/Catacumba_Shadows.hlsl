
#ifndef CATACUMBA_SHADOWS
#define CATACUMBA_SHADOWS

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_SHADOWED_POINT_LIGHT_COUNT 6*12


TEXTURE2D_SHADOW(_ShadowAtlas);
TEXTURE2D_SHADOW(_ShadowPointAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_Shadows)
	float4x4 _ShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
    float4x4 _ShadowPointMatrices[MAX_SHADOWED_POINT_LIGHT_COUNT];
    float4   _ShadowPointTiles[MAX_SHADOWED_POINT_LIGHT_COUNT];
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

static const float3 pointShadowPlanes[6] = {
	float3(-1.0, 0.0, 0.0),
	float3(1.0, 0.0, 0.0),
	float3(0.0, -1.0, 0.0),
	float3(0.0, 1.0, 0.0),
	float3(0.0, 0.0, -1.0),
	float3(0.0, 0.0, 1.0)
};

float SamplePointShadowAtlas (float3 positionSTS, float3 bounds) {
	positionSTS.xy = clamp(positionSTS.xy, bounds.xy, bounds.xy + bounds.z);
	return SAMPLE_TEXTURE2D_SHADOW(
		_ShadowPointAtlas, SHADOW_SAMPLER, positionSTS
	);
}

float GetPointShadowAttenuation(
    int tileIndex, 
    float3 posWS, 
    float3 normalWS, 
    float3 lightDirectionWS,
    float lightDistanceFactor)
{
    float faceOffset = CubeMapFaceID(lightDirectionWS);
    tileIndex = (tileIndex * 6.0) + faceOffset;
    //tileIndex += faceOffset;

    float3 lightPlane = pointShadowPlanes[faceOffset];
    float4 tileData = _ShadowPointTiles[tileIndex];

    float dist2Plane = dot(-lightDirectionWS, lightPlane);
    float3 normalBias = normalWS * (dist2Plane * tileData.w);

    float4 posSTS = mul(_ShadowPointMatrices[tileIndex], float4(posWS + normalBias, 1.0));

    float pointShadowSample = SamplePointShadowAtlas(posSTS.xyz/posSTS.w, tileData.xyz);
    return pointShadowSample * lightDistanceFactor;
}


#endif