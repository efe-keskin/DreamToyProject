using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ExplosionService
{
    private const float ConvergeDuration = 0.2f;

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
        Track(item.Explode(board, this), () => UnityEngine.Object.Destroy(item.gameObject));
    }

    public void TriggerCombo(List<SpecialItem> comboItems, SpecialItem tappedItem)
    {
        int x = tappedItem.X;
        int y = tappedItem.Y;
        Combo combo = ComboFactory.Create(comboItems);

        foreach (SpecialItem item in comboItems)
            board.ClearCell(item);

        Vector3 center = board.GetLocalPosition(x, y);
        Sequence converge = DOTween.Sequence();

        foreach (SpecialItem item in comboItems)
            converge.Join(item.MoveTo(center, ConvergeDuration, Ease.InBack));

        converge.AppendCallback(() =>
        {
            foreach (SpecialItem item in comboItems)
                UnityEngine.Object.Destroy(item.gameObject);

            Track(combo.Play(board, this, x, y));
        });

        Track(converge);
    }

    public void DamageCell(int x, int y)
    {
        GridItem target = board.GetItem(x, y);
        if (target == null)
            return;

        if (target is SpecialItem special)
            Trigger(special);
        else if (target is Obstacle obstacle)
            board.DamageObstacle(obstacle, DamageSource.SpecialItem);
        else
            board.RemoveItem(target);
    }

    public void DamageArea(int centerX, int centerY, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
                DamageCell(centerX + dx, centerY + dy);
        }
    }

    private void Track(Sequence sequence, Action onFinished = null)
    {
        activeExplosions++;

        sequence.OnComplete(() =>
        {
            onFinished?.Invoke();
            activeExplosions--;

            if (activeExplosions == 0)
                onAllExplosionsFinished();
        });
    }
}
