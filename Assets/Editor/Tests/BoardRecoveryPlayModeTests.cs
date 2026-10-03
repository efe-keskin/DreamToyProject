using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public class BoardRecoveryPlayModeTests
{
    private GameObject owner;
    private Random.State randomState;
    private float previousTimeScale;
    private float previousCaptureDeltaTime;
    private bool savedRuntimeState;

    [UnityTest]
    [Category("PlayModeIntegration")]
    public IEnumerator DeadRefill_RecoversAfterFalling_WithoutSpendingAnotherMove()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        yield return RunScenario();
    }

    private IEnumerator RunScenario()
    {
        randomState = Random.state;
        previousTimeScale = Time.timeScale;
        previousCaptureDeltaTime = Time.captureDeltaTime;
        savedRuntimeState = true;
        Random.InitState(731);
        Time.timeScale = 1f;
        // Keep animation-boundary checks independent of headless rendering speed.
        Time.captureDeltaTime = 0.02f;

        owner = new GameObject("Board recovery test");
        owner.SetActive(false);
        GridGenerator board = owner.AddComponent<GridGenerator>();
        ConfigurePrefabs(board);
        board.Initialize(CreateLevel());
        int settledCount = 0;
        board.BoardSettled += () => settledCount++;
        owner.SetActive(true);

        yield return WaitUntil(() => settledCount == 1, "The initial board did not settle.");
        Assert.That(board.HasValidMove(), Is.True);
        Assert.That(board.FindCubeGroup((Cube)board.GetItem(0, 1)).Count, Is.EqualTo(2));
        settledCount = 0;

        int movesBeforeClick = board.MovesRemaining;
        board.OnItemClicked(board.GetItem(0, 1));
        Assert.That(board.MovesRemaining, Is.EqualTo(movesBeforeClick - 1));

        // Refills are assigned immediately but still falling. Force a dead outcome
        // without moving their transforms, then let the real transition finish.
        var beforeShuffle = new GridItem[board.Width, board.Height];
        var cubeColors = new Dictionary<Cube, CubeType>();
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                GridItem item = board.GetItem(x, y);
                beforeShuffle[x, y] = item;
                if (item is Cube cube)
                {
                    CubeType color = (x + y) % 2 == 0 ? CubeType.Red : CubeType.Blue;
                    cube.Initialize(color, x, y, board);
                    cubeColors.Add(cube, color);
                }
            }
        }

        Assert.That(board.HasValidMove(), Is.False, "The forced refill must have no legal group.");
        float fallingStarted = Time.time;
        while (Time.time - fallingStarted < 0.1f)
        {
            Assert.That(board.HasValidMove(), Is.False, "Recovery must wait for gravity to finish.");
            Assert.That(settledCount, Is.Zero);
            AssertSameCells(board, beforeShuffle);
            yield return null;
        }

        yield return WaitUntil(board.HasValidMove, "The dead board was not reshuffled.");
        Assert.That(settledCount, Is.Zero, "BoardSettled must wait for the shuffle animation.");
        board.OnItemClicked(FindPlayableCube(board));
        Assert.That(board.MovesRemaining, Is.EqualTo(movesBeforeClick - 1),
            "Input must remain locked while shuffled cubes are moving.");

        yield return WaitUntil(() => settledCount == 1, "The reshuffle did not finish.");
        Assert.That(board.HasValidMove(), Is.True);
        Assert.That(board.MovesRemaining, Is.EqualTo(movesBeforeClick - 1),
            "Recovery must not consume an extra move.");

        var remainingCubes = new HashSet<Cube>(cubeColors.Keys);
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                GridItem item = board.GetItem(x, y);
                Assert.That(item.X, Is.EqualTo(x));
                Assert.That(item.Y, Is.EqualTo(y));
                Assert.That(Vector3.Distance(item.transform.localPosition, board.GetLocalPosition(x, y)),
                    Is.LessThan(0.001f), "The visual position must match the logical cell.");
                if (item is Cube cube)
                {
                    Assert.That(remainingCubes.Remove(cube), Is.True, "Recovery must preserve each cube exactly once.");
                    Assert.That(cube.Type, Is.EqualTo(cubeColors[cube]), "Recovery must preserve cube colors.");
                }
                else
                {
                    Assert.That(item, Is.SameAs(beforeShuffle[x, y]), "Fixed obstacles must stay in place.");
                    Assert.That(((Stone)item).Health, Is.EqualTo(1));
                }
            }
        }

        Assert.That(remainingCubes, Is.Empty);
        yield return null;
        Assert.That(settledCount, Is.EqualTo(1), "A recovered turn must settle exactly once.");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (!Application.isPlaying)
            yield break;

        if (owner != null)
            Object.Destroy(owner);
        yield return null;

        if (savedRuntimeState)
        {
            Random.state = randomState;
            Time.timeScale = previousTimeScale;
            Time.captureDeltaTime = previousCaptureDeltaTime;
            savedRuntimeState = false;
        }

        yield return new ExitPlayMode();
    }

    private static IEnumerator WaitUntil(System.Func<bool> condition, string failure)
    {
        float deadline = Time.realtimeSinceStartup + 3f;
        while (!condition() && Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.That(condition(), Is.True, failure);
    }

    private static void AssertSameCells(GridGenerator board, GridItem[,] expected)
    {
        for (int x = 0; x < board.Width; x++)
            for (int y = 0; y < board.Height; y++)
                Assert.That(board.GetItem(x, y), Is.SameAs(expected[x, y]));
    }

    private static Cube FindPlayableCube(GridGenerator board)
    {
        for (int x = 0; x < board.Width; x++)
            for (int y = 0; y < board.Height; y++)
                if (board.GetItem(x, y) is Cube cube && board.FindCubeGroup(cube).Count >= 2)
                    return cube;
        Assert.Fail("The recovered board needs a playable cube group.");
        return null;
    }

    private static LevelData CreateLevel()
    {
        string[] colors = { LevelTokens.Red, LevelTokens.Green, LevelTokens.Blue, LevelTokens.Yellow };
        var tokens = new string[36];
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
                tokens[y * 6 + x] = y == 0 ? LevelTokens.Stone : colors[(x + y) % 4];
        tokens[6] = tokens[7] = LevelTokens.Red;
        return JsonUtility.FromJson<LevelData>("{\"level_number\":1,\"grid_width\":6,\"grid_height\":6,"
            + "\"move_count\":10,\"grid\":[\"" + string.Join("\",\"", tokens) + "\"]}");
    }

    private static void ConfigurePrefabs(GridGenerator board)
    {
        var factory = new SpecialItemFactory();
        SetField(factory, "rocketPrefab", LoadPrefab<Rocket>("RocketPrefab"));
        SetField(factory, "tntPrefab", LoadPrefab<Tnt>("TntPrefab"));
        SetField(board, "specialItemFactory", factory);
        SetField(board, "cubePrefab", LoadPrefab<Cube>("CubePrefab").gameObject);
        SetObstacles(board,
            (LevelTokens.Stone, LoadPrefab<Stone>("StonePrefab")),
            (LevelTokens.Vase, LoadPrefab<Vase>("VasePrefab")));
        SetField(board, "chaliceBoxPrefab", LoadPrefab<ChaliceBox>("ChaliceBoxPrefab"));
    }

    private static void SetObstacles(GridGenerator board, params (string token, Obstacle prefab)[] entries)
    {
        FieldInfo field = typeof(GridGenerator).GetField("obstacles", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, "Missing fixture field: obstacles");

        System.Type entryType = field.FieldType.GetElementType();
        System.Array table = System.Array.CreateInstance(entryType, entries.Length);
        for (int i = 0; i < entries.Length; i++)
        {
            object entry = System.Activator.CreateInstance(entryType, true);
            SetField(entry, "token", entries[i].token);
            SetField(entry, "prefab", entries[i].prefab);
            table.SetValue(entry, i);
        }

        field.SetValue(board, table);
    }

    private static T LoadPrefab<T>(string name) where T : Component
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{name}.prefab");
        Assert.That(prefab, Is.Not.Null, $"Missing prefab: {name}");
        T component = prefab.GetComponent<T>();
        Assert.That(component, Is.Not.Null);
        return component;
    }

    private static void SetField(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing fixture field: {name}");
        field.SetValue(target, value);
    }
}
