using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;

    [SerializeField] private GameObject cameraPrefab;

    [Space(5)]

    [SerializeField] private Vector3 offSetPosition;

    [Space(5)]

    [SerializeField] private Vector3 offSetRotation;

    private CinemachineClearShot cinemachineClearShot;
    private int index = 0;

    private void Awake()
    {
        instance = this;

        cinemachineClearShot = GetComponent<CinemachineClearShot>();
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
        index = (index - 1);
        if(index < 0)
            index = cinemachineClearShot.ChildCameras.Length - 1;
        
        ChangePriority(index);
    }

    public static void InitCamera(List<Transform> playersTransform)
    {
        foreach (Transform playerTransform in playersTransform)
        {

            GameObject cameraObject = Instantiate(instance.cameraPrefab, instance.cameraPrefab.transform.position, playerTransform.rotation);
            cameraObject.transform.position = playerTransform.position + instance.offSetPosition;
            cameraObject.transform.Rotate(instance.offSetRotation);
            cameraObject.transform.SetParent(instance.transform, true);

        }

        instance.cinemachineClearShot.ChildCameras[0].Priority = 1;
    }

}
