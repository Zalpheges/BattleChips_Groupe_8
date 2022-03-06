using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour
{
    public int id;
    public enum Direction
    {
        None = -1,
        Right = 0,
        Up = 1,
        Left = 2,
        Down = 3
    }
    public Direction direction = Direction.None;
}
