using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catacumba.Rendering
{
    [System.Serializable]
    public class ShadowSettings 
    {
        [System.Serializable]
        public struct Directional {
            public TextureSize atlasSize;
        }

        public struct Point {
            public TextureSize atlasSize;
            public float filter;
        }

        public enum TextureSize {
            _256 = 256, _512 = 512, _1024 = 1024,
            _2048 = 2048, _4096 = 4096, _8192 = 8192
        }

        [Min(0f)]
        public float maxDistance = 100f;

        public Directional directional = new Directional {
            atlasSize = TextureSize._1024
        };

        public Point point = new Point {
            atlasSize = TextureSize._1024,
            filter = 1f
        };
    }
    
    [CreateAssetMenu(menuName="Rendering/Catacumba Rendering Pipeline", fileName="CatacumbaRenderPipeline")]
    public class RPAsset : RenderPipelineAsset
    {
        [SerializeField]
        ShadowSettings shadows = default;

        public void OnEnable()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
        }

        protected override RenderPipeline CreatePipeline()
        {
            return new RP(shadows);
        }
    }
}