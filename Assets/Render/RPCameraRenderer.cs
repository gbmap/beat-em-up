using UnityEngine;
using UnityEngine.Rendering;

namespace Catacumba.Rendering
{
    public partial class RPCameraRenderer 
    {
        ScriptableRenderContext context;
        Camera camera;

        const string bufferName = "Render Camera";
        CullingResults cullingResults;

        CommandBuffer buffer = new CommandBuffer {
            name = bufferName
        };

        RPLightData lightRenderer;

        static ShaderTagId[] shaderTagIds = new ShaderTagId[] 
        {
            new ShaderTagId("Level"),
            new ShaderTagId("Entity")
        };

        static ShaderTagId[] legacyShaderTagIds = {
                new ShaderTagId("Always"),
                new ShaderTagId("ForwardBase"),
                new ShaderTagId("PrepassBase"),
                new ShaderTagId("Vertex"),
                new ShaderTagId("VertexLMRGBM"),
                new ShaderTagId("VertexLM")
            };

        public RPCameraRenderer(ShadowSettings shadowSettings)
        {
            lightRenderer = new RPLightData(shadowSettings);
        }

        public void Render (
            ScriptableRenderContext context, 
            Camera camera, 
            ShadowSettings shadowSettings
        ) {
            this.context = context;
            this.camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull(lightRenderer.ShadowSettings.maxDistance)) { return; }

            buffer.BeginSample(SampleName);
            ExecuteBuffer();
            lightRenderer.Setup(context, cullingResults, shadowSettings);
            buffer.EndSample(SampleName);

            Setup();
            foreach (var shaderTagId in shaderTagIds)
                DrawVisibleGeometry(shaderTagId);
            context.DrawSkybox(camera);

            DrawUnsupportedShaders();
            DrawGizmos();

            Submit();

            lightRenderer.Cleanup();
            Submit();
        }

        void Setup () 
        {
            context.SetupCameraProperties(camera);
            CameraClearFlags flags = camera.clearFlags;
            buffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ?
                    camera.backgroundColor.linear : Color.clear
            );
            buffer.BeginSample(SampleName);
            ExecuteBuffer();
        }

        void DrawVisibleGeometry(ShaderTagId shaderTagId) 
        {
            var sortingSettings   = new SortingSettings(camera) {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings   = new DrawingSettings(shaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);

            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );

        }

        void DrawUnsupportedShaders () {
                var drawingSettings = new DrawingSettings(
                legacyShaderTagIds[0], new SortingSettings(camera)
            );
            var filteringSettings = FilteringSettings.defaultValue;
            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );
        }

        void Submit()
        {
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        bool Cull (float maxShadowDistance) 
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
                p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
                cullingResults = context.Cull(ref p);
                return true;
            }
            return false;
        }
    }
}