using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public enum State
    {
        PlacingShips,
        Playing
    }

    [Header("Board")]

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
    private Dictionary<Cell.Type, Material> _cellMaterials;

    public Button SubmitButton;
    private static GameManager _instance;
    public static State CurrentState;
    public static int CurrentShipId = -1;
    public static GameObject CurrentInstanciatedChip;
    public static int LastRotation;
    public static int NShipsToPlace = 1;
    private bool _boarded = false;
    public static bool PlacingShips => CurrentState == State.PlacingShips;
    public static bool IsShipSelected => CurrentShipId != -1;

    private bool _displayShipMenu = false;

    private int _iRemove;
    private int _jRemove;

    [Space(5)]
    [Header("Game")]

    private int _turn = -1;
    private Player[] _players;

    private static Player Me => _instance._players[MyID];
    private static Player CurrentPlayer => _instance._players[_instance._turn];

    public static int MyID { get; private set; }
    public static bool MyTurn => _instance._turn == MyID;

    [Space(5)]
    [Header("Missile")]

    [SerializeField]
    private Missile _missilePrefab;

    [SerializeField] //-112 -45
    private Vector3 _localSpawnPosition;

    private void Awake()
    {
        _instance = this;

        foreach (ShipData shipData in _shipsDatas)
            _shipDatas.Add(shipData.id, shipData);

        foreach (CellMaterial cellMat in _cellsMaterials)
            _cellMaterials.Add(cellMat.cellType, cellMat.material);
    }

    private void Start()
    {
        _shipDatas = new Dictionary<int, ShipData>();
        _cellMaterials = new Dictionary<Cell.Type, Material>();
    }

    private void Update()
    {
        if (PlacingShips)
        {
            SubmitButton.interactable = _boarded ? false : NShipsToPlace == 0;
        }
    }
    public void Boarded()
    {
        _boarded = true;

        ClientManager.Boarded();
    }

    void OnGUI()
    {
        if (_displayShipMenu && !_boarded)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(CurrentInstanciatedChip.transform.position);
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, 300, 400), GUI.skin.box);

            GUIStyle labelStyle = new GUIStyle("Label") { fontSize = 32 };
            GUIStyle buttonStyle = new GUIStyle("Button") { fontSize = 32 };

            GUILayout.Label("Remove this ship ?", labelStyle);

            if (GUILayout.Button("Remove it", buttonStyle))
            {
                _players[MyID].RemoveShip(_iRemove, _jRemove);
                ClientManager.RemoveShip(_iRemove, _jRemove);

                ++NShipsToPlace;
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

    public static void OnCellClicked(Cell cell)
    {
        Player player = cell.transform.parent.GetComponent<Player>();
        if (_instance._displayShipMenu)
            return;
        if (MyTurn && !player.You)
        {
            ClientManager.Shoot(player.Id, cell.position.x, cell.position.y);
        }
        else if (PlacingShips)
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
                if (cell.ship != null)
                {
                    CurrentInstanciatedChip = cell.ship;
                    _instance._iRemove = cell.position.x;
                    _instance._jRemove = cell.position.y;
                    _instance._displayShipMenu = true;
                }
            }
        }
        Debug.Log(cell.position.ToString() + cell._type.ToString());
    }

    public static void Board(int id, int count)
    {
        _instance._players = Map.CreatePlayers(id, count);

        UIManager.ShowMenu(UIManager.Menu.Board);
    }

    public static void KillPlayer(int id)
    {
        _instance._players[id].dead = true;
    }

    public static void Play()
    {
        _instance._turn = 0;

        CurrentState = State.Playing;

        UIManager.SetTurn(ClientManager.GetName(_instance._turn));
    }

    public static void Shoot(int id, int x, int y, Action onTargetReach = null)
    {
        Player target = _instance._players[id];
        Transform player = CurrentPlayer.transform;

        Vector3 from = player.position + _instance._localSpawnPosition;
        Vector3 to = target.GetWorldPosition(x, y);

        ClientManager.Wait = true;

        Vector3 worldSpawnPosition = player.forward * _instance._localSpawnPosition.z + player.right * _instance._localSpawnPosition.x;

        Missile missile = Instantiate(_instance._missilePrefab);
        missile.SetCallbacks(
            onTargetReach,
            delegate () {
                UIManager.SetTurn(ClientManager.GetName(CurrentPlayer.Id));
            }
        );

        missile.Shoot(from, to, onTargetReach != null);

        UIManager.ShowShoot(ClientManager.GetName(CurrentPlayer.Id), ClientManager.GetName(target.Id));

        target.SetCellType(x, y, Cell.Type.ShipHit);

        do
        {
            _instance._turn = (_instance._turn + 1) % _instance._players.Length;
        } while (CurrentPlayer.dead);
    }

    public static void Shoot(int id, int x, int y, int shipId, int shipX, int shipY, int shipDir)
    {
        Player target = _instance._players[id];

        Shoot(id, x, y, delegate () {
            Transform ship;
            if (!target.You)
            {
                ship = Instantiate(_instance._shipDatas[shipId].prefab, target.GetWorldPosition(shipX, shipY),
                    Quaternion.identity, target.transform).transform;
            }
            else
                ship = target.GetShip(x, y).transform;
            ship.localRotation *= Quaternion.Euler(Vector3.right * 1000f);
        });
       
    }

    public static void RotateChip()
    {
        if (CurrentShipId != -1)
        {
            LastRotation += 90;
            LastRotation %= 360;
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

    public static void PrevisualizeShipOnCell(Transform cellTransform)
    {
        GameObject newChip = _instance._shipDatas[CurrentShipId].prefab;
        CurrentInstanciatedChip = Instantiate(newChip, cellTransform.position, cellTransform.rotation);
        CurrentInstanciatedChip.transform.SetParent(cellTransform.parent);
        CurrentInstanciatedChip.transform.Rotate(cellTransform.up * LastRotation);
    }

    public static bool PlaceChip(Vector2Int cellPosition, Player player)
    {
        int dir = LastRotation / 90;
        Vector2Int vect = Ship.DirToVector(dir);

        int i = cellPosition.x, j = cellPosition.y;
        int length = _instance._shipDatas[CurrentShipId].length;

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
