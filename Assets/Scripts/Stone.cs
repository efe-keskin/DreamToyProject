using UnityEngine;

public class Stone : Obstacle
{
    [SerializeField] private Sprite[] fragmentSprites;
    [SerializeField] private Sprite dustSprite;

    public override GoalType GoalType => GoalType.Stone;

    protected override int MaxHealth => 1;

    public override bool CanFall => false;

    public override bool CanTakeDamageFrom(DamageSource source)
    {
        return source == DamageSource.SpecialItem;
    }

    public override void PlayBreakEffect()
    {
        StoneBreakEffect.Play(Board, transform.localPosition, fragmentSprites, dustSprite);
    }
}
