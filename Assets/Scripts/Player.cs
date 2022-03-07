using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject prefabCell;
    public int id;
    public bool you;
    private const int WIDTH = 10, HEIGHT = 10;
    private float _cellSize;
    private Vector3 _gridStart;
    private PlayerCell[,] _grid;

    private bool _displayShipMenu = false;
    private int _iRemove;
    private int _jRemove;



    public void Initialize()
    {
        _cellSize = prefabCell.transform.localScale.x;
        _grid = new PlayerCell[HEIGHT, WIDTH];
        _gridStart = transform.position +
                    transform.forward * (WIDTH - 1) / 2f * _cellSize -
                    transform.right * (HEIGHT - 1) / 2f * _cellSize
                    +Vector3.up * 10;


        for (int x = 0; x < HEIGHT; x++)
        {
            for (int y = 0; y < WIDTH; y++)
            {
                GameObject cellGo = Instantiate(prefabCell, transform);
                cellGo.transform.position = _gridStart + 
                                            x * _cellSize * transform.right -
                                            _cellSize * y * transform.forward;
                cellGo.transform.localRotation = Quaternion.identity;
                PlayerCell cell = cellGo.GetComponent<PlayerCell>();
                cell.position = new Vector2Int(x, y);
                cell.type = PlayerCell.CellType.None;
                cell.onClick += OnCellClicked;
                _grid[x, y] = cell;
            }
        }
    }
    void OnCellClicked(PlayerCell cell)
    {
        if (_displayShipMenu)
            return;
        if (Main.currentState == Main.PlayerState.Waiting)
            Debug.Log("fdp");
        else if (Main.currentState == Main.PlayerState.Aiming && !you)
        {
            ClientManager.Shoot(id, cell.position.x, cell.position.y);
        }
        else if (Main.currentState == Main.PlayerState.PlacingChips)
        {
            if (Main.currentId != -1)
            {
                if (PlaceChip(cell.position))
                {
                    Main.currentInstanciatedChip = null;
                    Main.currentId = -1;
                }
            }
            else
            {
                if (cell.ship != null)
                {
                    Main.currentInstanciatedChip = cell.ship;
                    _iRemove = cell.position.x;
                    _jRemove = cell.position.y;
                    _displayShipMenu = true;
                }
            }
        }
        Debug.Log(cell.position.ToString() + cell.type.ToString());
    }

    private bool PlaceChip(Vector2Int cellPosition)
    {
        Vector3 vect = Main.currentInstanciatedChip.transform.GetChild(0).forward;
        Vector2Int dir = new Vector2Int((int)vect.x, -(int)vect.z);
        int i = cellPosition.x, j = cellPosition.y;
        int length = Main.chipsLengths[Main.currentId];
        for (int k = 0; k < length; k++)
        {
            if (i < 0 || i >= _grid.GetLength(0) || j < 0 || j >= _grid.GetLength(1))
                return false;
            if (_grid[i, j].ship != null)
                return false;
            i += dir.x;
            j += dir.y;
        }

        i = cellPosition.x;
        j = cellPosition.y;
        int direction = 0;
        if (dir == Vector2Int.right)
            direction = 0;
        else if (dir == Vector2Int.up)
            direction = 3;
        else if (dir == Vector2Int.left)
            direction = 2;
        else if (dir == Vector2Int.down)
            direction = 1;
        Main.currentInstanciatedChip.GetComponentInChildren<Chip>().direction = dir;
        ClientManager.AddShip(Main.currentId, i, j, direction, length);
        --Main.nShipsToPlace;

        for (int k = 0; k < Main.chipsLengths[Main.currentId]; k++)
        {
            _grid[i, j].ship = Main.currentInstanciatedChip;
            i += dir.x;
            j += dir.y;
        }
        return true;
    }

    private void RemoveShip()
    {
        Chip chip = _grid[_iRemove, _jRemove].ship.GetComponentInChildren<Chip>();
        Vector2Int shipDir = chip.direction;
        Vector2Int browseDir = new Vector2Int(shipDir.x, -shipDir.y);
        while ((_iRemove < 0 || _iRemove >= _grid.GetLength(0) || _jRemove < 0 || _jRemove >= _grid.GetLength(1))
            && _grid[_iRemove + browseDir.x, _jRemove + browseDir.y].ship == _grid[_iRemove, _jRemove].ship)
        {
            _iRemove += browseDir.x;
            _jRemove += browseDir.y;
        }
    }

    public Vector3 GetWorldPosition(int i, int j)
    {
        return _gridStart + i * _cellSize * transform.right - j * _cellSize * transform.forward;
    }

    public void EmptyCellHit(int i, int j)
    {
        _grid[i, j].GetComponent<MeshRenderer>().material = Main.cellMaterials[PlayerCell.CellType.EmptyHit];
    }
    public void ShipCellHit(int i, int j)
    {
        _grid[i, j].GetComponent<MeshRenderer>().material = Main.cellMaterials[PlayerCell.CellType.ShipHit];
    }

    void OnGUI()
    {
        if (_displayShipMenu)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(Main.currentInstanciatedChip.transform.position);
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, 300, 400), GUI.skin.box);

            GUIStyle labelStyle = new GUIStyle("Label");
            labelStyle.fontSize = 32;
            GUILayout.Label("Remove this ship ?", labelStyle);

            GUIStyle buttonStyle = new GUIStyle("Button");
            buttonStyle.fontSize = 32;
            if (GUILayout.Button("Remove it", buttonStyle))
            {
                RemoveShip();
                ClientManager.RemoveShip(_iRemove, _jRemove);
                Main.nShipsToPlace++;
                Main.chipsButtons[Main.currentInstanciatedChip.GetComponentInChildren<Chip>().id].interactable = true;
                Destroy(Main.currentInstanciatedChip);
                Main.currentId = -1;
                Main.currentInstanciatedChip = null;
                _displayShipMenu = false;
            }

            if (GUILayout.Button("Cancel", buttonStyle))
            {
                Main.currentId = -1;
                Main.currentInstanciatedChip = null;
                _displayShipMenu = false;
            }
            GUILayout.EndArea();
        }
    }
}
