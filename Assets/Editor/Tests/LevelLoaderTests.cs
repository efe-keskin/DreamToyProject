using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class LevelLoaderTests
{
    [Test]
    [Category("AssetIntegration")]
    public void LevelLoader_LoadsAllLevelsInSequence()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Levels");
        Assert.That(files, Is.Not.Empty, "At least one playable level must be included.");
        Assert.That(LevelProgression.LevelCount, Is.EqualTo(files.Length));
        Assert.That(files.Select(file => file.name),
            Is.EquivalentTo(Enumerable.Range(1, files.Length).Select(number => $"level_{number:00}")),
            "Level files must be numbered consecutively from level_01 without gaps or duplicates.");

        for (int levelNumber = 1; levelNumber <= files.Length; levelNumber++)
        {
            LevelData level = null;
            Assert.DoesNotThrow(() => level = LevelLoader.Load(levelNumber),
                $"Level {levelNumber} must load and pass the public loader's validation.");
            Assert.That(level.LevelNumber, Is.EqualTo(levelNumber),
                $"Level {levelNumber} must report the requested level number.");
        }
    }
}
