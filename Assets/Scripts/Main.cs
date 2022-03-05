using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public static Dictionary<GameObject, Button> chipsButtons = new Dictionary<GameObject, Button>();

    [Serializable]
    private struct LinkedButton
    {
        public Button button;
        public GameObject prefabChip;
    }
    [SerializeField] private LinkedButton[] linkedButtons;
    //[SerializeField] private GameObject mainPlayerField;
    //[SerializeField] private GameObject otherPlayerField;

    private void Awake()
    {
        instance = this;
        foreach (LinkedButton button in linkedButtons)
        {
            chipsButtons.Add(button.prefabChip, button.button);
        }
    }

    private void ChangeCurrentChip(GameObject chip)
    {
        currentSelectedChip = chip;
    }
    public void DisableButton(Button button)
    {
        button.interactable = false;
    }
}
