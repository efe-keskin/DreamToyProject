public class Stone : Obstacle
{
    protected override int MaxHealth => 1;

    public override bool CanFall => false;

    public override bool CanTakeDamageFrom(DamageSource source)
    {
        return source == DamageSource.SpecialItem;
    }
}
