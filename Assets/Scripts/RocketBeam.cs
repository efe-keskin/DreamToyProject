using DG.Tweening;
using UnityEngine;

public static class RocketBeam
{
    private const float SecondsPerCell = 0.05f;

    public static void AddPair(Sequence sequence, GridGenerator board, ExplosionService explosions, DamageTransaction transaction,
        int originX, int originY, Vector2Int axis, Rocket source)
    {
        if (!board.IsInside(originX, originY))
            return;

        sequence.InsertCallback(0f, () => explosions.DamageCell(originX, originY, transaction));
        AddHalf(sequence, board, explosions, transaction, originX, originY, axis, source);
        AddHalf(sequence, board, explosions, transaction, originX, originY, -axis, source);
    }

    private static void AddHalf(Sequence sequence, GridGenerator board, ExplosionService explosions, DamageTransaction transaction,
        int originX, int originY, Vector2Int direction, Rocket source)
    {
        Sprite sprite = source.GetHalfSprite(direction);
        Transform half = EffectSprite.Create(board, sprite, Color.white, board.GetLocalPosition(originX, originY), Vector3.one).transform;
        half.gameObject.AddComponent<RocketTrail>().Initialize(board, direction, source);

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
