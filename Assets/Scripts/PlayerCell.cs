using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCell : MonoBehaviour, IPointerClickHandler
{
    public enum CellType
    {
        None,
        Attacked
    }
    public Action<PlayerCell> onClick;

    public Vector2Int position;
    public CellType type;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!EventSystem.current.IsPointerOverGameObject())
            onClick?.Invoke(this);
    }
    private void OnMouseEnter()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Material mat = GetComponent<MeshRenderer>().material;
            EnableHighlight(mat);
        }
    }
    private void OnMouseExit()
    {
        Material mat = GetComponent<MeshRenderer>().material;
        DisableHighlight(mat);              
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
