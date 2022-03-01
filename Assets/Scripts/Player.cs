using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int width = 10, height = 10;
    [SerializeField] private GameObject _prefabCell;

    private float _cellSize;
    private Vector3 _gridStart;
    private PlayerCell[,] _grid;

    private bool you;

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
        if (Main.currentState == Main.PlayerState.Waiting)
            Debug.Log("fdp");
        else if (Main.currentState == Main.PlayerState.Aiming)
            Shoot();
        else if (Main.currentState == Main.PlayerState.PlacingChips)
            return;
        //TODO: Avertir Serveur
        Debug.Log(cell.position.ToString() + cell.type.ToString());
    }

    bool Shoot()
    {
        if (true) return false;
        else if (false) return true;
        else return false;
    }
}
