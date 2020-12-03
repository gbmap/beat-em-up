#ifndef CATACUMBA_UTILS
#define CATACUMBA_UTILS

#define UNITY_MATRIX_M   unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V   unity_MatrixV
#define UNITY_MATRIX_VP  unity_MatrixVP
#define UNITY_MATRIX_P   glstate_matrix_projection

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;

float3 _WorldSpaceCameraPos;
float4 unity_FogColor;

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4x4 unity_WorldToObject;
	float4 unity_LODFade;
	real4 unity_WorldTransformParams;
CBUFFER_END

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#define MAX_LIGHTS 4
CBUFFER_START(_LightData)
    float4 _LightColors[MAX_LIGHTS];
    float4 _LightDirections[MAX_LIGHTS];
    float4 _LightShadowData[MAX_LIGHTS];
CBUFFER_END

#include "Catacumba_Shadows.hlsl"

float4 healthEffectDisplacement(float v, float t)
{
    float offset = sin(t*100)*0.02*v;
    return float4(offset, offset, offset, 0.0);
}

float4 healthEffectColor(float v)
{
    return float4(1.0, 1.0, 1.0, 0.0) * v;
}

float4 selectEffectColor(float v, float4 vertWorld, float3 n, float t)
{
    float3 vert2Cam = normalize(_WorldSpaceCameraPos - vertWorld.xyz);
    float intensity = (1-max(0, dot(vert2Cam, n)))*((sin(t*5)+1)/2);
 
    return float4(1.6, 0.6, 0.3, 0.0) * v * pow(0.1, intensity);
}
 
float3 lighting(float3 color, float3 normal, float3 vertWS)
 {
    //return unity_FogColor.xyz;
    float3 ldir = _LightDirections[0].xyz;
    float3 n = TransformWorldToObject(normal);
    float a = step(0.6, (dot(ldir, n) + 1.0)/2.0); 
    float shadow = GetShadowAttenuation(0, vertWS);
    //return float3(shadow,shadow,shadow);

    return lerp(unity_FogColor.xyz, color, min(shadow, a));
    //return fixed3(a,a,a);
 }

 #endif