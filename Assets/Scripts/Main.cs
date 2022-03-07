using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public static Main instance;
    public GameObject canvasSelection;
    public Button submitButton;
    public static int currentId = -1;
    public static GameObject currentInstanciatedChip;
    public static float lastRotation;
    public static int nShipsToPlace = 5;
    public static Dictionary<int, Button> chipsButtons = new Dictionary<int, Button>();
    public static Dictionary<int, int> chipsLengths = new Dictionary<int, int>();
    public enum PlayerState
    {
        PlacingChips,
        Waiting,
        Aiming
    }

    public static PlayerState currentState;


    [Serializable]
    private struct ShipsData
    {
        public Button button;
        public int shipId;
        public int length;
    }
    [SerializeField] private ShipsData[] shipsDatas;
    [SerializeField] private LayerMask _cellsLayer;
    private PlayerCell _currentCell;

    private void Awake()
    {
        instance = this;
        foreach (ShipsData button in shipsDatas)
        {
            chipsButtons.Add(button.shipId, button.button);
            chipsLengths.Add(button.shipId, button.length);
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
            if (_currentCell != tmp)
            {
                if(_currentCell != null)
                    _currentCell.MouseExit();
                _currentCell = tmp;
                _currentCell.MouseEnter();
            }
        }
        if (Input.GetMouseButtonDown(1))
            RotateChip();
        submitButton.interactable = nShipsToPlace == 0;
    }

    public void RotateChip()
    {
        if (currentId == -1)
            return;
        lastRotation += 90;
        currentInstanciatedChip.transform.Rotate(Vector3.up * 90);
    }

    public void ChangeCurrentChip(int id)
    {
        Destroy(currentInstanciatedChip);
        if (currentId != -1)
            chipsButtons[currentId].interactable = true;
        chipsButtons[id].interactable = false;
        currentId = id;
    }
}
