using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class ComboFactoryTests
{
    private readonly List<GameObject> ownedObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject owner in ownedObjects)
            Object.DestroyImmediate(owner);

        ownedObjects.Clear();
    }

    [TestCase("RR", typeof(RocketRocketCombo))]
    [TestCase("TR", typeof(TntRocketCombo))]
    [TestCase("RT", typeof(TntRocketCombo))]
    [TestCase("TT", typeof(TntTntCombo))]
    public void ComboFactory_SelectsPairStrategy(string arrangement, Type expected)
    {
        Combo combo = ComboFactory.Create(CreateItems(arrangement));

        Assert.That(combo, Is.TypeOf(expected), $"Combo selection for {arrangement}");
    }

    [Test]
    public void ComboFactory_TwoTntsTakePrecedence()
    {
        foreach (string arrangement in new[] { "TTR", "TRT", "RTT" })
        {
            Combo combo = ComboFactory.Create(CreateItems(arrangement));

            Assert.That(combo, Is.TypeOf<TntTntCombo>(),
                $"Two TNTs must take precedence in {arrangement}");
        }
    }

    [Test]
    public void ComboFactory_RejectsNullGroup()
    {
        ArgumentNullException error = Assert.Throws<ArgumentNullException>(() => ComboFactory.Create(null));

        Assert.That(error.ParamName, Is.EqualTo("items"));
    }

    [TestCase("")]
    [TestCase("R")]
    [TestCase("T")]
    public void ComboFactory_RejectsGroupsWithoutASupportedPair(string arrangement)
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => ComboFactory.Create(CreateItems(arrangement)));

        Assert.That(error.ParamName, Is.EqualTo("items"));
        Assert.That(error.Message, Does.Contain("at least two supported special items"));
    }

    [TestCase("R")]
    [TestCase("T")]
    public void ComboFactory_NullMemberDoesNotCompleteAPair(string arrangement)
    {
        List<SpecialItem> items = CreateItems(arrangement);
        items.Add(null);

        Assert.Throws<ArgumentException>(() => ComboFactory.Create(items));
    }

    private List<SpecialItem> CreateItems(string arrangement)
    {
        var items = new List<SpecialItem>();

        foreach (char kind in arrangement)
        {
            var owner = new GameObject($"Combo test {kind}");
            ownedObjects.Add(owner);
            owner.SetActive(false);
            owner.AddComponent<SpriteRenderer>();

            SpecialItem item = kind == 'T'
                ? (SpecialItem)owner.AddComponent<Tnt>()
                : owner.AddComponent<Rocket>();
            items.Add(item);
        }

        return items;
    }
}
