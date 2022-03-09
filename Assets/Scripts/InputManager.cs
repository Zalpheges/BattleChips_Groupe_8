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
        if (!ClientManager.MyTurn && !GameManager.PlacingShips)
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

        if (Input.GetMouseButtonDown(1) && GameManager.CurrentState == GameManager.PlayerState.PlacingShips)
                GameManager.RotateChip();
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
                GameManager.ShipDatas[GameManager.CurrentInstanciatedChip.GetComponentInChildren<Ship>().id].button.interactable = true;
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
