using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{

    [SerializeField] GameObject prefab;

    [SerializeField] Transform StartTransform;
    [SerializeField] Transform EndTransform;
    // Start is called before the first frame update
    private GameObject temp;
    void Start()
    {
        StartCoroutine(Create());
    }

    IEnumerator Create()
    {
        yield return new WaitForSeconds(2f);
        temp = Instantiate(prefab);
        Missile missile = temp.GetComponent<Missile>();
        missile.Init(StartTransform.position, EndTransform.position, true);

    }
}
