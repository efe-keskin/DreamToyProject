using DG.Tweening;
using UnityEngine;

public abstract class Combo
{
    protected readonly Sprite sprite;
    protected readonly Color color;

    protected Combo(SpriteRenderer visualSource)
    {
        sprite = visualSource.sprite;
        color = visualSource.color;
    }

    public abstract Sequence Play(GridGenerator board, ExplosionService explosions, int x, int y);
}
