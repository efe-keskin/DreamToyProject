using DG.Tweening;
using UnityEngine;

public class TntRocketCombo : Combo
{
    public TntRocketCombo(SpriteRenderer visualSource) : base(visualSource)
    {
    }

    public override Sequence Play(GridGenerator board, ExplosionService explosions, int x, int y)
    {
        Sequence explosion = DOTween.Sequence();

        for (int offset = -1; offset <= 1; offset++)
        {
            RocketBeam.AddPair(explosion, board, explosions, x, y + offset, Vector2Int.right, sprite, color);
            RocketBeam.AddPair(explosion, board, explosions, x + offset, y, Vector2Int.up, sprite, color);
        }

        return explosion;
    }
}
