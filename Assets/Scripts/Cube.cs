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

    private SpriteRenderer hintRenderer;

    public void Initialize(CubeType type, int x, int y, GridGenerator board)
    {
        Type = type;
        AttachToBoard(board, x, y);
        GetComponent<SpriteRenderer>().color = GetColor(type);
    }

    public void ShowHint(SpecialItemKind hint)
    {
        if (hint == SpecialItemKind.None)
        {
            if (hintRenderer != null)
                hintRenderer.enabled = false;
            return;
        }

        if (hintRenderer == null)
            hintRenderer = CreateHintRenderer();

        hintRenderer.enabled = true;
        hintRenderer.color = hint == SpecialItemKind.Rocket ? Color.white : Color.black;
        hintRenderer.transform.localScale = hint == SpecialItemKind.Rocket
            ? new Vector3(0.6f, 0.2f, 1f)
            : new Vector3(0.4f, 0.4f, 1f);
    }

    private SpriteRenderer CreateHintRenderer()
    {
        SpriteRenderer body = GetComponent<SpriteRenderer>();

        GameObject hint = new GameObject("Hint");
        hint.transform.SetParent(transform, false);

        SpriteRenderer renderer = hint.AddComponent<SpriteRenderer>();
        renderer.sprite = body.sprite;
        renderer.sortingOrder = body.sortingOrder + 1;
        return renderer;
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
