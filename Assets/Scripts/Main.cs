using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private static Main _instance;
    public static int currentId;
    public static GameObject currentInstanciatedChip;
    public static float lastRotation;
    public enum PlayerState
    {
        PlacingChips,
        Waiting,
        Aiming
    }

    public static PlayerState currentState;

    public static Dictionary<int, Button> chipsButtons = new Dictionary<int, Button>();

    [Serializable]
    private struct LinkedButton
    {
        public Button button;
        public int shipId;
    }
    [SerializeField] private LinkedButton[] linkedButtons;
    [SerializeField] private LayerMask _cellsLayer;
    private PlayerCell currentCell;

    private void Awake()
    {
        _instance = this;
        foreach (LinkedButton button in linkedButtons)
        {
            chipsButtons.Add(button.shipId, button.button);
        }
    }

    private void Update()
    {
        PlayerCell tmp;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _cellsLayer))
        {
            tmp = hitInfo.transform.GetComponent<PlayerCell>();
            if (Input.GetMouseButtonDown(0))
                tmp.PointerClick();
            if (currentCell != tmp)
            {
                if(currentCell != null)
                    currentCell.MouseExit();
                currentCell = tmp;
                currentCell.MouseEnter();
            }
        }
    }

    public void ChangeCurrentChip(int id)
    {
        Destroy(currentInstanciatedChip);
        if (currentId != 1)
            chipsButtons[currentId].interactable = true;
        chipsButtons[id].interactable = false;
        currentId = id;
    }
}
