using NUnit.Framework;
using UnityEngine;

public class ObstacleDamageTests
{
    private GameObject owner;

    [SetUp]
    public void SetUp()
    {
        owner = new GameObject("Obstacle damage test");
        owner.SetActive(false);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void Vase_BlastIsDeduplicatedPerTransaction()
    {
        TestVase vase = CreateVase();
        Assert.That(vase.Health, Is.EqualTo(2), "A Vase starts with two hit points.");

        var blast = new DamageTransaction(DamageSource.Blast);
        vase.ReceiveBlast(blast, 3);
        Assert.That(vase.Health, Is.EqualTo(1), "Multiple adjacent cubes in one blast cause one hit.");

        vase.ReceiveBlast(blast, 3);
        Assert.That(vase.Health, Is.EqualTo(1), "Delivering the same blast again must not cause another hit.");
        Assert.That(vase.IsCleared, Is.False);

        vase.ReceiveBlast(new DamageTransaction(DamageSource.Blast), 1);
        Assert.That(vase.Health, Is.Zero, "A separate blast delivers the second hit.");
        Assert.That(vase.IsCleared, Is.True);
        Assert.That(vase.RemainingGoalCount, Is.Zero);
    }

    [Test]
    public void Stone_RejectsBlastAndAcceptsSpecial()
    {
        TestStone stone = CreateStone();
        Assert.That(stone.Health, Is.EqualTo(1), "A Stone starts with one hit point.");
        Assert.That(stone.CanFall, Is.False, "Stones must remain immobile.");

        var blast = new DamageTransaction(DamageSource.Blast);
        stone.ReceiveBlast(blast, 3);
        Assert.That(stone.Health, Is.EqualTo(1), "A cube blast cannot damage a Stone.");
        Assert.That(stone.IsCleared, Is.False);
        Assert.That(blast.HasDamaged(stone), Is.False, "Rejected damage must not consume a transaction hit.");

        stone.ReceiveSpecialHit(new DamageTransaction(DamageSource.SpecialItem), Vector2Int.zero);
        Assert.That(stone.Health, Is.Zero, "A special-item hit clears a Stone.");
        Assert.That(stone.IsCleared, Is.True);
        Assert.That(stone.RemainingGoalCount, Is.Zero);
    }

    [Test]
    public void LevelGoals_CompleteOnlyAfterEveryRegisteredGoalIsCleared()
    {
        var goals = new LevelGoals();
        Assert.That(goals.IsComplete, Is.True);

        TestVase vase = CreateVase();
        TestStone stone = CreateStone();
        goals.Register(vase);
        goals.Register(stone);
        Assert.That(goals.IsComplete, Is.False);

        vase.ReceiveBlast(new DamageTransaction(DamageSource.Blast), 1);
        goals.Refresh(vase);
        Assert.That(goals.GetRemaining(GoalType.Vase), Is.EqualTo(1));
        Assert.That(goals.IsComplete, Is.False);

        vase.ReceiveBlast(new DamageTransaction(DamageSource.Blast), 1);
        goals.Refresh(vase);
        Assert.That(goals.GetRemaining(GoalType.Vase), Is.Zero);
        Assert.That(goals.IsComplete, Is.False);

        stone.ReceiveSpecialHit(new DamageTransaction(DamageSource.SpecialItem), Vector2Int.zero);
        goals.Refresh(stone);
        Assert.That(goals.GetRemaining(GoalType.Stone), Is.Zero);
        Assert.That(goals.IsComplete, Is.True);
    }

    private TestVase CreateVase()
    {
        TestVase vase = owner.AddComponent<TestVase>();
        Assert.That(vase.Health, Is.Zero,
            "The inactive Vase fixture must not run Awake before explicit test initialization.");
        vase.InitializeForTest();
        return vase;
    }

    private TestStone CreateStone()
    {
        TestStone stone = owner.AddComponent<TestStone>();
        Assert.That(stone.Health, Is.Zero,
            "The inactive Stone fixture must not run Awake before explicit test initialization.");
        stone.InitializeForTest();
        return stone;
    }
}
