using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;

    public static float transitionDelay;

    [SerializeField] 
    private GameObject cameraPrefab;

    [Space(5)]

    [SerializeField]
    private Vector3 offSetPosition;

    [Space(5)]

    [SerializeField]
    private Vector3 offSetRotation;

    [Space(20)]

    private CinemachineClearShot cinemachineClearShot;
    private int index = 0;
    

    private void Awake()
    {
        _instance = this;

        cinemachineClearShot = GetComponent<CinemachineClearShot>();
    }

    private void Start()
    {
        transitionDelay = cinemachineClearShot.m_DefaultBlend.m_Time;
    }


    private void ChangeCamera(int index)
    {
        cinemachineClearShot.LiveChild.Priority = 0;
        cinemachineClearShot.ChildCameras[index].Priority = 1;
    }

    public void Next()
    {
        index = (index + 1) % cinemachineClearShot.ChildCameras.Length;
        ChangeCamera(index);
    }

    public void Previous()
    {
        index = (index - 1);
        if (index < 0)
            index = cinemachineClearShot.ChildCameras.Length - 1;

        ChangeCamera(index);
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

        _instance.index = GameManager.MyID;
        _instance.cinemachineClearShot.ChildCameras[GameManager.MyID].Priority = 1;
    }

    public static void DestroyCamera(CinemachineVirtualCamera cinemachineVirtualCamera, int nextCamera)
    {

        if (nextCamera < _instance.cinemachineClearShot.ChildCameras.Length)
            _instance.ChangeCamera(nextCamera);
        else
            _instance.ChangeCamera(_instance.index);

        Destroy(cinemachineVirtualCamera.gameObject, transitionDelay);
    }
}
