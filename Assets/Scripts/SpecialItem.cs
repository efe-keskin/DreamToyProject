using DG.Tweening;

public abstract class SpecialItem : GridItem
{
    public abstract Sequence Explode(GridGenerator board, ExplosionService explosions);
}
