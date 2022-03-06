using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private static Main instance;

    [SerializeField] private GameObject mainPlayerField;
    [SerializeField] private GameObject otherPlayerField;

    private void Awake()
    {
        instance = this;
    }

}
