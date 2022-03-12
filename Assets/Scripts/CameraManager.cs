using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;

    public static float transitionDelay { get; private set; }

    [SerializeField] 
    private GameObject cameraPrefab;

    [Space(5)]

    [SerializeField]
    private Vector3 offSetPosition;

    [Space(5)]

    [SerializeField]
    private Vector3 offSetRotation;

    [Space(20)]

    private CinemachineClearShot _cinemachineClearShot;
    private int _index = 0;

    private void Awake()
    {
        _instance = this;

        _cinemachineClearShot = GetComponent<CinemachineClearShot>();
    }

    private void Start()
    {
        transitionDelay = _cinemachineClearShot.m_DefaultBlend.m_Time;
    }

    private void ChangeCamera(int index)
    {
        _cinemachineClearShot.LiveChild.Priority = 0;
        _cinemachineClearShot.ChildCameras[index].Priority = 1;
    }

    public void Next()
    {
        _index = (_index + 1) % _cinemachineClearShot.ChildCameras.Length;
        ChangeCamera(_index);
    }

    public void Previous()
    {
        _index = (_index - 1);
        if (_index < 0)
            _index = _cinemachineClearShot.ChildCameras.Length - 1;

        ChangeCamera(_index);
    }

    public static CinemachineVirtualCamera CreateCamera(Transform objectTransform, Vector3 offSetPosition, Vector3 offSetRotation)
    {
        GameObject cameraObject = Instantiate(_instance.cameraPrefab, _instance.cameraPrefab.transform.position, objectTransform.rotation);
        cameraObject.transform.position = objectTransform.position + _instance.offSetPosition;
        cameraObject.transform.Rotate(_instance.offSetRotation);
        cameraObject.transform.SetParent(_instance.transform, true);

        CinemachineVirtualCamera cinemachineVirtualCamera = cameraObject.GetComponent<CinemachineVirtualCamera>();

        return cinemachineVirtualCamera;
    }

    public static void InitCamera(List<Transform> playersTransform)
    {
        foreach (Transform playerTransform in playersTransform)
            CreateCamera(playerTransform, _instance.offSetPosition, _instance.offSetRotation);

        _instance._index = GameManager.MyID;
        _instance._cinemachineClearShot.ChildCameras[GameManager.MyID].Priority = 1;
    }

    public static void DestroyCamera(CinemachineVirtualCamera cinemachineVirtualCamera, int nextCamera)
    {

        if (nextCamera < _instance._cinemachineClearShot.ChildCameras.Length)
            _instance.ChangeCamera(nextCamera);
        else
            _instance.ChangeCamera(_instance._index);

        Destroy(cinemachineVirtualCamera.gameObject, transitionDelay);
    }
}
