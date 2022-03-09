using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject prefabCell;
    public string nickName;
    public int id;
    public bool you;
    public bool dead = false;
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
        if (_displayShipMenu || dead)
            return;
        if (GameManager.CurrentState == GameManager.PlayerState.Aiming && !you)
        {
            ClientManager.Shoot(id, cell.position.x, cell.position.y);
        }
        else if (GameManager.PlacingShips)
        {
            if (GameManager.CurrentShipId != -1)
            {
                if (PlaceChip(cell.position))
                {
                    GameManager.CurrentInstanciatedChip = null;
                    GameManager.CurrentShipId = -1;
                }
            }
            else
            {
                if (cell.ship != null)
                {
                    GameManager.CurrentInstanciatedChip = cell.ship;
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
        int dir = (int)GameManager.LastRotation / 90;//sens horaire
        Vector2Int vect = Vector2Int.zero;
        if (dir == 0)
            vect = Vector2Int.right;
        else if (dir == 1)
            vect = Vector2Int.up;
        else if (dir == 2)
            vect = Vector2Int.left;
        else if (dir == 3)
            vect = Vector2Int.down;
        int i = cellPosition.x, j = cellPosition.y;
        int length = GameManager.ChipsLengths[GameManager.CurrentShipId];
        for (int k = 0; k < length; k++)
        {
            if (i < 0 || i >= _grid.GetLength(0) || j < 0 || j >= _grid.GetLength(1))
                return false;
            if (_grid[i, j].ship != null)
                return false;
            i += vect.x;
            j += vect.y;
        }

        i = cellPosition.x;
        j = cellPosition.y;
        GameManager.CurrentInstanciatedChip.GetComponentInChildren<Ship>().direction = vect;
        int trigDir = dir % 2 == 1 ? dir + 2 % 4 : dir;
        ClientManager.AddShip(GameManager.CurrentShipId, i, j, trigDir, length);
        --GameManager.NShipsToPlace;

        for (int k = 0; k < GameManager.ChipsLengths[GameManager.CurrentShipId]; k++)
        {
            _grid[i, j].ship = GameManager.CurrentInstanciatedChip;
            i += vect.x;
            j += vect.y;
        }
        return true;
    }

    private void RemoveShip()
    {
        Ship chip = _grid[_iRemove, _jRemove].ship.GetComponentInChildren<Ship>();
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
        _grid[i, j].type = PlayerCell.CellType.EmptyHit;
        _grid[i, j].GetComponent<MeshRenderer>().material = GameManager.CellMaterials[PlayerCell.CellType.EmptyHit];
    }
    public void ShipCellHit(int i, int j)
    {
        _grid[i, j].type = PlayerCell.CellType.ShipHit;
        _grid[i, j].GetComponent<MeshRenderer>().material = GameManager.CellMaterials[PlayerCell.CellType.ShipHit];
    }

    public GameObject GetShip(int i, int j)
    {
        return _grid[i, j].ship;
    }

    void OnGUI()
    {
        if (_displayShipMenu && !GameManager.Boarded)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(GameManager.CurrentInstanciatedChip.transform.position);
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
                GameManager.NShipsToPlace++;
                GameManager.ChipsButtons[GameManager.CurrentInstanciatedChip.GetComponentInChildren<Ship>().id].interactable = true;
                Destroy(GameManager.CurrentInstanciatedChip);
                GameManager.CurrentShipId = -1;
                GameManager.CurrentInstanciatedChip = null;
                _displayShipMenu = false;
            }

            if (GUILayout.Button("Cancel", buttonStyle))
            {
                GameManager.CurrentShipId = -1;
                GameManager.CurrentInstanciatedChip = null;
                _displayShipMenu = false;
            }
            GUILayout.EndArea();
        }
    }
}
