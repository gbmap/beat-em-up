using UnityEngine;
using UnityEngine.Rendering;

namespace Catacumba.Rendering
{
    public class RPShadows {

        const string bufferName = "Shadows";

        const int maxShadowedDirectionalLightCount = 4,
                  maxShadowedPointLightCount       = 6*12;

        struct ShadowedDirectionalLight {
            public int visibleLightIndex;
        }

        ShadowedDirectionalLight[] shadowedDirectionalLights =
            new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

        struct ShadowedOtherLight {
            public int visibleLightIndex;
            public float slopeScaleBias;
            public float normalBias;
            public bool isPoint;
        }

        ShadowedOtherLight[] shadowedOtherLights =
            new ShadowedOtherLight[maxShadowedPointLightCount];

        CommandBuffer buffer = new CommandBuffer {
            name = bufferName
        };

        ScriptableRenderContext context;
        CullingResults cullingResults;
        ShadowSettings settings;

        int ShadowedDirectionalLightCount,
            ShadowedPointLightCount;

        Vector4 atlasSizes;

        static int  idDirShadowAtlasSize  = Shader.PropertyToID("_ShadowAtlasSize"),
                    idDirShadowAtlas      = Shader.PropertyToID("_ShadowAtlas"),
                    idDirShadowMatrices   = Shader.PropertyToID("_ShadowMatrices"),
                    idPointShadowAtlas    = Shader.PropertyToID("_ShadowPointAtlas"),
                    idPointShadowMatrices = Shader.PropertyToID("_ShadowPointMatrices"),
                    idPointShadowTiles    = Shader.PropertyToID("_ShadowPointTiles");

        Matrix4x4[] dirShadowMatrices     = new Matrix4x4[maxShadowedDirectionalLightCount],
                    pointShadowMatrices   = new Matrix4x4[maxShadowedPointLightCount];

        Vector4[]   pointShadowTiles      = new Vector4[maxShadowedPointLightCount];

        public void Setup (
            ScriptableRenderContext context, CullingResults cullingResults,
            ShadowSettings settings
        ) {
            this.context = context;
            this.cullingResults = cullingResults;
            this.settings = settings;

            ShadowedDirectionalLightCount = 0;
            ShadowedPointLightCount = 0;
        }

        public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex) 
        {
            if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
                light.shadows != LightShadows.None && light.shadowStrength > 0f &&
                cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)) 
            {
                shadowedDirectionalLights[ShadowedDirectionalLightCount] =
                    new ShadowedDirectionalLight {
                        visibleLightIndex = visibleLightIndex
                    };
                
                return new Vector2(light.shadowStrength, ShadowedDirectionalLightCount++);
            }
            return Vector2.zero;
        }

        public bool ReservePointShadows(Light light, int visibleLightIndex, out Vector4 data)
        {
            if (light.shadows == LightShadows.None || light.shadowStrength <= 0f) {
                data = new Vector4(0f, 0f, 0f, -1f);
                return false;
            }

            float maskChannel = -1f;

            int newLightCount = ShadowedPointLightCount + 6;
            if (
                newLightCount > maxShadowedPointLightCount ||
                !cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)
            ) {
                data = new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
                return false;
            }

            shadowedOtherLights[ShadowedPointLightCount] = new ShadowedOtherLight {
                visibleLightIndex = visibleLightIndex,
                slopeScaleBias = 0.039f, //light.shadowBias,
                normalBias = 0.17f, //light.shadowNormalBias,
                isPoint = true
            };

            data = new Vector4(
                light.shadowStrength, ShadowedPointLightCount, 1f, maskChannel
            );

            ShadowedPointLightCount = newLightCount;
            return true;
        }

        public void Render()
        {
            if (ShadowedDirectionalLightCount > 0) {
                RenderDirectionalShadows();
            }
            else {
                buffer.GetTemporaryRT(idDirShadowAtlas, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            }

            if (ShadowedPointLightCount > 0) {
                RenderPointShadows();
            }
            else {
                buffer.SetGlobalTexture(idPointShadowAtlas, idDirShadowAtlas);
            }

            buffer.SetGlobalVector(idDirShadowAtlas, atlasSizes);
            buffer.EndSample(bufferName);
            ExecuteBuffer();

        }

        void RenderDirectionalShadows()
        {
            int atlasSize = (int)settings.directional.atlasSize;
            atlasSizes.x = atlasSize;
            atlasSizes.y = 1/atlasSize;

            buffer.GetTemporaryRT(idDirShadowAtlas, atlasSize, atlasSize,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
            buffer.SetRenderTarget(idDirShadowAtlas, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.ClearRenderTarget(true, false, Color.clear);
            ExecuteBuffer();

            int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
            int tileSize = atlasSize / split;

            for (int i = 0; i < ShadowedDirectionalLightCount; i++) {
                RenderDirectionalShadows(i, split, tileSize);
            }

            buffer.SetGlobalMatrixArray(idDirShadowMatrices, dirShadowMatrices);
            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }

        void RenderDirectionalShadows(int index, int split, int tileSize) {
            ShadowedDirectionalLight light = shadowedDirectionalLights[index];
            var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData
            );
            shadowSettings.splitData = splitData;

            SetTileViewport(index, split, tileSize);
            dirShadowMatrices[index] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, 
                                                            SetTileViewport(index, split, tileSize), 
                                                            split);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            buffer.SetGlobalDepthBias(0f, 3f);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
            buffer.SetGlobalDepthBias(0f, 0f);
        }

        void RenderPointShadows()
        {
            int atlasSize = (int)settings.point.atlasSize;
            atlasSizes.z = atlasSize;
            atlasSizes.w = 1f / atlasSize;
            buffer.GetTemporaryRT(
                idPointShadowAtlas, atlasSize, atlasSize,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
            buffer.SetRenderTarget(
                idPointShadowAtlas,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
            buffer.ClearRenderTarget(true, false, Color.clear);
            buffer.BeginSample(bufferName);
            ExecuteBuffer();

            int tiles = ShadowedPointLightCount;
            int split = GetTileSplits(ShadowedPointLightCount);
            int tileSize = atlasSize / split;
            
            for (int i = 0; i < ShadowedPointLightCount;) {
                RenderPointShadows(i, split, tileSize);
                i += 6;
            }

            buffer.SetGlobalMatrixArray(idPointShadowMatrices, pointShadowMatrices);
            buffer.SetGlobalVectorArray(idPointShadowTiles, pointShadowTiles);
            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }

        int GetTileSplits(int lightCount)
        {
            return Mathf.CeilToInt(Mathf.Sqrt(lightCount));
            /*
            if (lightCount <= 1) { return 1; }
            if (lightCount <= 4) { return 2; }
            if (lightCount <= 6) { return 4; }
            if (lightCount <= 8) { return 6; }
            return 8;
            */
        }

        void RenderPointShadows (int index, int split, int tileSize) {
            ShadowedOtherLight light = shadowedOtherLights[index];
            var shadowSettings =
                new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
            float texelSize = 2f / tileSize;
            float filterSize = texelSize * ((float)settings.point.filter + 1f);
            float bias = light.normalBias * filterSize * 1.4142136f;
            float tileScale = 1f / split;
            float fovBias = Mathf.Atan(1f + bias + filterSize) * Mathf.Rad2Deg * 2f - 90f;
            for (int i = 0; i < 6; i++) {
                cullingResults.ComputePointShadowMatricesAndCullingPrimitives(
                    light.visibleLightIndex, (CubemapFace)i, fovBias,
                    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                    out ShadowSplitData splitData
                );
                viewMatrix.m11 = -viewMatrix.m11;
                viewMatrix.m12 = -viewMatrix.m12;
                viewMatrix.m13 = -viewMatrix.m13;

                shadowSettings.splitData = splitData;
                int tileIndex = index + i;
                Vector2 offset = SetTileViewport(tileIndex, split, tileSize);
                SetOtherTileData(tileIndex, offset, tileScale, bias);
                pointShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
                    projectionMatrix * viewMatrix, offset, tileScale
                );

                buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
                ExecuteBuffer();
                context.DrawShadows(ref shadowSettings);
                buffer.SetGlobalDepthBias(0f, 0f);
            }
        }

        void SetOtherTileData (int index, Vector2 offset, float scale, float bias) {
            float border = atlasSizes.w * 0.5f;
            Vector4 data;
            data.x = offset.x * scale + border;
            data.y = offset.y * scale + border;
            data.z = scale - border - border;
            data.w = bias;
            pointShadowTiles[index] = data;
        }

        void SetTileViewport (int index, int split) {
            Vector2 offset = new Vector2(index % split, index / split);
        }

        Vector2 SetTileViewport (int index, int split, float tileSize) {
            Vector2 offset = new Vector2(index % split, index / split);
            buffer.SetViewport(new Rect(
                offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
            ));
            return offset;
        }

        Matrix4x4 ConvertToAtlasMatrix (Matrix4x4 m, Vector2 offset, float scale) {
            if (SystemInfo.usesReversedZBuffer) {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }

            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            return m;
        }

        void ExecuteBuffer () {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        public void Cleanup()
        {
            buffer.ReleaseTemporaryRT(idDirShadowAtlas);
            if (ShadowedPointLightCount > 0)
                buffer.ReleaseTemporaryRT(idPointShadowAtlas);
            ExecuteBuffer();
        }
    }
}