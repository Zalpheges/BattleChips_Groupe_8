using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject CanvasSelection;
    public Button SubmitButton;

    private GameManager _instance;
    public enum PlayerState
    {
        PlacingShips,
        Waiting,
        Aiming
    }
    public static int CurrentShipId = -1;
    public static GameObject CurrentInstanciatedChip;
    public static float LastRotation;
    public static int NShipsToPlace = 1;
    public static PlayerState CurrentState;
    public static Dictionary<int, Button> ChipsButtons = new Dictionary<int, Button>();
    public static Dictionary<int, int> ChipsLengths = new Dictionary<int, int>();
    public static Dictionary<PlayerCell.CellType, Material> CellMaterials = new Dictionary<PlayerCell.CellType, Material>();
    public static bool Boarded = false;
    public static bool PlacingShips => CurrentState == PlayerState.PlacingShips;
    [Serializable]
    private struct ShipsData
    {
        public Button button;
        public int shipId;
        public int length;
    }
    [Serializable]
    private struct CellMaterial
    {
        public PlayerCell.CellType cellType;
        public Material material;
    }
    [SerializeField] private ShipsData[] _shipsDatas;
    [SerializeField] private CellMaterial[] _cellsMaterials;

    private void Awake()
    {
        _instance = this;
        foreach (ShipsData button in _shipsDatas)
        {
            ChipsButtons.Add(button.shipId, button.button);
            ChipsLengths.Add(button.shipId, button.length);
        }
        foreach (CellMaterial cellMat in _cellsMaterials)
        {
            CellMaterials.Add(cellMat.cellType, cellMat.material);
        }
    }

    private void Update()
    {
        if (CurrentState == PlayerState.PlacingShips)
        {
            SubmitButton.interactable = Boarded ? false : NShipsToPlace == 0;
        }
    }
    public static void RotateChip()
    {
        if (CurrentShipId == -1)
            return;
        LastRotation += 90;
        LastRotation %= 360;
        CurrentInstanciatedChip.transform.Rotate(Vector3.up * 90);
    }
    public void ChangeCurrentChip(int id)
    {
        if (CurrentShipId != -1)
        {
            Destroy(CurrentInstanciatedChip);
            ChipsButtons[CurrentShipId].interactable = true;
        }
        ChipsButtons[id].interactable = false;
        CurrentShipId = id;
    }
}
