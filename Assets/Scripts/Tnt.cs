using DG.Tweening;
using UnityEngine;

public class Tnt : SpecialItem
{
    private const int Radius = 2;

    public override Sequence Explode(GridGenerator board, ExplosionService explosions)
    {
        Vector3 scale = transform.localScale;
        Sequence explosion = DOTween.Sequence();

        explosion.Append(transform.DOScale(scale * 1.4f, 0.12f).SetEase(Ease.OutQuad));
        explosion.AppendCallback(() =>
        {
            for (int dx = -Radius; dx <= Radius; dx++)
            {
                for (int dy = -Radius; dy <= Radius; dy++)
                    explosions.DamageCell(X + dx, Y + dy);
            }
        });
        explosion.Append(transform.DOScale(scale * 3f, 0.2f).SetEase(Ease.OutQuad));
        explosion.Join(GetComponent<SpriteRenderer>().DOFade(0f, 0.2f));

        return explosion;
    }
}
