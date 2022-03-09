using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    [SerializeField] private GameObject firePrefab;

    [SerializeField] private float fireOffSetY;

    private void OnDestroy()
    {
        Instantiate(firePrefab, transform.position + Vector3.up * fireOffSetY, Quaternion.identity);
    }
}
