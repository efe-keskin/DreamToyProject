using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Vase : Obstacle
{
    [SerializeField] private Sprite damagedSprite;

    public override GoalType GoalType => GoalType.Vase;

    protected override int MaxHealth => 2;

    protected override void OnDamaged()
    {
        GetComponent<SpriteRenderer>().sprite = damagedSprite;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetLink(gameObject);
    }
}
