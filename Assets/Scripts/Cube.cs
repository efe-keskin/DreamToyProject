using UnityEngine;

public enum CubeType
{
    Red,
    Green,
    Blue,
    Yellow
}

[RequireComponent(typeof(SpriteRenderer))]
public class Cube : GridItem
{
    public CubeType Type { get; private set; }

    public void Initialize(CubeType type, int x, int y, GridGenerator board)
    {
        Type = type;
        AttachToBoard(board, x, y);
        GetComponent<SpriteRenderer>().color = GetColor(type);
    }

    private static Color GetColor(CubeType type)
    {
        switch (type)
        {
            case CubeType.Red: return Color.red;
            case CubeType.Green: return Color.green;
            case CubeType.Blue: return Color.blue;
            case CubeType.Yellow: return Color.yellow;
            default: return Color.white;
        }
    }
}
