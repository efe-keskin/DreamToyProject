using UnityEngine;

public static class LevelTokens
{
    public const string Red = "r";
    public const string Green = "g";
    public const string Blue = "b";
    public const string Yellow = "y";
    public const string RandomCube = "rand";
    public const string VerticalRocket = "vro";
    public const string HorizontalRocket = "hro";
    public const string Tnt = "t";
    public const string Stone = "s";
    public const string Vase = "v";
    public const string ChaliceBoxTopLeft = "cbTL";
    public const string ChaliceBoxTopRight = "cbTR";
    public const string ChaliceBoxBottomLeft = "cbBL";
    public const string ChaliceBoxBottomRight = "cbBR";

    public static bool IsKnown(string token)
    {
        switch (token)
        {
            case Red:
            case Green:
            case Blue:
            case Yellow:
            case RandomCube:
            case VerticalRocket:
            case HorizontalRocket:
            case Tnt:
            case Stone:
            case Vase:
            case ChaliceBoxTopLeft:
            case ChaliceBoxTopRight:
            case ChaliceBoxBottomLeft:
            case ChaliceBoxBottomRight:
                return true;
            default:
                return false;
        }
    }

    public static bool TryGetChaliceBoxOffset(string token, out Vector2Int offset)
    {
        switch (token)
        {
            case ChaliceBoxBottomLeft: offset = new Vector2Int(0, 0); return true;
            case ChaliceBoxBottomRight: offset = new Vector2Int(1, 0); return true;
            case ChaliceBoxTopLeft: offset = new Vector2Int(0, 1); return true;
            case ChaliceBoxTopRight: offset = new Vector2Int(1, 1); return true;
            default: offset = Vector2Int.zero; return false;
        }
    }
}
