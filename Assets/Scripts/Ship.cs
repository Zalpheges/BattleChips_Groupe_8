using UnityEngine;

public class Ship : MonoBehaviour
{
    public int id;
    public Vector2Int direction;
    public bool visible;

    public static Vector2Int DirToVector(int dir)
    {
        if (dir == 0)
            return Vector2Int.right;
        else if (dir == 1)
            return Vector2Int.up;
        else if (dir == 2)
            return Vector2Int.left;
        else if (dir == 3)
            return Vector2Int.down;
        else
            return Vector2Int.zero;
    }
}
