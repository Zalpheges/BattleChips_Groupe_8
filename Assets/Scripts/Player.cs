using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private int width = 10, height = 10;
    [SerializeField] private GameObject _prefabCell;

    private float _cellSize;
    private Vector3 _gridStart;
    private PlayerCell[,] _grid;

    private bool you;
    private bool _displayShipMenu = false;

    private void Start()
    {
        //TODO: setup you via serveur
    }

    public void Initialize()
    {
        _cellSize = _prefabCell.transform.localScale.x;
        _grid = new PlayerCell[height, width];
        _gridStart = transform.position +
                    transform.forward * (width - 1) / 2f * _cellSize -
                    transform.right * (height - 1) / 2f * _cellSize
                    +Vector3.up * 10;


        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                GameObject cellGo = Instantiate(_prefabCell, transform);
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
        else if (Main.currentState == Main.PlayerState.Aiming)
            Shoot();
        else if (Main.currentState == Main.PlayerState.PlacingChips)
        {
            if (Main.currentId != -1)
            {
                bool a = PlaceChip(cell.position);
                Debug.Log(a);
                if (a)
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
                    _displayShipMenu = true;
                }
            }
        }
        //TODO: Avertir Serveur
        Debug.Log(cell.position.ToString() + cell.type.ToString());
    }

    private bool PlaceChip(Vector2Int cellPosition)
    {
        Vector3 vect = Main.currentInstanciatedChip.transform.GetChild(0).forward;
        Debug.Log(vect);
        Vector2Int dir = new Vector2Int((int)vect.x, -(int)vect.z);
        int i = cellPosition.x, j = cellPosition.y;
        for(int k = 0; k < Main.chipsLengths[Main.currentId]; k++)
        {
            Debug.Log(i + " " + j);
            if (i < 0 || i >= _grid.GetLength(0) || j < 0 || j >= _grid.GetLength(1))
                return false;
            if (_grid[i, j].ship != null)
                return false;
            i += dir.x;
            j += dir.y;
        }

        i = cellPosition.x;
        j = cellPosition.y;
        for (int k = 0; k < Main.chipsLengths[Main.currentId]; k++)
        {
            _grid[i, j].ship = Main.currentInstanciatedChip;
            i += dir.x;
            j += dir.y;
        }
        return true;
    }
    private void FillGridWithChip(Vector2Int cellPosition)
    {
        Vector3 vect = Main.currentInstanciatedChip.transform.forward;
        Vector2Int dir = new Vector2Int((int)vect.x, -(int)vect.z);
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

    bool Shoot()
    {
        if (true) return false;
        else if (false) return true;
        else return false;
    }
}
