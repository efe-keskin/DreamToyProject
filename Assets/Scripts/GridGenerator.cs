using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private SpecialItemFactory specialItemFactory;
    [SerializeField] private Vase vasePrefab;
    [SerializeField] private List<Vector2Int> vaseCells = new List<Vector2Int>();
    [SerializeField] private Stone stonePrefab;
    [SerializeField] private List<Vector2Int> stoneCells = new List<Vector2Int>();
    [SerializeField] private ChaliceBox chaliceBoxPrefab;
    [SerializeField] private List<Vector2Int> chaliceBoxBottomLeftCells = new List<Vector2Int>();
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private float spacing = 1.1f;
    [SerializeField] private float fallDuration = 0.3f;
    [SerializeField] private float mergeDuration = 0.2f;
    [SerializeField] private float appearDuration = 0.15f;
    [SerializeField] private int startingMoves = 20;

    private const int MinGroupSize = 2;

    public int MovesRemaining { get; private set; }
    public int Width => width;
    public int Height => height;

    public event System.Action<int> MovesChanged;

    private GridItem[,] items;
    private ExplosionService explosions;
    private HintPresenter hints;
    private bool isBusy;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Awake()
    {
        MovesRemaining = startingMoves;
        explosions = new ExplosionService(this, ApplyGravityAndRefill);
        hints = new HintPresenter(this);
    }

    void Start()
    {
        items = new GridItem[width, height];

        foreach (Vector2Int bottomLeft in chaliceBoxBottomLeftCells)
            CreateChaliceBox(bottomLeft.x, bottomLeft.y);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                if (items[x, y] != null)
                    continue;

                if (vaseCells.Contains(cell))
                    CreateObstacle(vasePrefab, x, y);
                else if (stoneCells.Contains(cell))
                    CreateObstacle(stonePrefab, x, y);
                else
                    CreateCube(x, y, GetLocalPosition(x, y));
            }
        }

        hints.Refresh();
    }

    public void OnItemClicked(GridItem item)
    {
        if (isBusy || MovesRemaining <= 0)
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
        ClearCells(item);
        Destroy(item.gameObject);
    }

    public void RemoveIfCleared(Obstacle obstacle)
    {
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
            RemoveIfCleared(entry.Key);
        }
    }

    private void CreateObstacle(Obstacle prefab, int x, int y)
    {
        Obstacle obstacle = Instantiate(prefab, transform);
        obstacle.transform.localPosition = GetLocalPosition(x, y);
        obstacle.AttachToBoard(this, x, y);
        items[x, y] = obstacle;
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
    }

    private void SpendMove()
    {
        MovesRemaining--;
        Debug.Log($"Moves remaining: {MovesRemaining}");
        MovesChanged?.Invoke(MovesRemaining);
    }

    private Cube CreateCube(int x, int y, Vector3 spawnPosition)
    {
        GameObject cubeObject = Instantiate(cubePrefab, transform);
        cubeObject.transform.localPosition = spawnPosition;

        CubeType randomType = (CubeType)Random.Range(0, 4);
        Cube cube = cubeObject.GetComponent<Cube>();
        cube.Initialize(randomType, x, y, this);
        items[x, y] = cube;
        return cube;
    }

    private void MergeIntoSpecialItem(List<Cube> group, int x, int y, SpecialItem specialItem)
    {
        BeginTransition();

        foreach (Cube member in group)
            ClearCells(member);

        specialItem.transform.localPosition = GetLocalPosition(x, y);
        specialItem.AttachToBoard(this, x, y);
        items[x, y] = specialItem;

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
        Sequence transition = DOTween.Sequence();

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
        hints.Clear();
    }

    private void EndTransition()
    {
        isBusy = false;
        hints.Refresh();
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
                transition.Join(item.MoveTo(GetLocalPosition(x, targetY), fallDuration));
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
            Cube cube = CreateCube(x, y, GetLocalPosition(x, spawnY));
            transition.Join(cube.MoveTo(GetLocalPosition(x, y), fallDuration));
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
