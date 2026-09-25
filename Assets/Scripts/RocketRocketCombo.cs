using DG.Tweening;
using UnityEngine;

public class RocketRocketCombo : Combo
{
    public RocketRocketCombo(SpriteRenderer visualSource) : base(visualSource)
    {
    }

    public override Sequence Play(GridGenerator board, ExplosionService explosions, int x, int y)
    {
        Sequence explosion = DOTween.Sequence();
        RocketBeam.AddPair(explosion, board, explosions, x, y, Vector2Int.right, sprite, color);
        RocketBeam.AddPair(explosion, board, explosions, x, y, Vector2Int.up, sprite, color);
        return explosion;
    }
}
