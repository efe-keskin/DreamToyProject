using DG.Tweening;
using UnityEngine;

public class Tnt : SpecialItem
{
    private const int Radius = 2;
    [SerializeField] private TntVfxSettings vfxSettings = new TntVfxSettings();

    public TntVfxSettings VfxSettings => vfxSettings;

    public override Sequence Explode(GridGenerator board, ExplosionService explosions, DamageTransaction transaction)
    {
        Vector3 scale = transform.localScale;
        SpriteRenderer body = GetComponent<SpriteRenderer>();
        Sequence explosion = DOTween.Sequence();

        explosion.Append(transform.DOScale(scale * 1.4f, 0.12f).SetEase(Ease.OutQuad));
        explosion.AppendCallback(() =>
        {
            body.enabled = false;
            TntExplosionEffect.Play(board, transform.localPosition, vfxSettings);
            explosions.DamageArea(X, Y, Radius, transaction);
        });
        explosion.AppendInterval(0.2f);

        return explosion;
    }
}
