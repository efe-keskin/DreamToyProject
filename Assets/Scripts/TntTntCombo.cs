using DG.Tweening;
using UnityEngine;

public class TntTntCombo : Combo
{
    private const int Radius = 3;

    public TntTntCombo(SpriteRenderer visualSource) : base(visualSource)
    {
    }

    public override Sequence Play(GridGenerator board, ExplosionService explosions, int x, int y)
    {
        SpriteRenderer blast = EffectSprite.Create(board, sprite, color, board.GetLocalPosition(x, y), Vector3.one);
        Sequence explosion = DOTween.Sequence();

        explosion.Append(blast.transform.DOScale(2f, 0.15f).SetEase(Ease.OutQuad));
        explosion.AppendCallback(() => explosions.DamageArea(x, y, Radius));
        explosion.Append(blast.transform.DOScale(5f, 0.25f).SetEase(Ease.OutQuad));
        explosion.Join(blast.DOFade(0f, 0.25f));
        explosion.AppendCallback(() => Object.Destroy(blast.gameObject));

        return explosion;
    }
}
