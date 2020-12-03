using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catacumba.Rendering
{
    public class RPLightData 
    {
        const int MAX_LIGHTS = 4;
        static int ID_LIGHT_COLORS = Shader.PropertyToID("_LightColors");
        static int ID_LIGHT_DIRECTIONS = Shader.PropertyToID("_LightDirections");
        static int ID_LIGHT_SHADOW_MAP = Shader.PropertyToID("_LightShadowData");

        Vector4[] _lightColors     = new Vector4[MAX_LIGHTS];
        Vector4[] _lightDirections = new Vector4[MAX_LIGHTS];
        Vector4[] _lightShadowData = new Vector4[MAX_LIGHTS];

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
        )
        {
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
        )
        {
            GetLightData(cull, shadows, ref _lightDirections, ref _lightColors, ref _lightShadowData);
            SendLightDataToGPU(context, _lightDirections, _lightColors, _lightShadowData);
        }

        void GetLightData(
            CullingResults cull, 
            RPShadows shadows,
            ref Vector4[] lightDirections, 
            ref Vector4[] lightColors,
            ref Vector4[] lightShadowData
        ) {
            for (int i = 0; i < Mathf.Min(lightColors.Length, cull.visibleLights.Length); i++) {
                VisibleLight light = cull.visibleLights[i];

                Vector3 fwd = light.light.transform.forward;
                Vector4 lightPos = new Vector4(fwd.x, -fwd.y, fwd.z, 1.0f);

                lightColors    [i] = light.finalColor;
                lightDirections[i] = lightPos;
                lightShadowData[i] = shadows.ReserveDirectionalShadows(light.light, i);
            }
        }

        void SendLightDataToGPU(ScriptableRenderContext context, 
                                Vector4[] lightDirections, 
                                Vector4[] lightColors, 
                                Vector4[] lightShadowmaps)
        {
            buffer.SetGlobalVectorArray(ID_LIGHT_DIRECTIONS, lightDirections);
            buffer.SetGlobalVectorArray(ID_LIGHT_COLORS, lightColors);
            buffer.SetGlobalVectorArray(ID_LIGHT_SHADOW_MAP, lightShadowmaps);

            context.ExecuteCommandBuffer(buffer);

            //buffer.Release();
            context.Submit();
        }

        public void Cleanup()
        {
            shadows.Cleanup();
        }

    }
}