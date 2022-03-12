using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] private LayerMask _cellsLayer;

    private PlayerCell _currentCell;

    private void Update()
    {
        if (!GameManager.MyTurn && !GameManager.PlacingShips)
        {
            _currentCell?.MouseExit();
            _currentCell = null;
            return;
        }

        PlayerCell tmp;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _cellsLayer))
        {
            tmp = hitInfo.transform.GetComponent<PlayerCell>();
            if (Input.GetMouseButtonDown(0))
                tmp.PointerClick();
            if (_currentCell != tmp)
            {
                _currentCell?.MouseExit();
                _currentCell = tmp;
                _currentCell.MouseEnter();
            }
        }
        else
            _currentCell?.MouseExit();

        if (Input.GetMouseButtonDown(1) && GameManager.CurrentState == GameManager.State.PlacingShips)
                GameManager.shipPlacement.RotateChip();
    }
}
