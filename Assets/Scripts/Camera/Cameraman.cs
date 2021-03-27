using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine;
using System.Linq;
using Frictionless;

public class Cameraman : MonoBehaviour
{
    // MY NAME IS...
    private class Shake
    {
        private float _trauma;

        public void Update(Cinemachine.CinemachineComposer composer)
        {
            _trauma = Mathf.Clamp01(_trauma - Time.deltaTime * 1.25f);
            composer.m_TrackedObjectOffset = Random.insideUnitSphere * (_trauma * _trauma) * 2f;
        }

        public void AddTrauma(float v=0.25f)
        {
            _trauma += v;
        }
    }

    public Vector3 OriginalOffset = new Vector3(0, 22f, -12f);

    public float InLerpSpeed = 10f;
    public float OutLerpSpeed = 5f;
    public Vector3 AdditionalOffset = new Vector3(0f, 0f, 10f);
    public float OffsetFactor = 0f;

    private Vector3 _currentOffset = Vector3.zero;
    private bool _isPlayerOccluded;

    private Shake _shake = new Shake();

    void OnEnable()
    {
        ServiceFactory.Instance.Resolve<MessageRouter>().AddHandler<Catacumba.Events.OnPlayerHit>(OnPlayerHit);
        ServiceFactory.Instance.Resolve<MessageRouter>().AddHandler<Catacumba.Events.OnPlayerDamaged>(OnPlayerDamaged);
    }

    void OnDisable()
    {
        ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<Catacumba.Events.OnPlayerHit>(OnPlayerHit);
        ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<Catacumba.Events.OnPlayerDamaged>(OnPlayerDamaged);
    }

    private void OnPlayerHit(Catacumba.Events.OnPlayerHit msg)
    {
        _shake.AddTrauma(0.5f);
    }

    private void OnPlayerDamaged(Catacumba.Events.OnPlayerDamaged msg)
    {
        _shake.AddTrauma(0.8f);
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraManager.Follow == null) return;
        if (CameraManager.Object == null) return;

        _currentOffset += Vector3.ClampMagnitude(GetAdditionalOffset(ref _isPlayerOccluded) - _currentOffset, 1f) * Time.deltaTime * (_isPlayerOccluded ? InLerpSpeed : OutLerpSpeed);
        CameraManager.Transposer.m_FollowOffset = OriginalOffset + _currentOffset;

        _shake.Update(CameraManager.Composer);
    }

    Vector3 GetAdditionalOffset(ref bool isPlayerOccluded)
    {
        Transform target = CameraManager.Follow;
        Vector3 startPos   = target.position;
        Vector3 dir        = new Vector3(0f, 0f, -1f);
        int targetLayer    = (1 << LayerMask.NameToLayer("Level"));

        RaycastHit hitInfo; 
        isPlayerOccluded = Physics.Raycast(
            startPos, 
            dir.normalized, 
            out hitInfo,
            5f,
            targetLayer
        );

        if (!isPlayerOccluded)
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
    private static Cinemachine.CinemachineComposer _Composer;

    public static GameObject Object { get { return CameraObject; } }

    public static Cinemachine.CinemachineTransposer Transposer { get {return _Transposer; } }
    public static Cinemachine.CinemachineComposer Composer { get { return _Composer; } }

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
        //_Transposer.m_FollowOffset = Cameraman.OriginalOffset;

        Cameraman cameraman = VirtualCameraObject.AddComponent<Cameraman>();

        var aim = VirtualCamera.AddCinemachineComponent<Cinemachine.CinemachineComposer>();
        _Composer = aim;
        return VirtualCameraObject;
    }
}
