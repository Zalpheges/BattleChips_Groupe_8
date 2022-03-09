using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject CanvasSelection;
    public Button SubmitButton;

    private ShipPlacement _shipPlacement;
    private static GameManager _instance;
    public enum PlayerState
    {
        PlacingShips,
        Playing
    }
    public static int CurrentShipId = -1;
    public static GameObject CurrentInstanciatedChip;
    public static int LastRotation;
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
    private bool _displayShipMenu = false;
    private Player _player;

    private int _iRemove;
    private int _jRemove;

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
        if (PlacingShips)
        {
            SubmitButton.interactable = Boarded ? false : NShipsToPlace == 0;
        }
    }
    void OnGUI()
    {
        if (_displayShipMenu && !Boarded)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(CurrentInstanciatedChip.transform.position);
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, 300, 400), GUI.skin.box);

            GUIStyle labelStyle = new GUIStyle("Label") { fontSize = 32 };
            GUIStyle buttonStyle = new GUIStyle("Button") { fontSize = 32 };

            GUILayout.Label("Remove this ship ?", labelStyle);

            if (GUILayout.Button("Remove it", buttonStyle))
            {
                _player.RemoveShip(_iRemove, _jRemove);
                ClientManager.RemoveShip(_iRemove, _jRemove);

                ++NShipsToPlace;
                ShipDatas[CurrentInstanciatedChip.GetComponentInChildren<Ship>().id].button.interactable = true;
                Destroy(CurrentInstanciatedChip);
                CurrentShipId = -1;
                CurrentInstanciatedChip = null;

                _displayShipMenu = false;
            }

            if (GUILayout.Button("Cancel", buttonStyle))
            {
                CurrentInstanciatedChip = null;

                _displayShipMenu = false;
            }
            GUILayout.EndArea();
        }
    }
    public static void OnCellClicked(PlayerCell cell)
    {
        Player player = cell.transform.parent.GetComponent<Player>();
        if (_instance._displayShipMenu)
            return;
        if (ClientManager.MyTurn && !player.you)
        {
            ClientManager.Shoot(player.id, cell.position.x, cell.position.y);
        }
        else if (PlacingShips)
        {
            if (IsShipSelected)
            {
                if (PlaceChip(cell.position, player))
                {
                    CurrentInstanciatedChip = null;
                    CurrentShipId = -1;
                }
            }
            else
            {
                if (cell.ship != null)
                {
                    CurrentInstanciatedChip = cell.ship;
                    _instance._iRemove = cell.position.x;
                    _instance._jRemove = cell.position.y;
                    _instance._displayShipMenu = true;
                }
            }
        }
        Debug.Log(cell.position.ToString() + cell.type.ToString());
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
    public static void PrevisualizeShipOnCell(Transform cellTransform)
    {
        GameObject newChip = ShipDatas[CurrentShipId].prefab;
        CurrentInstanciatedChip = Instantiate(newChip, cellTransform.position, cellTransform.rotation);
        CurrentInstanciatedChip.transform.SetParent(cellTransform.parent);
        CurrentInstanciatedChip.transform.Rotate(cellTransform.up * LastRotation);
    }
    public static bool PlaceChip(Vector2Int cellPosition, Player player)
    {
        int dir = LastRotation / 90;
        Vector2Int vect = _instance._shipPlacement.IntToVector(dir);

        int i = cellPosition.x, j = cellPosition.y;
        int length = ShipDatas[CurrentShipId].length;

        if (!player.IsSpaceFree(i, j, length, vect))
            return false;

        CurrentInstanciatedChip.GetComponentInChildren<Ship>().direction = vect;
        player.AddShipToGrid(CurrentInstanciatedChip, i, j, length, vect);

        int trigDir = dir % 2 == 1 ? (dir + 2) % 4 : dir; //inverse 1 and 3
        ClientManager.AddShip(CurrentShipId, i, j, trigDir, length);
        --NShipsToPlace;

        return true;
    }
}
