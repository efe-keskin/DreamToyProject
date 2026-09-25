using DG.Tweening;
using UnityEngine;

public static class RocketBeam
{
    private const float SecondsPerCell = 0.05f;

    public static void AddPair(Sequence sequence, GridGenerator board, ExplosionService explosions, DamageTransaction transaction,
        int originX, int originY, Vector2Int axis, Sprite sprite, Color color)
    {
        if (!board.IsInside(originX, originY))
            return;

        sequence.InsertCallback(0f, () => explosions.DamageCell(originX, originY, transaction));
        AddHalf(sequence, board, explosions, transaction, originX, originY, axis, sprite, color);
        AddHalf(sequence, board, explosions, transaction, originX, originY, -axis, sprite, color);
    }

    private static void AddHalf(Sequence sequence, GridGenerator board, ExplosionService explosions, DamageTransaction transaction,
        int originX, int originY, Vector2Int direction, Sprite sprite, Color color)
    {
        Vector3 scale = direction.x != 0 ? new Vector3(1f, 0.5f, 1f) : new Vector3(0.5f, 1f, 1f);
        Transform half = EffectSprite.Create(board, sprite, color, board.GetLocalPosition(originX, originY), scale).transform;

        int steps = 0;
        int x = originX + direction.x;
        int y = originY + direction.y;

        while (board.IsInside(x, y))
        {
            steps++;
            int cellX = x;
            int cellY = y;
            sequence.InsertCallback(steps * SecondsPerCell, () => explosions.DamageCell(cellX, cellY, transaction));
            x += direction.x;
            y += direction.y;
        }

        float travelDuration = (steps + 1) * SecondsPerCell;
        sequence.Insert(0f, half.DOLocalMove(board.GetLocalPosition(x, y), travelDuration).SetEase(Ease.Linear));
        sequence.InsertCallback(travelDuration, () => Object.Destroy(half.gameObject));
    }
}
