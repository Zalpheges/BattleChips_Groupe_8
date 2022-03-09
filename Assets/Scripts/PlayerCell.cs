using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
        if(ClientManager.MyTurn && parentPlayer.you || type != CellType.None)
           return;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Material mat = GetComponent<MeshRenderer>().material;
            EnableHighlight(mat);
            if(GameManager.CurrentState == GameManager.PlayerState.PlacingShips && GameManager.CurrentShipId != -1)
            {
                GameObject newChip = GameManager.ChipsButtons[GameManager.CurrentShipId].transform.GetChild(0).gameObject;
                GameManager.CurrentInstanciatedChip = Instantiate(newChip, transform.position, transform.rotation);
                GameManager.CurrentInstanciatedChip.transform.SetParent(transform.parent);
                GameManager.CurrentInstanciatedChip.transform.Rotate(transform.up * 1 * GameManager.LastRotation);
            }
        }
    }
    public void MouseExit()
    {
        Material mat = GetComponent<MeshRenderer>().material;
        DisableHighlight(mat);
        if (GameManager.PlacingShips && GameManager.CurrentShipId != -1)
            Destroy(GameManager.CurrentInstanciatedChip);
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
