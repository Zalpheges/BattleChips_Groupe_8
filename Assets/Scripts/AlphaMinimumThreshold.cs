using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class AlphaMinimumThreshold : MonoBehaviour
{
    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }
}
