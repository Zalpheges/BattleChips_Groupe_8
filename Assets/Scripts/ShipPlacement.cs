using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipPlacement : MonoBehaviour
{
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
        public Cell.Type cellType;
        public Material material;
    }

    [SerializeField]
    private ShipData[] _shipsDatas;

    [SerializeField]
    private CellMaterial[] _cellsMaterials;

    private Dictionary<int, ShipData> _shipDatas;
    public Dictionary<Cell.Type, Material> CellMaterials;

    [SerializeField]
    private Button _submitButton;

    public int CurrentShipId = -1;
    public GameObject CurrentInstanciatedChip;

    public bool IsShipSelected => CurrentShipId != -1;

    private int _lastRotation;
    private int _nShipsToPlace = 1;

    private bool _displayShipMenu = false;

    private int _iRemove;
    private int _jRemove;

    private void Start()
    {
        _shipDatas = new Dictionary<int, ShipData>();
        CellMaterials = new Dictionary<Cell.Type, Material>();

        foreach (ShipData shipData in _shipsDatas)
            _shipDatas.Add(shipData.id, shipData);

        foreach (CellMaterial cellMat in _cellsMaterials)
            CellMaterials.Add(cellMat.cellType, cellMat.material);
    }

    private void Update()
    {
        if (GameManager.PlacingShips)
        {
            _submitButton.interactable = GameManager.PlacingShips && _nShipsToPlace == 0;
        }
    }

    public ShipData GetShipDataByID(int ID)
    {
        return _shipDatas[ID];
    }

    public void RotateChip()
    {
        if (IsShipSelected)
        {
            _lastRotation += 90;
            _lastRotation %= 360;
            CurrentInstanciatedChip.transform.Rotate(Vector3.up * 90);
        }
    }

    public void ChangeCurrentChip(int id)
    {
        if (IsShipSelected)
        {
            Destroy(CurrentInstanciatedChip);
            _shipDatas[CurrentShipId].button.interactable = true;
        }

        _shipDatas[id].button.interactable = false;
        CurrentShipId = id;
    }

    public void PrevisualizeShipOnCell(Transform cellTransform)
    {
        GameObject newChip = _shipDatas[CurrentShipId].prefab;
        CurrentInstanciatedChip = Instantiate(newChip, cellTransform.position, cellTransform.rotation);
        CurrentInstanciatedChip.transform.SetParent(cellTransform.parent);
        CurrentInstanciatedChip.transform.Rotate(cellTransform.up * _lastRotation);
    }

    public bool PlaceChip(Vector2Int cellPosition, Player player)
    {
        int dir = _lastRotation / 90;
        Vector2Int vect = Ship.DirToVector(dir);

        int i = cellPosition.x, j = cellPosition.y;
        int length = _shipDatas[CurrentShipId].length;

        if (!player.IsSpaceFree(i, j, length, vect))
            return false;

        CurrentInstanciatedChip.GetComponentInChildren<Ship>().direction = vect;
        player.AddShipToGrid(CurrentInstanciatedChip, i, j, length, vect);

        int trigDir = dir % 2 == 1 ? (dir + 2) % 4 : dir; //inverse 1 and 3 
        ClientManager.AddShip(CurrentShipId, i, j, trigDir, length);
        --_nShipsToPlace;

        return true;
    }

    void OnGUI()
    {
        if (_displayShipMenu && GameManager.PlacingShips)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(CurrentInstanciatedChip.transform.position);
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, 300, 400), GUI.skin.box);

            GUIStyle labelStyle = new GUIStyle("Label") { fontSize = 32 };
            GUIStyle buttonStyle = new GUIStyle("Button") { fontSize = 32 };

            GUILayout.Label("Remove this ship ?", labelStyle);

            if (GUILayout.Button("Remove it", buttonStyle))
            {
                GameManager.RemoveShip(_iRemove, _jRemove);
                ClientManager.RemoveShip(_iRemove, _jRemove);

                ++_nShipsToPlace;
                _shipDatas[CurrentInstanciatedChip.GetComponentInChildren<Ship>().id].button.interactable = true;
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

    public void OnCellClicked(Cell cell)
    {
        Player player = cell.transform.parent.GetComponent<Player>();
        if (_displayShipMenu)
            return;
        if (GameManager.MyTurn && !player.You)
        {
            ClientManager.Shoot(player.Id, cell.Position.x, cell.Position.y);
        }
        else if (GameManager.CurrentState == GameManager.State.PlacingShips)
        {
            if (IsShipSelected)
            {
                if (PlaceChip(cell.Position, player))
                {
                    CurrentInstanciatedChip = null;
                    CurrentShipId = -1;
                }
            }
            else
            {
                if (cell.Ship != null)
                {
                    CurrentInstanciatedChip = cell.Ship;
                    _iRemove = cell.Position.x;
                    _jRemove = cell.Position.y;
                    _displayShipMenu = true;
                }
            }
        }
    }

}
