using DG.Tweening;
using UnityEngine;

public class TntTntCombo : Combo
{
    private const int Radius = 3;

    private readonly Sprite sprite;
    private readonly Color color;
    private readonly TntVfxSettings vfxSettings;

    public TntTntCombo(SpriteRenderer visualSource)
    {
        sprite = visualSource.sprite;
        color = visualSource.color;
        vfxSettings = visualSource.GetComponent<Tnt>().VfxSettings;
    }

    public override Sequence Play(GridGenerator board, ExplosionService explosions, DamageTransaction transaction, int x, int y)
    {
        SpriteRenderer blast = EffectSprite.Create(board, sprite, color, board.GetLocalPosition(x, y), Vector3.one);
        Sequence explosion = DOTween.Sequence();

        explosion.Append(blast.transform.DOScale(2f, 0.15f).SetEase(Ease.OutQuad));
        explosion.AppendCallback(() =>
        {
            blast.enabled = false;
            TntExplosionEffect.Play(board, blast.transform.localPosition, vfxSettings, 1.4f);
            explosions.DamageArea(x, y, Radius, transaction);
        });
        explosion.AppendInterval(0.25f);
        explosion.AppendCallback(() => Object.Destroy(blast.gameObject));

        return explosion;
    }
}
