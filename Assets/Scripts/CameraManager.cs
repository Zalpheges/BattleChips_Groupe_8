using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;

    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private CinemachineClearShot cinemachineClearShot;
    private int index = 0;

    private void Awake()
    {
        instance = this;

        cinemachineClearShot = GetComponent<CinemachineClearShot>();
    }

    private void Start()
    {

    }

    private void ChangePriority(int index)
    {
        cinemachineClearShot.LiveChild.Priority = 0;
        cinemachineClearShot.ChildCameras[index].Priority = 1;
    }

    public void Next()
    {
        index = (index + 1) % cinemachineClearShot.ChildCameras.Length;
        ChangePriority(index);
    }

    public void Previous()
    {
        index = (index + 1) % cinemachineClearShot.ChildCameras.Length;
        ChangePriority(index);
    }

    public static void InitCamera(List<Vector3> playersTransform)
    {
        foreach (Vector3 playerTransform in playersTransform)
        {
            GameObject cameraObject = Instantiate(instance.cameraPrefab);
            cameraObject.transform.position += playerTransform;
            cameraObject.transform.SetParent(instance.transform, true);
        }
        instance.ChangePriority(0);
    }

}
