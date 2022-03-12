using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager _instance;

    [SerializeField]
    private LayerMask _cellLayer;

    private Cell _currentCell;

    private void Update()
    {
        if (!GameManager.MyTurn && !GameManager.PlacingShips)
        {
            _currentCell?.MouseExit();
            _currentCell = null;

            return;
        }

        Cell tmp;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _cellLayer))
        {
            tmp = hitInfo.transform.GetComponent<Cell>();

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
                GameManager.RotateChip();
    }
}
