using UnityEngine;

public abstract class Obstacle : GridItem
{
    public int Health { get; private set; }

    public virtual bool IsCleared => Health <= 0;

    protected abstract int MaxHealth { get; }

    protected virtual void Awake()
    {
        Health = MaxHealth;
    }

    public virtual bool CanTakeDamageFrom(DamageSource source)
    {
        return true;
    }

    public void ReceiveBlast(DamageTransaction transaction, int adjacentCubeCount)
    {
        if (CanTakeDamageFrom(transaction.Source))
            OnBlast(transaction, adjacentCubeCount);
    }

    public void ReceiveSpecialHit(DamageTransaction transaction, Vector2Int cell)
    {
        if (CanTakeDamageFrom(transaction.Source))
            OnSpecialHit(transaction, cell);
    }

    protected virtual void OnBlast(DamageTransaction transaction, int adjacentCubeCount)
    {
        TakeDamageOnce(transaction);
    }

    protected virtual void OnSpecialHit(DamageTransaction transaction, Vector2Int cell)
    {
        TakeDamageOnce(transaction);
    }

    protected void TakeDamageOnce(DamageTransaction transaction)
    {
        if (!transaction.TryMarkDamaged(this))
            return;

        Health--;
        OnDamaged();
    }

    protected virtual void OnDamaged()
    {
    }
}
