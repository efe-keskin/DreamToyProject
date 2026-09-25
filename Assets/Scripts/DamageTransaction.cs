using System.Collections.Generic;
using UnityEngine;

public enum DamageSource
{
    Blast,
    SpecialItem
}

public class DamageTransaction
{
    private readonly HashSet<Obstacle> damagedObstacles = new HashSet<Obstacle>();
    private readonly HashSet<Vector2Int> hitCells = new HashSet<Vector2Int>();

    public DamageSource Source { get; }

    public DamageTransaction(DamageSource source)
    {
        Source = source;
    }

    public bool TryMarkDamaged(Obstacle obstacle)
    {
        return damagedObstacles.Add(obstacle);
    }

    public bool HasDamaged(Obstacle obstacle)
    {
        return damagedObstacles.Contains(obstacle);
    }

    public bool TryMarkCell(Vector2Int cell)
    {
        return hitCells.Add(cell);
    }
}
