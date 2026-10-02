using UnityEngine;

public static class EffectSprite
{
    private const int DefaultSortingOrder = 10;

    public static SpriteRenderer Create(GridGenerator board, Sprite sprite, Color color, Vector3 localPosition, Vector3 scale, int sortingOrder = DefaultSortingOrder)
    {
        GameObject effect = new GameObject("Effect");
        effect.transform.SetParent(board.transform, false);
        effect.transform.localPosition = localPosition;
        effect.transform.localScale = scale;

        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }
}
