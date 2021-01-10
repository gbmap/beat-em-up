#ifndef CATACUMBA_UTILS
#define CATACUMBA_UTILS

#include "UnityCG.cginc"

#define MAX_LIGHTS 4
float4 _LightColors[MAX_LIGHTS];
float4 _LightDirections[MAX_LIGHTS];

float4 healthEffectDisplacement(float v, float t)
{
    float offset = sin(t*100)*0.02*v;
    return float4(offset, offset, offset, 0.0);
}

float4 healthEffectColor(float v)
{
    return fixed4(1.0, 1.0, 1.0, 0.0) * v;
}

fixed4 selectEffectColor(float v, float4 vertWorld, float3 n, float t)
{
    fixed3 vert2Cam = normalize(_WorldSpaceCameraPos - vertWorld.xyz);
    fixed3 normal = mul(unity_ObjectToWorld, n).xyz;

    float intensity = (1-max(0, dot(vert2Cam, normal)))*((sin(t*5)+1)/2);

    return fixed4(1.6, 0.6, 0.3, 0.0) * v * intensity;
}
 
 fixed3 lighting(fixed3 color, float4 vertex, float3 normal)
 {
    //return unity_FogColor.xyz;
    float3 ldir = mul(unity_WorldToObject, _LightDirections[0]).xyz;
    float a = step(0.5, (dot(ldir,normal) + 1.0)/2.0); 
    return lerp(unity_FogColor.xyz, color, a);
    //return fixed3(a,a,a);
 }

 #endif