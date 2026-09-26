using DG.Tweening;
using UnityEngine;

public class TntRocketCombo : Combo
{
    private readonly Rocket rocket;

    public TntRocketCombo(Rocket rocket)
    {
        this.rocket = rocket;
    }

    public override Sequence Play(GridGenerator board, ExplosionService explosions, DamageTransaction transaction, int x, int y)
    {
        Sequence explosion = DOTween.Sequence();

        for (int offset = -1; offset <= 1; offset++)
        {
            RocketBeam.AddPair(explosion, board, explosions, transaction, x, y + offset, Vector2Int.right, rocket);
            RocketBeam.AddPair(explosion, board, explosions, transaction, x + offset, y, Vector2Int.up, rocket);
        }

        return explosion;
    }
}
