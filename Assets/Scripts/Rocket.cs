using DG.Tweening;
using UnityEngine;

public enum RocketOrientation
{
    Horizontal,
    Vertical
}

public class Rocket : SpecialItem
{
    [SerializeField] private float secondsPerCell = 0.05f;

    public RocketOrientation Orientation { get; private set; }

    public void SetOrientation(RocketOrientation orientation)
    {
        Orientation = orientation;

        Vector3 scale = transform.localScale;
        transform.localScale = orientation == RocketOrientation.Horizontal
            ? new Vector3(scale.x, scale.y * 0.5f, scale.z)
            : new Vector3(scale.x * 0.5f, scale.y, scale.z);
    }

    public override Sequence Explode(GridGenerator board, ExplosionService explosions)
    {
        GetComponent<SpriteRenderer>().enabled = false;

        Vector2Int direction = Orientation == RocketOrientation.Horizontal ? Vector2Int.right : Vector2Int.up;
        Sequence explosion = DOTween.Sequence();
        AddHalf(explosion, board, explosions, direction);
        AddHalf(explosion, board, explosions, -direction);
        return explosion;
    }

    private void AddHalf(Sequence explosion, GridGenerator board, ExplosionService explosions, Vector2Int direction)
    {
        Transform half = CreateHalf(board);

        int steps = 0;
        int x = X + direction.x;
        int y = Y + direction.y;

        while (board.IsInside(x, y))
        {
            steps++;
            int cellX = x;
            int cellY = y;
            explosion.InsertCallback(steps * secondsPerCell, () => explosions.DamageCell(cellX, cellY));
            x += direction.x;
            y += direction.y;
        }

        float travelDuration = (steps + 1) * secondsPerCell;
        explosion.Insert(0f, half.DOLocalMove(board.GetLocalPosition(x, y), travelDuration).SetEase(Ease.Linear));
        explosion.InsertCallback(travelDuration, () => Destroy(half.gameObject));
    }

    private Transform CreateHalf(GridGenerator board)
    {
        SpriteRenderer source = GetComponent<SpriteRenderer>();

        GameObject half = new GameObject("RocketHalf");
        half.transform.SetParent(board.transform, false);
        half.transform.localPosition = transform.localPosition;
        half.transform.localScale = transform.localScale;

        SpriteRenderer renderer = half.AddComponent<SpriteRenderer>();
        renderer.sprite = source.sprite;
        renderer.color = source.color;
        renderer.sortingOrder = source.sortingOrder + 1;

        return half.transform;
    }
}
