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
                cell.onClick += GameManager.OnCellClicked;
                _grid[x, y] = cell;
            }
        }
    }
    

    public bool IsSpaceFree(int i, int j, int length, Vector2Int dir)
    {
        for (int k = 0; k < length; k++)
        {
            if (i < 0 || i >= _grid.GetLength(0) || j < 0 || j >= _grid.GetLength(1))
                return false;
            if (_grid[i, j].ship != null)
                return false;
            i += dir.x;
            j += dir.y;
        }
        return true;
    }

    public void AddShipToGrid(GameObject ship, int i, int j, int length, Vector2Int dir)
    {
        for (int k = 0; k < length; k++)
        {
            _grid[i, j].ship = ship;
            i += dir.x;
            j += dir.y;
        }
    }


    public void RemoveShip(int iStart, int jStart)
    {
        Ship ship = _grid[iStart, jStart].ship.GetComponentInChildren<Ship>();
        Vector2Int shipDir = ship.direction;
        Vector2Int browseDir = new Vector2Int(shipDir.x, -shipDir.y);
        for (int k = 0; k < 2; ++k)
        {
            int i = iStart;
            int j = jStart;
            _grid[i, j] = null;

            while (i > 0 && i < _grid.GetLength(0) && j > 0 && j < _grid.GetLength(1)
                && _grid[i + browseDir.x, j + browseDir.y].ship == _grid[i, j].ship)
            {
                i += browseDir.x;
                j += browseDir.y;
                _grid[i, j] = null;
            }
            browseDir = - browseDir;
        }
    }

    public Vector3 GetWorldPosition(int i, int j)
    {
        return _gridStart + i * _cellSize * transform.right - j * _cellSize * transform.forward;
    }

    public void SetCellType(int i, int j, PlayerCell.CellType cellType)
    {
        _grid[i, j].SetType(cellType);
    }

    public GameObject GetShip(int i, int j)
    {
        return _grid[i, j].ship;
    }
}
