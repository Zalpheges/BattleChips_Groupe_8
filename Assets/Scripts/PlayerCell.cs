using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCell : MonoBehaviour
{
    public enum CellType
    {
        None,
        Attacked
    }
    public Action<PlayerCell> onClick;

    public Vector2Int position;
    public CellType type;
    public GameObject ship;

    public void PointerClick()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
            onClick?.Invoke(this);
    }

    public void MouseEnter()
    {
        Debug.Log("fdpEnter");
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Material mat = GetComponent<MeshRenderer>().material;
            EnableHighlight(mat);
            if(Main.currentState == Main.PlayerState.PlacingChips && Main.currentId != -1)
            {
                GameObject newChip = Main.chipsButtons[Main.currentId].transform.GetChild(0).gameObject;
                Main.currentInstanciatedChip = Instantiate(newChip, transform.position, Quaternion.identity);
                Main.currentInstanciatedChip.transform.SetParent(transform);
                Main.currentInstanciatedChip.transform.Rotate(transform.up * Main.lastRotation);
            }
        }
    }
    public void MouseExit()
    {
        Debug.Log("fdpExit");
        Material mat = GetComponent<MeshRenderer>().material;
        DisableHighlight(mat); 
        if (Main.currentState == Main.PlayerState.PlacingChips && Main.currentInstanciatedChip != null && Main.currentId != -1)
        {
            Destroy(Main.currentInstanciatedChip);
        }
    }

    private void EnableHighlight(Material mat)
    {
        mat?.EnableKeyword("_EMISSION");
        mat?.SetColor("_EmissionColor", Color.grey);
    }
    private void DisableHighlight(Material mat)
    {
        mat?.DisableKeyword("_EMISSION");
    }
}
