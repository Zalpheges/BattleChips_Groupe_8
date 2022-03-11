using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class PlayerCell : MonoBehaviour
{
    public enum CellType
    {
        None,
        EmptyHit,
        ShipHit
    }
    public Action<PlayerCell> onClick;

    public Vector2Int position;
    public CellType type;
    public GameObject ship;

    public void PointerClick()
    {
        if(!EventSystem.current.IsPointerOverGameObject() && type == CellType.None)
            onClick?.Invoke(this);
    }

    public void MouseEnter()
    {
        Player parentPlayer = transform.parent.GetComponent<Player>();
        if(GameManager.MyTurn && parentPlayer.you || type != CellType.None)
           return;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Material mat = GetComponent<MeshRenderer>().material;
            EnableHighlight(mat);

            if (GameManager.PlacingShips && GameManager.IsShipSelected)
                GameManager.PrevisualizeShipOnCell(transform);
        }
    }

    public void MouseExit()
    {
        Material mat = GetComponent<MeshRenderer>().material;
        DisableHighlight(mat);
        if (GameManager.PlacingShips && GameManager.CurrentShipId != -1)
            Destroy(GameManager.CurrentInstanciatedChip);
    }

    public void SetType(CellType newType)
    {
        type = newType;
        GetComponent<MeshRenderer>().material = GameManager.CellMaterials[newType];
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
