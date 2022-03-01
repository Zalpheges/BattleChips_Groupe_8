using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private static Main instance;
    public static GameObject currentSelectedChip;
    public static GameObject currentInstanciatedChip;
    public static float lastRotation;
    public enum PlayerState
    {
        PlacingChips,
        Waiting,
        Aiming
    }
    public static PlayerState currentState;
    [SerializeField] private GameObject mainPlayerField;
    [SerializeField] private GameObject otherPlayerField;

    private void Awake()
    {
        instance = this;
    }

    private void ChangeCurrentChip(GameObject chip)
    {
        currentSelectedChip = chip;
    }
}
