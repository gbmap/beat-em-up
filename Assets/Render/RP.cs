using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catacumba.Rendering
{
    public class RP : RenderPipeline
    {
        const int MAX_LIGHTS = 4;
        static int ID_LIGHT_COLORS = Shader.PropertyToID("_LightColors");
        static int ID_LIGHT_DIRECTIONS = Shader.PropertyToID("_LightDirections");

        Vector4[] _lightColors     = new Vector4[MAX_LIGHTS];
        Vector4[] _lightDirections = new Vector4[MAX_LIGHTS];

        RPCameraRenderer renderer;
        ShadowSettings shadowSettings = new ShadowSettings();

        public RP(ShadowSettings shadows)
        {
            shadowSettings = shadows;
            renderer = new RPCameraRenderer(shadowSettings);
        }

        protected override void Render (ScriptableRenderContext context, Camera[] cameras) 
        {
            foreach (var camera in cameras)
            {
                renderer.Render(context, camera, shadowSettings);
            }
        }
    }
}