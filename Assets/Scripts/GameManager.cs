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
    public static PlayerState CurrentState;
    public static int CurrentShipId = -1;
    public static GameObject CurrentInstanciatedChip;
    public static float LastRotation;
    public static int NShipsToPlace = 1;
    public static Dictionary<int, ShipData> ShipDatas = new Dictionary<int, ShipData>();
    public static Dictionary<PlayerCell.CellType, Material> CellMaterials = new Dictionary<PlayerCell.CellType, Material>();
    public static bool Boarded = false;
    public static bool PlacingShips => CurrentState == PlayerState.PlacingShips;
    public static bool IsShipSelected => CurrentShipId != -1;

    [Serializable]
    public struct ShipData
    {
        public GameObject prefab;
        public Button button;
        public int id;
        public int length;
    }
    [Serializable]
    private struct CellMaterial
    {
        public PlayerCell.CellType cellType;
        public Material material;
    }
    [SerializeField] private ShipData[] _shipsDatas;
    [SerializeField] private CellMaterial[] _cellsMaterials;

    private void Awake()
    {
        _instance = this;
        foreach (ShipData shipData in _shipsDatas)
        {
            ShipDatas.Add(shipData.id, shipData);
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
        if (IsShipSelected)
        {
            Destroy(CurrentInstanciatedChip);
            ShipDatas[CurrentShipId].button.interactable = true;
        }
        ShipDatas[id].button.interactable = false;
        CurrentShipId = id;
    }
    public static void PrevisualizeShipOnCell(Transform cellTransform)
    {
        GameObject newChip = ShipDatas[CurrentShipId].prefab;
        CurrentInstanciatedChip = Instantiate(newChip, cellTransform.position, cellTransform.rotation);
        CurrentInstanciatedChip.transform.SetParent(cellTransform.parent);
        CurrentInstanciatedChip.transform.Rotate(cellTransform.up * LastRotation);
    }
}
