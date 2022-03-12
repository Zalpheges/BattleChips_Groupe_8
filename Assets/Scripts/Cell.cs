using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class Cell : MonoBehaviour
{
    public enum Type
    {
        None,
        EmptyHit,
        ShipHit
    }

    private Type _type;
    private Action<Cell> _onCellClick;

    public Vector2Int Position { get; private set; }

    private Player _owner;

    public void Init(Vector2Int position, Action<Cell> onClick)
    {
        _type = Type.None;

        Position = position;
        _onCellClick = onClick;

        _owner = GetComponentInParent<Player>();
    }

    public void PointerClick()
    {
        if(_type == Type.None && !EventSystem.current.IsPointerOverGameObject())
            _onCellClick?.Invoke(this);
    }

    public void MouseEnter()
    {
        if(_type != Type.None && GameManager.MyTurn)
           return;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            EnableHighlight(GetComponent<MeshRenderer>().material);

            if (GameManager.PlacingShips && GameManager.IsShipSelected)
                GameManager.PrevisualizeShipOnCell(transform);
        }
    }

    public void MouseExit()
    {
        DisableHighlight(GetComponent<MeshRenderer>().material);

        if (GameManager.PlacingShips && GameManager.CurrentShipId != -1)
            Destroy(GameManager.CurrentInstanciatedChip);
    }

    public void SetType(Type newType)
    {
        _type = newType;

        GetComponent<MeshRenderer>().material = GameManager._cellMaterials[newType];
    }

    private void EnableHighlight(Material mat)
    {
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.grey);
    }

    private void DisableHighlight(Material mat)
    {
        mat.DisableKeyword("_EMISSION");
    }
}
