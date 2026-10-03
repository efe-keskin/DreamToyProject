using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class CubeShuffleTests
{
    private Random.State randomState;

    [SetUp]
    public void SetUp()
    {
        randomState = Random.state;
    }

    [TearDown]
    public void TearDown()
    {
        Random.state = randomState;
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SparseBoard_AlwaysPreservesCubesAndCreatesAnAdjacentPair(bool vertical)
    {
        CubeType[] colors = { CubeType.Green, CubeType.Blue, CubeType.Red, CubeType.Yellow, CubeType.Red };
        Vector2Int[] cells =
        {
            Vector2Int.zero,
            vertical ? Vector2Int.up : Vector2Int.right,
            new Vector2Int(3, 2),
            new Vector2Int(6, 5),
            new Vector2Int(9, 9)
        };
        CubeType[] originalColors = (CubeType[])colors.Clone();
        Vector2Int[] originalCells = (Vector2Int[])cells.Clone();

        for (int seed = 0; seed < 128; seed++)
        {
            Random.InitState(seed);
            int[] order = CubeShuffle.Create(colors, cells);

            Assert.That(order, Is.Not.Null, $"Seed {seed}: a valid permutation exists.");
            Assert.That(order, Is.EquivalentTo(new[] { 0, 1, 2, 3, 4 }),
                $"Seed {seed}: every existing cube must appear exactly once.");
            Assert.That(colors[order[0]], Is.EqualTo(CubeType.Red),
                $"Seed {seed}: only Red has a pair and only the first two slots touch.");
            Assert.That(colors[order[1]], Is.EqualTo(CubeType.Red));
            Assert.That(colors, Is.EqualTo(originalColors), "Planning must not recolor cubes.");
            Assert.That(cells, Is.EqualTo(originalCells), "Planning must not change occupied cells.");
        }
    }

    [TestCaseSource(nameof(ImpossibleBoards))]
    public void ImpossibleBoard_ReturnsNoPermutation(CubeType[] colors, Vector2Int[] cells)
    {
        CubeType[] originalColors = (CubeType[])colors.Clone();
        Vector2Int[] originalCells = (Vector2Int[])cells.Clone();

        Assert.That(CubeShuffle.Create(colors, cells), Is.Null);
        Assert.That(colors, Is.EqualTo(originalColors));
        Assert.That(cells, Is.EqualTo(originalCells));
    }

    private static IEnumerable<TestCaseData> ImpossibleBoards()
    {
        yield return new TestCaseData(new CubeType[0], new Vector2Int[0])
            .SetName("EmptyBoard_HasNoCubePreservingRecovery");
        yield return new TestCaseData(new[] { CubeType.Red }, new[] { Vector2Int.zero })
            .SetName("SingleCube_HasNoCubePreservingRecovery");
        yield return new TestCaseData(
                new[] { CubeType.Red, CubeType.Green, CubeType.Blue, CubeType.Yellow },
                new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.one })
            .SetName("AllColorsDistinct_HaveNoCubePreservingRecovery");
        yield return new TestCaseData(
                new[] { CubeType.Red, CubeType.Red },
                new[] { Vector2Int.zero, Vector2Int.one })
            .SetName("OnlyDiagonalCubeSlots_HaveNoCubePreservingRecovery");
        yield return new TestCaseData(
                new[] { CubeType.Red, CubeType.Red, CubeType.Blue },
                new[] { Vector2Int.zero, new Vector2Int(0, 2), new Vector2Int(4, 0) })
            .SetName("SeparatedCubeSlots_HaveNoCubePreservingRecovery");
    }
}
