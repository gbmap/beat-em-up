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
#define MAX_OTHER_LIGHTS 6*12 

CBUFFER_START(_LightData)
    int    _LightCount;
    float4 _LightColors[MAX_LIGHTS];
    float4 _LightDirections[MAX_LIGHTS];
    float4 _LightShadowsData[MAX_LIGHTS];

    int    _OtherLightCount;
    float4 _OtherLightColors[MAX_OTHER_LIGHTS];
    float4 _OtherLightPositions[MAX_OTHER_LIGHTS];
    float4 _OtherLightData[MAX_OTHER_LIGHTS];
    float4 _OtherLightShadowData[MAX_OTHER_LIGHTS];
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
    float3 vert2Cam = normalize(_WorldSpaceCameraPos - vertWorld.xyz).xyz;
    float intensity = (1-max(0, dot(vert2Cam, n)))*((sin(t*5)+1)/2);
 
    return float4(1.6, 0.6, 0.3, 0.0) * v * pow(0.1, intensity);
}

float light_distance_factor(float3 lightToVertex, float d)
{
    return 1. - step(d, length(lightToVertex));
}

void other_lights(float3 normalWS, float3 vertWS, out float a, out float3 color)
{
    a = 0.0;
    color = 0.0;


    for (int i = 0; i < _OtherLightCount; i++)
    {
        float  lrange = _OtherLightData[i].x;
        float3 ldelta = vertWS - _OtherLightPositions[i];
        float  ldotn  = -dot(normalize(ldelta), normalWS);

        // clamps light if too far 
        float  distF  = light_distance_factor(ldelta, lrange);

        // shadow attenuation
        float sa = GetPointShadowAttenuation(i, vertWS, normalWS, ldelta, distF);
              //sa = step(0.5, sa);
              //sa = smoothstep(0.0, 0.55, sa) * sa;
              //sa *= 1/(pow(length(ldelta),0.2));

        // light clamping based on distance 
        float lclamp = max(a, step(0., ldotn) * distF); 


        a += sa; // / length(ldelta);
        //a = lerp(a, sa, 0.5);
        //a += smoothstep(-0.15, 0.95, sa);

        float lcolor = 0.5 - smoothstep(0.0, 20.0, lrange)*0.4;
        color += _OtherLightColors[i] * step(lcolor, ldotn*light_distance_factor(ldelta, lrange*0.8));
    }

    a = saturate(a);
}

float directional_lights(float3 normalWS, float3 vertWS)
{
    float shadow = .0;
    float a = .0;
    for (int i = 0; i < _LightCount; i++)
    {
        float3 ldir   = _LightDirections[i].xyz;
        //a      = max(a, (max(0., dot(ldir, normalWS)))));
        //shadow = max(shadow, GetShadowAttenuation(i, vertWS) * _LightShadowsData[i].x);
    }

    return a;
    //color = lerp(lerp(color, unity_FogColor.xyz,.9), color, min(a, shadow));
    //return color;
}
 
float3 lighting(float3 color, float3 normalWS, float3 vertWS)
 {
    float3 normal = TransformWorldToObject(normalWS);
    float a = 0.0;
    float3 lightClr = float3(0.0, 0.0, 0.0);

    other_lights(normalWS, vertWS, a, lightClr);

    //return a;

    //return lightClr;
    return lerp(unity_FogColor.xyz, color+lightClr, a);
 }

 #endif