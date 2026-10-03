using NUnit.Framework;
using UnityEngine;

public class LevelDataTests
{
    [Test]
    public void LevelData_JsonParsingPreservesCoordinates()
    {
        const string json = @"{
            ""level_number"": 7,
            ""grid_width"": 6,
            ""grid_height"": 6,
            ""move_count"": 19,
            ""grid"": [
                ""r"",    ""rand"", ""rand"", ""rand"", ""rand"", ""g"",
                ""b"",    ""rand"", ""rand"", ""rand"", ""rand"", ""rand"",
                ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"",
                ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"",
                ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""rand"",
                ""rand"", ""rand"", ""rand"", ""rand"", ""rand"", ""t""
            ]
        }";

        LevelData level = JsonUtility.FromJson<LevelData>(json);

        Assert.That(level, Is.Not.Null, "The JSON fixture must deserialize into a level.");
        Assert.That(level.LevelNumber, Is.EqualTo(7), "level_number must map to LevelNumber.");
        Assert.That(level.Width, Is.EqualTo(6), "grid_width must map to Width.");
        Assert.That(level.Height, Is.EqualTo(6), "grid_height must map to Height.");
        Assert.That(level.MoveCount, Is.EqualTo(19), "move_count must map to MoveCount.");
        Assert.That(level.CellCount, Is.EqualTo(36), "The grid must preserve all 36 cells.");
        Assert.That(level.GetToken(0, 0), Is.EqualTo("r"), "(0, 0) is the bottom-left cell.");
        Assert.That(level.GetToken(5, 0), Is.EqualTo("g"), "(5, 0) is the bottom-right cell.");
        Assert.That(level.GetToken(0, 1), Is.EqualTo("b"), "(0, 1) starts the second row from the bottom.");
        Assert.That(level.GetToken(5, 5), Is.EqualTo("t"), "(5, 5) is the top-right cell.");
    }
}
