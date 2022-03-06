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
            if(Main.currentId != -1)
            {
                cell.ship = Main.currentInstanciatedChip;
                //remplir les cell nécessaires pour stocker le bati bato
                Main.currentInstanciatedChip = null;
                Main.currentId = -1;
            }
            else
            {
                if(cell.ship != null)
                {
                    Main.currentInstanciatedChip = cell.ship;
                    Main.currentId = cell.ship.GetComponent<Chip>().id;
                    _displayShipMenu = true;
                }
            }
        }
        //TODO: Avertir Serveur
        Debug.Log(cell.position.ToString() + cell.type.ToString());
    }

    void OnGUI()
    {
        if (_displayShipMenu)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(Main.currentInstanciatedChip.transform.position);
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, 160, 200), GUI.skin.box);

            GUILayout.Label("Remove this ship ?");

            if (GUILayout.Button("Remove it"))
            {
                Destroy(Main.currentInstanciatedChip);
                Main.chipsButtons[Main.currentId].interactable = true;
                Main.currentId = -1;
                _displayShipMenu = false;
            }

            if (GUILayout.Button("Cancel"))
            {
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
