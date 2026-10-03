using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private SpecialItemFactory specialItemFactory;
    [SerializeField] private Vase vasePrefab;
    [SerializeField] private Stone stonePrefab;
    [SerializeField] private ChaliceBox chaliceBoxPrefab;
    [SerializeField] private float spacing = 1.1f;
    [SerializeField] private float fallDuration = 0.3f;
    [SerializeField, Min(0f)] private float fallBounceHeight = 0.1f;
    [SerializeField, Min(0f)] private float fallBounceDuration = 0.16f;
    [SerializeField] private float mergeDuration = 0.2f;
    [SerializeField] private float appearDuration = 0.15f;

    private const int MinGroupSize = 2;
    private const float ReshuffleDuration = 0.3f;

    public int MovesRemaining { get; private set; }
    public LevelGoals Goals { get; } = new LevelGoals();
    public int Width => width;
    public int Height => height;

    public event System.Action<int> MovesChanged;
    public event System.Action BoardCreated;
    public event System.Action BoardSettled;

    private LevelData level;
    private int width;
    private int height;
    private GridItem[,] items;
    private ExplosionService explosions;
    private HintPresenter hints;
    private bool isBusy;
    private bool inputDisabled;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Awake()
    {
        explosions = new ExplosionService(this, ApplyGravityAndRefill);
        hints = new HintPresenter(this);
    }

    public void Initialize(LevelData levelData)
    {
        level = levelData;
        width = level.Width;
        height = level.Height;
        MovesRemaining = level.MoveCount;
    }

    void Start()
    {
        BeginTransition();
        items = new GridItem[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                CreateFromToken(level.GetToken(x, y), x, y);
        }

        BoardCreated?.Invoke();
        EndTransition();
    }

    public bool HasRemainingObstacles()
    {
        foreach (GridItem item in items)
        {
            if (item is Obstacle)
                return true;
        }

        return false;
    }

    public bool HasValidMove()
    {
        foreach (GridItem item in items)
        {
            if (item is SpecialItem)
                return true;

            if (item is Cube cube && FindCubeGroup(cube).Count >= MinGroupSize)
                return true;
        }

        return false;
    }

    public void DisableInput()
    {
        inputDisabled = true;
        hints.Clear();
    }

    public void OnItemClicked(GridItem item)
    {
        if (inputDisabled || isBusy || MovesRemaining <= 0)
            return;

        if (item is SpecialItem clickedSpecial)
        {
            List<SpecialItem> connected = FindConnected(clickedSpecial, _ => true);

            SpendMove();
            BeginTransition();

            if (connected.Count == 1)
                explosions.Trigger(clickedSpecial);
            else
                explosions.TriggerCombo(connected, clickedSpecial);

            return;
        }

        if (item is not Cube clickedCube)
            return;

        List<Cube> group = FindCubeGroup(clickedCube);
        if (group.Count < MinGroupSize)
            return;

        SpendMove();
        DamageAdjacentObstacles(group);

        SpecialItem specialItem = specialItemFactory.CreateForGroupSize(group.Count, transform);
        if (specialItem == null)
        {
            foreach (Cube member in group)
                RemoveItem(member);

            ApplyGravityAndRefill();
        }
        else
        {
            MergeIntoSpecialItem(group, clickedCube.X, clickedCube.Y, specialItem);
        }
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public GridItem GetItem(int x, int y)
    {
        return IsInside(x, y) ? items[x, y] : null;
    }

    public List<Cube> FindCubeGroup(Cube start)
    {
        return FindConnected(start, cube => cube.Type == start.Type);
    }

    public Vector3 GetLocalPosition(int x, int y)
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;
        return new Vector3(x * spacing - offsetX, y * spacing - offsetY, 0f);
    }

    public void ClearCells(GridItem item)
    {
        for (int dx = 0; dx < item.Size.x; dx++)
        {
            for (int dy = 0; dy < item.Size.y; dy++)
            {
                if (GetItem(item.X + dx, item.Y + dy) == item)
                    items[item.X + dx, item.Y + dy] = null;
            }
        }
    }

    public void RemoveItem(GridItem item)
    {
        if (item is Cube cube)
            hints.ClearGroup(FindCubeGroup(cube));

        ClearCells(item);
        item.PlayBreakEffect();
        Destroy(item.gameObject);
    }

    public void ResolveObstacleHit(Obstacle obstacle)
    {
        Goals.Refresh(obstacle);

        if (obstacle.IsCleared)
            RemoveItem(obstacle);
    }

    private void DamageAdjacentObstacles(List<Cube> group)
    {
        Dictionary<Obstacle, int> adjacentCubeCounts = new Dictionary<Obstacle, int>();

        foreach (Cube cube in group)
        {
            HashSet<Obstacle> touchedByThisCube = new HashSet<Obstacle>();

            foreach (Vector2Int direction in Directions)
            {
                if (GetItem(cube.X + direction.x, cube.Y + direction.y) is Obstacle obstacle)
                    touchedByThisCube.Add(obstacle);
            }

            foreach (Obstacle obstacle in touchedByThisCube)
            {
                adjacentCubeCounts.TryGetValue(obstacle, out int count);
                adjacentCubeCounts[obstacle] = count + 1;
            }
        }

        DamageTransaction transaction = new DamageTransaction(DamageSource.Blast);

        foreach (KeyValuePair<Obstacle, int> entry in adjacentCubeCounts)
        {
            entry.Key.ReceiveBlast(transaction, entry.Value);
            ResolveObstacleHit(entry.Key);
        }
    }

    private void CreateFromToken(string token, int x, int y)
    {
        switch (token)
        {
            case LevelTokens.Red:
                CreateCube(x, y, GetLocalPosition(x, y), CubeType.Red);
                break;
            case LevelTokens.Green:
                CreateCube(x, y, GetLocalPosition(x, y), CubeType.Green);
                break;
            case LevelTokens.Blue:
                CreateCube(x, y, GetLocalPosition(x, y), CubeType.Blue);
                break;
            case LevelTokens.Yellow:
                CreateCube(x, y, GetLocalPosition(x, y), CubeType.Yellow);
                break;
            case LevelTokens.RandomCube:
                CreateCube(x, y, GetLocalPosition(x, y), RandomCubeType());
                break;
            case LevelTokens.VerticalRocket:
                PlaceItem(specialItemFactory.CreateRocket(RocketOrientation.Vertical, transform), x, y);
                break;
            case LevelTokens.HorizontalRocket:
                PlaceItem(specialItemFactory.CreateRocket(RocketOrientation.Horizontal, transform), x, y);
                break;
            case LevelTokens.Tnt:
                PlaceItem(specialItemFactory.CreateTnt(transform), x, y);
                break;
            case LevelTokens.Stone:
                PlaceObstacle(Instantiate(stonePrefab, transform), x, y);
                break;
            case LevelTokens.Vase:
                PlaceObstacle(Instantiate(vasePrefab, transform), x, y);
                break;
            case LevelTokens.ChaliceBoxBottomLeft:
                CreateChaliceBox(x, y);
                break;
        }
    }

    private void PlaceItem(GridItem item, int x, int y)
    {
        item.transform.localPosition = GetLocalPosition(x, y);
        item.AttachToBoard(this, x, y);
        items[x, y] = item;
    }

    private void PlaceObstacle(Obstacle obstacle, int x, int y)
    {
        PlaceItem(obstacle, x, y);
        Goals.Register(obstacle);
    }

    private void CreateChaliceBox(int x, int y)
    {
        ChaliceBox box = Instantiate(chaliceBoxPrefab, transform);
        box.transform.localPosition = (GetLocalPosition(x, y) + GetLocalPosition(x + 1, y + 1)) / 2f;
        box.transform.localScale = new Vector3(1f + spacing, 1f + spacing, 1f);
        box.AttachToBoard(this, x, y);

        for (int dx = 0; dx < box.Size.x; dx++)
        {
            for (int dy = 0; dy < box.Size.y; dy++)
                items[x + dx, y + dy] = box;
        }

        Goals.Register(box);
    }

    private void SpendMove()
    {
        MovesRemaining--;
        Debug.Log($"Moves remaining: {MovesRemaining}");
        MovesChanged?.Invoke(MovesRemaining);
    }

    private Cube CreateCube(int x, int y, Vector3 spawnPosition, CubeType type)
    {
        GameObject cubeObject = Instantiate(cubePrefab, transform);
        cubeObject.transform.localPosition = spawnPosition;

        Cube cube = cubeObject.GetComponent<Cube>();
        cube.Initialize(type, x, y, this);
        items[x, y] = cube;
        return cube;
    }

    private static CubeType RandomCubeType()
    {
        return (CubeType)Random.Range(0, 4);
    }

    private void MergeIntoSpecialItem(List<Cube> group, int x, int y, SpecialItem specialItem)
    {
        BeginTransition();

        foreach (Cube member in group)
            ClearCells(member);

        PlaceItem(specialItem, x, y);

        Vector3 target = GetLocalPosition(x, y);
        Sequence merge = DOTween.Sequence();

        foreach (Cube member in group)
            merge.Join(member.MoveTo(target, mergeDuration, Ease.InBack));

        merge.AppendCallback(() =>
        {
            foreach (Cube member in group)
                Destroy(member.gameObject);
        });
        merge.Append(specialItem.PlayAppear(appearDuration));
        merge.OnComplete(ApplyGravityAndRefill);
    }

    private void ApplyGravityAndRefill()
    {
        BeginTransition();
        Sequence transition = DOTween.Sequence().SetLink(gameObject);

        for (int x = 0; x < width; x++)
        {
            int firstEmptyY = CompactColumn(x, transition);
            RefillColumn(x, firstEmptyY, transition);
        }

        transition.OnComplete(EndTransition);
    }

    private void BeginTransition()
    {
        isBusy = true;
    }

    private void EndTransition()
    {
        // A finished level takes priority over recovery, including a win on the last move.
        if (!inputDisabled && MovesRemaining > 0 && HasRemainingObstacles() && !HasValidMove()
            && TryReshuffleCubes())
            return;

        FinishTransition();
    }

    private bool TryReshuffleCubes()
    {
        var cubes = new List<Cube>();
        var colors = new List<CubeType>();
        var cells = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (items[x, y] is not Cube cube)
                    continue;

                cubes.Add(cube);
                colors.Add(cube.Type);
                cells.Add(new Vector2Int(x, y));
            }
        }

        int[] order = CubeShuffle.Create(colors, cells);
        if (order == null)
            return false;

        hints.Clear();
        Sequence shuffle = DOTween.Sequence().SetLink(gameObject);
        for (int i = 0; i < cells.Count; i++)
        {
            Cube cube = cubes[order[i]];
            Vector2Int cell = cells[i];
            items[cell.x, cell.y] = cube;
            cube.SetGridPosition(cell.x, cell.y);
            shuffle.Join(cube.MoveTo(GetLocalPosition(cell.x, cell.y), ReshuffleDuration));
        }

        shuffle.OnComplete(FinishTransition);
        return true;
    }

    private void FinishTransition()
    {
        if (!inputDisabled)
            hints.Refresh();

        isBusy = false;
        BoardSettled?.Invoke();
    }

    private int CompactColumn(int x, Sequence transition)
    {
        int targetY = 0;

        for (int y = 0; y < height; y++)
        {
            GridItem item = items[x, y];
            if (item == null)
                continue;

            if (!item.CanFall)
            {
                targetY = y + 1;
                continue;
            }

            if (y != targetY)
            {
                items[x, targetY] = item;
                items[x, y] = null;
                item.SetGridPosition(x, targetY);
                transition.Join(item.FallTo(GetLocalPosition(x, targetY), fallDuration, spacing * fallBounceHeight, fallBounceDuration));
            }

            targetY++;
        }

        return targetY;
    }

    private void RefillColumn(int x, int firstEmptyY, Sequence transition)
    {
        for (int y = firstEmptyY; y < height; y++)
        {
            int spawnY = height + (y - firstEmptyY);
            Cube cube = CreateCube(x, y, GetLocalPosition(x, spawnY), RandomCubeType());
            transition.Join(cube.FallTo(GetLocalPosition(x, y), fallDuration, spacing * fallBounceHeight, fallBounceDuration));
        }
    }

    private List<T> FindConnected<T>(T start, System.Predicate<T> matches) where T : GridItem
    {
        List<T> group = new List<T>();
        HashSet<T> visited = new HashSet<T>();
        Queue<T> queue = new Queue<T>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            T current = queue.Dequeue();
            group.Add(current);

            foreach (Vector2Int direction in Directions)
            {
                int nx = current.X + direction.x;
                int ny = current.Y + direction.y;

                if (GetItem(nx, ny) is not T neighbor || !matches(neighbor) || visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return group;
    }
}
