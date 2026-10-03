using NUnit.Framework;
using UnityEngine;

public class DamageTransactionTests
{
    [Test]
    public void DamageTransaction_DeduplicatesCells()
    {
        var transaction = new DamageTransaction(DamageSource.SpecialItem);
        var cellA = new Vector2Int(2, 3);
        var cellB = new Vector2Int(3, 2);

        Assert.That(transaction.TryMarkCell(cellA), Is.True, "The first hit on cell A must count.");
        Assert.That(transaction.TryMarkCell(new Vector2Int(2, 3)), Is.False,
            "Repeating cell A in the same transaction must not count again.");
        Assert.That(transaction.TryMarkCell(cellB), Is.True, "A distinct cell B must count independently.");

        var nextTransaction = new DamageTransaction(DamageSource.SpecialItem);
        Assert.That(nextTransaction.TryMarkCell(cellA), Is.True,
            "An independent transaction must be able to hit cell A again.");
    }
}
