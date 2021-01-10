using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catacumba.Rendering
{
    public partial class RPCameraRenderer 
    {
        partial void DrawGizmos();
        partial void PrepareForSceneWindow ();
        partial void PrepareBuffer();

        #if UNITY_EDITOR

        partial void DrawGizmos () {
            if (Handles.ShouldRenderGizmos()) {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }

        partial void PrepareForSceneWindow () {
            if (camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }

        #endif

        #if UNITY_EDITOR 

        string SampleName { get; set; }
	
        partial void PrepareBuffer () {
            buffer.name = camera.name;
        }

        #else

        const string SampleName = bufferName;

        #endif
    }
}