using NUnit.Framework;

public class LevelLoaderTests
{
    [Test]
    [Category("AssetIntegration")]
    public void LevelLoader_LoadsAllTenLevels()
    {
        for (int levelNumber = 1; levelNumber <= 10; levelNumber++)
        {
            LevelData level = null;
            Assert.DoesNotThrow(() => level = LevelLoader.Load(levelNumber),
                $"Level {levelNumber} must load and pass the public loader's validation.");
            Assert.That(level.LevelNumber, Is.EqualTo(levelNumber),
                $"Level {levelNumber} must report the requested level number.");
        }
    }
}
