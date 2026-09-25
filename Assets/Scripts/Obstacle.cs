public enum DamageSource
{
    Blast,
    SpecialItem
}

public abstract class Obstacle : GridItem
{
    public int Health { get; private set; }

    protected abstract int MaxHealth { get; }

    protected virtual void Awake()
    {
        Health = MaxHealth;
    }

    public virtual bool CanTakeDamageFrom(DamageSource source)
    {
        return true;
    }

    public bool TakeDamage()
    {
        Health--;

        if (Health > 0)
            OnDamaged();

        return Health <= 0;
    }

    protected virtual void OnDamaged()
    {
    }
}
