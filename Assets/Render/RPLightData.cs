using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catacumba.Rendering
{
    public class RPLightData 
    {
        const int MAX_LIGHTS       = 4,
                  MAX_OTHER_LIGHTS = 6*12;

        int ID_LIGHT_COLORS              = Shader.PropertyToID("_LightColors"),
            ID_LIGHT_DIRECTIONS          = Shader.PropertyToID("_LightDirections"),
            ID_LIGHT_SHADOW_MAP          = Shader.PropertyToID("_LightShadowsData"),
            ID_LIGHT_COUNT               = Shader.PropertyToID("_LightCount"),

            ID_OTHER_LIGHT_COUNT         = Shader.PropertyToID("_OtherLightCount"),
            ID_OTHER_LIGHT_COLORS        = Shader.PropertyToID("_OtherLightColors"),
            ID_OTHER_LIGHT_DATA          = Shader.PropertyToID("_OtherLightData"),
            ID_OTHER_LIGHT_SHADOW_MAP    = Shader.PropertyToID("_OtherLightShadowData"),
            ID_OTHER_LIGHT_POSITIONS     = Shader.PropertyToID("_OtherLightPositions");

        Vector4[] _lightColors           = new Vector4[MAX_LIGHTS],
                  _lightDirections       = new Vector4[MAX_LIGHTS],
                  _lightShadowData       = new Vector4[MAX_LIGHTS],
                  _otherLightColors      = new Vector4[MAX_OTHER_LIGHTS],
                  _otherLightPositions   = new Vector4[MAX_OTHER_LIGHTS],
                  _otherLightData        = new Vector4[MAX_OTHER_LIGHTS],
                  _otherLightShadowData  = new Vector4[MAX_OTHER_LIGHTS];

        RPShadows shadows = new RPShadows();

        const string bufferName = "Lighting";
        CommandBuffer buffer = new CommandBuffer {
            name = bufferName
        };

        CullingResults cullingResults;
        public ShadowSettings ShadowSettings { get; private set; }

        public RPLightData(ShadowSettings shadowSettings)
        {
            this.ShadowSettings = shadowSettings;
        }

        public void Setup(
            ScriptableRenderContext context,
            CullingResults cull,
            ShadowSettings settings
        ) {
            cullingResults = cull;

            buffer.BeginSample(bufferName);
            shadows.Setup(context, cullingResults, settings);
            UpdateLightData(context, cullingResults);
            shadows.Render();
            buffer.EndSample(bufferName);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        public void UpdateLightData(
            ScriptableRenderContext context, 
            CullingResults cull
        ) {
            int lightCount, otherLightCount;
            GetLightData(cull, out lightCount, out otherLightCount);
            SendLightDataToGPU(context, lightCount, otherLightCount);
        }

        void GetLightData(
            CullingResults cull, 
            out int lightCount, 
            out int otherLightCount
        ) {
            lightCount = 0;
            otherLightCount = 0;
            for (int i = 0; i < cull.visibleLights.Length; i++) 
            {
                VisibleLight light = cull.visibleLights[i];
                switch (light.lightType)
                {
                    case LightType.Directional:
                    {
                        if (lightCount < MAX_LIGHTS)
                            GetDirectionalLightData(lightCount++, i, ref light);
                        break;
                    }
                    case LightType.Point:
                    {
                        if (otherLightCount < MAX_OTHER_LIGHTS)
                            if (GetOtherLightData(otherLightCount, i, ref light))
                                otherLightCount++;
                        break;
                    }
                }
            }
        }

        void GetDirectionalLightData(int lightCount, int index, ref VisibleLight light)
        {
            Vector3 fwd = light.light.transform.forward;
            Vector4 lightPos = new Vector4(fwd.x, fwd.y, fwd.z, 1.0f);
            _lightColors[lightCount]     = light.finalColor;
            _lightDirections[lightCount] = lightPos;
            _lightShadowData[lightCount] = shadows.ReserveDirectionalShadows(light.light, index);
        }

        bool GetOtherLightData(int lightCount, int index, ref VisibleLight light)
        {
            Vector4 pointShadowData = Vector4.zero;
            bool isOnBounds = shadows.ReservePointShadows(light.light, index, out pointShadowData);
            if (!isOnBounds) return false;

            Vector4 position = light.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(light.range * light.range, 0.00001f);
            _otherLightColors[lightCount]     = light.finalColor;
            _otherLightPositions[lightCount]  = position;
            _otherLightData[lightCount]       = new Vector4(light.range, 0f,0f,0f);
            _otherLightShadowData[lightCount] = pointShadowData;

            return true;
        }

        void SendLightDataToGPU(
            ScriptableRenderContext context,
            int lightCount,
            int otherLightCount
        ) {
            buffer.SetGlobalInt(ID_LIGHT_COUNT, lightCount);
            if (lightCount > 0)
            {
                buffer.SetGlobalVectorArray(ID_LIGHT_DIRECTIONS, _lightDirections);
                buffer.SetGlobalVectorArray(ID_LIGHT_COLORS,     _lightColors);
                buffer.SetGlobalVectorArray(ID_LIGHT_SHADOW_MAP, _lightShadowData);
            }

            buffer.SetGlobalInt(ID_OTHER_LIGHT_COUNT, otherLightCount);
            if (otherLightCount > 0)
            {
                buffer.SetGlobalVectorArray(ID_OTHER_LIGHT_POSITIONS,  _otherLightPositions);
                buffer.SetGlobalVectorArray(ID_OTHER_LIGHT_COLORS,     _otherLightColors);
                buffer.SetGlobalVectorArray(ID_OTHER_LIGHT_SHADOW_MAP, _otherLightShadowData);
                buffer.SetGlobalVectorArray(ID_OTHER_LIGHT_DATA, _otherLightData);
            }

            context.ExecuteCommandBuffer(buffer);

            context.Submit();
        }

        public void Cleanup()
        {
            shadows.Cleanup();
        }

    }
}