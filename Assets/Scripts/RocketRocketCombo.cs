using DG.Tweening;
using UnityEngine;

public class RocketRocketCombo : Combo
{
    private readonly Rocket rocket;

    public RocketRocketCombo(Rocket rocket)
    {
        this.rocket = rocket;
    }

    public override Sequence Play(GridGenerator board, ExplosionService explosions, DamageTransaction transaction, int x, int y)
    {
        Sequence explosion = DOTween.Sequence();
        RocketBeam.AddPair(explosion, board, explosions, transaction, x, y, Vector2Int.right, rocket);
        RocketBeam.AddPair(explosion, board, explosions, transaction, x, y, Vector2Int.up, rocket);
        return explosion;
    }
}
