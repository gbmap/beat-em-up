using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine;
using System.Linq;

public class Cameraman : MonoBehaviour
{
    public static Vector3 OriginalOffset = new Vector3(0, 16f, -9.89f);

    public float LerpSpeed = 5f;
    public Vector3 AdditionalOffset = new Vector3(0f, 0f, 10f);
    public float OffsetFactor = 0f;

    private Vector3 _currentOffset = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (CameraManager.Object == null) return;

        _currentOffset += Vector3.ClampMagnitude(GetAdditionalOffset2() - _currentOffset, 1f) * Time.deltaTime * LerpSpeed;
        CameraManager.Transposer.m_FollowOffset = OriginalOffset + _currentOffset;
    }

    Vector3 GetAdditionalOffset()
    {
        Transform target   = CameraManager.Follow;
        Vector3 startPos   = target.position + OriginalOffset;
        Vector3 endPos     = CameraManager.Follow.position;
        Vector3 dir        = (endPos - startPos);
        int targetLayer    = (1 << LayerMask.NameToLayer("Level"))
                           | (1 << LayerMask.NameToLayer("Player"));
        RaycastHit hitInfo; 

        bool collides = Physics.Raycast(
            startPos, 
            dir.normalized, 
            out hitInfo,
            dir.magnitude,
            targetLayer);

        if (!collides)
            return Vector3.zero;

        bool collidesWithPlayer = hitInfo.transform != target;
        if (!collidesWithPlayer) return Vector3.zero; 

        float zDelta = Mathf.Abs((Mathf.Abs(hitInfo.transform.position.z) - Mathf.Abs(target.position.z)));
        OffsetFactor = Mathf.Max(0f, hitInfo.collider.bounds.size.y - zDelta - 2.0f); 
        OffsetFactor = OffsetFactor / hitInfo.collider.bounds.size.y;
        OffsetFactor *= OffsetFactor;
        return AdditionalOffset * OffsetFactor;
    }

    Vector3 GetAdditionalOffset2()
    {
        Transform target = CameraManager.Follow;
        Vector3 startPos   = target.position;
        Vector3 dir        = new Vector3(0f, 0f, -1f);
        int targetLayer    = (1 << LayerMask.NameToLayer("Level"));

        RaycastHit hitInfo; 
        bool collides = Physics.Raycast(
            startPos, 
            dir.normalized, 
            out hitInfo,
            5f,
            targetLayer);

        if (!collides)
            return Vector3.zero;

        
        float upperValue = (hitInfo.collider.bounds.size.y - hitInfo.collider.bounds.extents.z) - 1f;
        float lowerValue = target.position.z;

        float zDelta = Mathf.Abs(hitInfo.transform.position.z - lowerValue);
        OffsetFactor = Mathf.Max(0f, upperValue - zDelta); 
        OffsetFactor = OffsetFactor / upperValue;
        //OffsetFactor *= OffsetFactor;
        return AdditionalOffset * OffsetFactor;
    }
}

[CommandPrefix("camera.")]
public static class CameraManager
{
    private static GameObject CameraObject;
    private static GameObject VirtualCameraObject;
    private static Cinemachine.CinemachineVirtualCamera VirtualCamera;
    private static Cinemachine.CinemachineTransposer _Transposer;

    public static GameObject Object { get { return CameraObject; } }

    public static Cinemachine.CinemachineTransposer Transposer { get {return _Transposer; } }

    [Command("follow_target")] 
    public static Transform Follow
    {
        get { return VirtualCamera.Follow; }
        set { VirtualCamera.Follow = value; }
    }

    [Command("follow_offset")]
    public static Vector3 FollowOffset
    {
        get { return _Transposer.m_FollowOffset; }
        set { _Transposer.m_FollowOffset = value; }

    }

    [Command("look_target")]  
    public static Transform LookAt
    {
        get { return VirtualCamera.LookAt; }
        set { VirtualCamera.LookAt = value; }
    }

    [Command("target")]
    public static Transform Target
    {
        set 
        { 
            Follow = value;
            LookAt = value;  
        }
    }

    [Command("spawn")]
    public static GameObject Spawn()
    {
        CameraObject = Camera.main?.gameObject;
        if (CameraObject == null)
        {
            CameraObject = new GameObject("MainCamera");
            CameraObject.AddComponent<Camera>();
            CameraObject.tag = "MainCamera";
        }

        if (VirtualCameraObject)
            GameObject.Destroy(VirtualCameraObject);

        VirtualCameraObject = new GameObject("VCam");
        VirtualCamera = VirtualCameraObject.AddComponent<Cinemachine.CinemachineVirtualCamera>();

        _Transposer = VirtualCamera.AddCinemachineComponent<Cinemachine.CinemachineTransposer>();
        _Transposer.m_BindingMode = Cinemachine.CinemachineTransposer.BindingMode.WorldSpace;
        _Transposer.m_FollowOffset = Cameraman.OriginalOffset;

        VirtualCameraObject.AddComponent<Cameraman>();

        var aim = VirtualCamera.AddCinemachineComponent<Cinemachine.CinemachineComposer>();
        return VirtualCameraObject;
    }
}
