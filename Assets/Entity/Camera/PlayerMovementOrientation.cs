using Catacumba.Exploration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba
{
    public enum EPlayerMovementOrientationType
    {
        RelativeToPath,
        RelativeToCamera,
        RelativeToTransform
    }

    public struct MovementOrientation
    {
        public Vector3 forward;
        public Vector3 right;
    }

    [RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
    public class PlayerMovementOrientation : MonoBehaviour
    {
        public EPlayerMovementOrientationType Type;

        [Header("Relative To Transform Configuration")]
        public Transform Target;

        private new Cinemachine.CinemachineVirtualCamera camera;

        private void Awake()
        {
            camera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        }

        public MovementOrientation CalculateMovementOrientation()
        {
            switch (Type)
            {
                case EPlayerMovementOrientationType.RelativeToPath:
                    MovementOrientation mo = new MovementOrientation();

                    var dolly = camera.GetCinemachineComponent<Cinemachine.CinemachineTrackedDolly>();
                    if (!dolly || !dolly.m_Path)
                    {
                        Debug.LogError("No Dolly configured to camera!");
                    }

                    var posA = dolly.m_Path.EvaluatePosition(dolly.m_PathPosition);
                    var posB = dolly.m_Path.EvaluatePosition(dolly.m_PathPosition + 0.15f);

                    // If we're out of bounds when sampling the path, sample a point behind the current point.
                    if (Mathf.Approximately(Vector3.Distance(posA, posB), 0f))
                    {
                        posB = dolly.m_Path.EvaluatePosition(dolly.m_PathPosition - 0.15f);
                    }

                    var delta = (posB - posA).normalized;
                    delta.y = 0f;
                    var camRight = Camera.main.transform.right;

                    Vector3 pathRight = delta * (Vector3.Dot(delta, camRight) > 0f ? 1f : -1f);

                    if (delta.sqrMagnitude > 0f)
                    {
                        mo.right = pathRight;
                    }
                    else
                    {
                        mo.right = Camera.main.transform.right;
                    }

                    Vector3 pathForward = Vector3.Cross(pathRight, Vector3.up);
                    pathForward *= Vector3.Dot(Camera.main.transform.forward, pathForward) > 0f ? 1f : -1f;

                    mo.forward = pathForward;
                    return mo;
                case EPlayerMovementOrientationType.RelativeToCamera:
                    return new MovementOrientation { right = camera.transform.right, forward = camera.transform.forward };
                case EPlayerMovementOrientationType.RelativeToTransform:
                    return new MovementOrientation { right = Target.right, forward = Target.forward };
                default: return new MovementOrientation { right = Vector3.zero, forward = Vector3.zero };
            }
        }

    }
}