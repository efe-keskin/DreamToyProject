using DG.Tweening;

public abstract class Combo
{
    public abstract Sequence Play(GridGenerator board, ExplosionService explosions, DamageTransaction transaction, int x, int y);
}
