using NUnit.Framework;

public class SpecialItemFactoryTests
{
    [TestCase(3, SpecialItemKind.None)]
    [TestCase(4, SpecialItemKind.Rocket)]
    [TestCase(5, SpecialItemKind.Rocket)]
    [TestCase(6, SpecialItemKind.Tnt)]
    [TestCase(7, SpecialItemKind.Tnt)]
    public void GroupSize_SelectsExpectedSpecial(int groupSize, SpecialItemKind expected)
    {
        Assert.That(SpecialItemFactory.GetKindForGroupSize(groupSize), Is.EqualTo(expected));
    }
}
