using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Vase : Obstacle
{
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private Sprite[] fragmentSprites;
    [SerializeField] private Sprite dustSprite;

    public override GoalType GoalType => GoalType.Vase;

    protected override int MaxHealth => 2;

    protected override void OnDamaged()
    {
        if (IsCleared)
            return;

        GetComponent<SpriteRenderer>().sprite = damagedSprite;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetLink(gameObject);
        VaseBreakEffect.Play(Board, transform.localPosition, fragmentSprites, dustSprite, false);
    }

    public override void PlayBreakEffect()
    {
        VaseBreakEffect.Play(Board, transform.localPosition, fragmentSprites, dustSprite, true);
    }
}
