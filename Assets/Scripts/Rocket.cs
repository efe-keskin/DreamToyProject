using DG.Tweening;
using UnityEngine;

public enum RocketOrientation
{
    Horizontal,
    Vertical
}

public class Rocket : SpecialItem
{
    [SerializeField] private Sprite horizontalSprite;
    [SerializeField] private Sprite verticalSprite;
    [SerializeField] private Sprite leftHalfSprite;
    [SerializeField] private Sprite rightHalfSprite;
    [SerializeField] private Sprite topHalfSprite;
    [SerializeField] private Sprite bottomHalfSprite;

    public RocketOrientation Orientation { get; private set; }

    public void SetOrientation(RocketOrientation orientation)
    {
        Orientation = orientation;
        GetComponent<SpriteRenderer>().sprite = orientation == RocketOrientation.Horizontal
            ? horizontalSprite
            : verticalSprite;
    }

    public Sprite GetHalfSprite(Vector2Int direction)
    {
        if (direction.x != 0)
            return direction.x > 0 ? rightHalfSprite : leftHalfSprite;

        return direction.y > 0 ? topHalfSprite : bottomHalfSprite;
    }

    public override Sequence Explode(GridGenerator board, ExplosionService explosions, DamageTransaction transaction)
    {
        GetComponent<SpriteRenderer>().enabled = false;

        Vector2Int axis = Orientation == RocketOrientation.Horizontal ? Vector2Int.right : Vector2Int.up;
        Sequence explosion = DOTween.Sequence();
        RocketBeam.AddPair(explosion, board, explosions, transaction, X, Y, axis, this);
        return explosion;
    }
}
