using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BoardRecoveryTests
{
    private GameObject owner;
    private GridGenerator board;
    private GridItem[,] items;
    private Random.State randomState;

    [SetUp]
    public void SetUp()
    {
        randomState = Random.state;
        Random.InitState(42);
        owner = new GameObject("Board recovery test");
        owner.SetActive(false);
        board = owner.AddComponent<GridGenerator>();
        board.Initialize(JsonUtility.FromJson<LevelData>(
            "{\"level_number\":1,\"grid_width\":6,\"grid_height\":6,\"move_count\":8}"));
        items = new GridItem[6, 6];
        SetField(board, "items", items);
        Call(board, "Awake");
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();
        Object.DestroyImmediate(owner);
        Random.state = randomState;
    }

    [Test]
    [Category("AssetIntegration")]
    public void SettledDeadBoard_RecoversWithoutChangingCubesObstaclesOrMoves()
    {
        for (int x = 0; x < board.Width; x++)
        {
            AddStone(x, 0);
            for (int y = 1; y < board.Height; y++)
                AddCube(x, y, (x + y) % 2 == 0 ? CubeType.Red : CubeType.Green);
        }

        Assert.That(HasValidMove(), Is.False, "The forced checkerboard has no legal action.");
        foreach (GridItem item in items)
            board.OnItemClicked(item);
        Assert.That(board.MovesRemaining, Is.EqualTo(8), "Every click on the dead board is rejected.");

        var originals = new Dictionary<GridItem, Vector2Int>();
        var colors = new Dictionary<Cube, CubeType>();
        foreach (GridItem item in items)
        {
            originals.Add(item, new Vector2Int(item.X, item.Y));
            if (item is Cube cube)
                colors.Add(cube, cube.Type);
        }

        int settledCount = 0;
        board.BoardSettled += () => settledCount++;
        Call(board, "BeginTransition");
        Call(board, "EndTransition");
        Assert.That(settledCount, Is.Zero, "The shuffle must finish before the board is reported settled.");
        foreach (GridItem item in items)
            board.OnItemClicked(item);
        Assert.That(board.MovesRemaining, Is.EqualTo(8), "Input remains blocked during the shuffle.");
        DOTween.CompleteAll(true);

        Assert.That(HasValidMove(), Is.True, "A dead board must recover after resolution.");
        Assert.That(board.MovesRemaining, Is.EqualTo(8));
        Assert.That(board.Goals.GetRemaining(GoalType.Stone), Is.EqualTo(6));
        Assert.That(settledCount, Is.EqualTo(1));
        var remaining = new HashSet<GridItem>();
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                GridItem item = board.GetItem(x, y);
                Assert.That(remaining.Add(item), Is.True, "Each item occupies exactly one cell.");
                Assert.That(item.X, Is.EqualTo(x));
                Assert.That(item.Y, Is.EqualTo(y));
                Assert.That(Vector3.Distance(item.transform.localPosition, board.GetLocalPosition(x, y)), Is.LessThan(0.001f));
                if (item is Cube cube)
                    Assert.That(cube.Type, Is.EqualTo(colors[cube]));
                else
                {
                    Assert.That(new Vector2Int(x, y), Is.EqualTo(originals[item]));
                    Assert.That(((Obstacle)item).Health, Is.EqualTo(1));
                }
            }
        }
        CollectionAssert.AreEquivalent(originals.Keys, remaining);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SettledPlayableBoard_DoesNotReshuffle(bool useSpecial)
    {
        AddStone(0, 0);
        AddCube(0, 1, CubeType.Red);
        AddCube(1, 1, useSpecial ? CubeType.Green : CubeType.Red);
        if (useSpecial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TntPrefab.prefab");
            Tnt tnt = Object.Instantiate(prefab, owner.transform).GetComponent<Tnt>();
            tnt.AttachToBoard(board, 2, 1);
            items[2, 1] = tnt;
        }

        var before = (GridItem[,])items.Clone();
        int settledCount = 0;
        board.BoardSettled += () => settledCount++;
        Call(board, "BeginTransition");
        Call(board, "EndTransition");

        Assert.That(board.HasValidMove(), Is.True);
        Assert.That(settledCount, Is.EqualTo(1), "An already playable board finishes immediately.");
        CollectionAssert.AreEqual(before, items);
        Assert.That(board.MovesRemaining, Is.EqualTo(8));
    }

    [TestCase(0, true)]
    [TestCase(0, false)]
    [TestCase(8, false)]
    public void FinishedLevel_DoesNotReshuffle(int moves, bool hasObstacles)
    {
        AddCube(0, 1, CubeType.Red);
        AddCube(1, 1, CubeType.Green);
        AddCube(2, 1, CubeType.Red);
        board.Initialize(JsonUtility.FromJson<LevelData>(
            $"{{\"level_number\":1,\"grid_width\":6,\"grid_height\":6,\"move_count\":{moves}}}"));
        if (hasObstacles)
            AddStone(0, 0);

        var before = (GridItem[,])items.Clone();
        int settledCount = 0;
        board.BoardSettled += () => settledCount++;
        Call(board, "BeginTransition");
        Call(board, "EndTransition");

        Assert.That(board.HasValidMove(), Is.False);
        Assert.That(settledCount, Is.EqualTo(1), "Win/loss must be evaluated without waiting for a shuffle.");
        CollectionAssert.AreEqual(before, items);
        Assert.That(board.MovesRemaining, Is.EqualTo(moves));
    }

    [Test]
    public void CompletedGoals_SkipReshuffleBeforeClearedObstacleIsRemoved()
    {
        AddStone(0, 0);
        AddCube(0, 1, CubeType.Red);
        AddCube(1, 1, CubeType.Green);
        AddCube(2, 1, CubeType.Red);

        var stone = (Stone)board.GetItem(0, 0);
        stone.ReceiveSpecialHit(new DamageTransaction(DamageSource.SpecialItem), Vector2Int.zero);
        board.Goals.Refresh(stone);
        Assert.That(board.Goals.IsComplete, Is.True);
        Assert.That(board.HasValidMove(), Is.False);

        var before = (GridItem[,])items.Clone();
        int settledCount = 0;
        board.BoardSettled += () => settledCount++;
        Call(board, "BeginTransition");
        Call(board, "EndTransition");

        Assert.That(settledCount, Is.EqualTo(1));
        CollectionAssert.AreEqual(before, items);
        Assert.That(board.MovesRemaining, Is.EqualTo(8));
    }

    [Test]
    public void ImpossibleReshuffle_ReportsSettledWithoutChangingTheBoard()
    {
        AddStone(0, 0);
        AddCube(0, 1, CubeType.Red);
        AddCube(1, 1, CubeType.Green);
        var before = (GridItem[,])items.Clone();
        int settledCount = 0;
        board.BoardSettled += () => settledCount++;

        Call(board, "BeginTransition");
        Call(board, "EndTransition");

        Assert.That(board.HasValidMove(), Is.False);
        Assert.That(settledCount, Is.EqualTo(1), "An impossible arrangement must not retry forever.");
        CollectionAssert.AreEqual(before, items);
        Assert.That(board.MovesRemaining, Is.EqualTo(8));
    }

    [Test]
    [Category("AssetIntegration")]
    public void Reshuffle_PreservesHolesFallingObstaclesAndMultiCellObstacles()
    {
        AddCube(0, 0, CubeType.Red);
        AddCube(1, 0, CubeType.Green);
        AddCube(0, 1, CubeType.Green);
        AddCube(1, 1, CubeType.Red);
        Vase vase = AddObstacle<Vase>(2, 2);
        ChaliceBox box = AddObstacle<ChaliceBox>(3, 0);
        var before = (GridItem[,])items.Clone();

        Call(board, "BeginTransition");
        Call(board, "EndTransition");
        DOTween.CompleteAll(true);

        Assert.That(board.HasValidMove(), Is.True);
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (before[x, y] is not Cube)
                    Assert.That(items[x, y], Is.SameAs(before[x, y]), $"Cell ({x}, {y}) must be preserved.");
            }
        }
        Assert.That(vase.Health, Is.EqualTo(2));
        Assert.That(box.Health, Is.EqualTo(1));
        Assert.That(box.ChalicesCollected, Is.Zero);
        Assert.That(vase.transform.localPosition, Is.EqualTo(board.GetLocalPosition(2, 2)));
        Assert.That(box.transform.localPosition, Is.EqualTo(board.GetLocalPosition(3, 0)));
        Assert.That(board.Goals.GetRemaining(GoalType.Vase), Is.EqualTo(1));
        Assert.That(board.Goals.GetRemaining(GoalType.Chalice), Is.EqualTo(10));
        Assert.That(board.MovesRemaining, Is.EqualTo(8));
    }

    private T AddObstacle<T>(int x, int y) where T : Obstacle
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{typeof(T).Name}Prefab.prefab");
        T obstacle = Object.Instantiate(prefab, owner.transform).GetComponent<T>();
        Call(obstacle, "Awake");
        obstacle.AttachToBoard(board, x, y);
        obstacle.transform.localPosition = board.GetLocalPosition(x, y);
        for (int dx = 0; dx < obstacle.Size.x; dx++)
        {
            for (int dy = 0; dy < obstacle.Size.y; dy++)
                items[x + dx, y + dy] = obstacle;
        }
        board.Goals.Register(obstacle);
        return obstacle;
    }

    private Cube AddCube(int x, int y, CubeType type)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CubePrefab.prefab");
        Cube cube = Object.Instantiate(prefab, owner.transform).GetComponent<Cube>();
        Call(cube, "Awake");
        cube.Initialize(type, x, y, board);
        cube.transform.localPosition = board.GetLocalPosition(x, y);
        items[x, y] = cube;
        return cube;
    }

    private void AddStone(int x, int y)
    {
        var stoneObject = new GameObject("Stone");
        stoneObject.transform.SetParent(owner.transform);
        TestStone stone = stoneObject.AddComponent<TestStone>();
        stone.InitializeForTest();
        stone.AttachToBoard(board, x, y);
        stone.transform.localPosition = board.GetLocalPosition(x, y);
        items[x, y] = stone;
        board.Goals.Register(stone);
    }

    private bool HasValidMove()
    {
        foreach (GridItem item in items)
        {
            if (item is SpecialItem || item is Cube cube && board.FindCubeGroup(cube).Count >= 2)
                return true;
        }
        return false;
    }

    private static void SetField(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private static void Call(object target, string name)
    {
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
    }
}
