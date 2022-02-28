using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private CinemachineClearShot cinemachineClearShot;
    private int index = 0;
    private void Awake()
    {
        cinemachineClearShot = GetComponent<CinemachineClearShot>();
    }
    // Start is called before the first frame update
    
    public void Next()
    {
        
        if (cinemachineClearShot != null)
        {
            cinemachineClearShot.LiveChild.Priority = 0;
            index = (index + 1) % cinemachineClearShot.ChildCameras.Length;
            cinemachineClearShot.ChildCameras[index].Priority = 1;
        }
    }

}
