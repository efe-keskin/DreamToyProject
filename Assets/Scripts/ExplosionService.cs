using System;
using DG.Tweening;

public class ExplosionService
{
    private readonly GridGenerator board;
    private readonly Action onAllExplosionsFinished;
    private int activeExplosions;

    public ExplosionService(GridGenerator board, Action onAllExplosionsFinished)
    {
        this.board = board;
        this.onAllExplosionsFinished = onAllExplosionsFinished;
    }

    public void Trigger(SpecialItem item)
    {
        board.ClearCell(item);
        activeExplosions++;

        Sequence explosion = item.Explode(board, this);
        explosion.OnComplete(() =>
        {
            UnityEngine.Object.Destroy(item.gameObject);
            activeExplosions--;

            if (activeExplosions == 0)
                onAllExplosionsFinished();
        });
    }

    public void DamageCell(int x, int y)
    {
        GridItem target = board.GetItem(x, y);
        if (target == null)
            return;

        if (target is SpecialItem special)
            Trigger(special);
        else
            board.RemoveItem(target);
    }
}
