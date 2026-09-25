using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private SpecialItemFactory specialItemFactory;
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private float spacing = 1.1f;
    [SerializeField] private float fallDuration = 0.3f;
    [SerializeField] private float mergeDuration = 0.2f;
    [SerializeField] private float appearDuration = 0.15f;
    [SerializeField] private int startingMoves = 20;

    private const int MinGroupSize = 2;

    public int MovesRemaining { get; private set; }

    public event System.Action<int> MovesChanged;

    private GridItem[,] items;
    private ExplosionService explosions;
    private bool isBusy;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Awake()
    {
        MovesRemaining = startingMoves;
        explosions = new ExplosionService(this, ApplyGravityAndRefill);
    }

    void Start()
    {
        items = new GridItem[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                CreateCube(x, y, GetLocalPosition(x, y));
        }
    }

    public void OnItemClicked(GridItem item)
    {
        if (isBusy || MovesRemaining <= 0)
            return;

        if (item is SpecialItem clickedSpecial)
        {
            SpendMove();
            isBusy = true;
            explosions.Trigger(clickedSpecial);
            return;
        }

        if (item is not Cube clickedCube)
            return;

        List<Cube> group = FindGroup(clickedCube);
        if (group.Count < MinGroupSize)
            return;

        SpendMove();

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

    public Vector3 GetLocalPosition(int x, int y)
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;
        return new Vector3(x * spacing - offsetX, y * spacing - offsetY, 0f);
    }

    public void ClearCell(GridItem item)
    {
        if (items[item.X, item.Y] == item)
            items[item.X, item.Y] = null;
    }

    public void RemoveItem(GridItem item)
    {
        ClearCell(item);
        Destroy(item.gameObject);
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
        isBusy = true;

        foreach (Cube member in group)
            ClearCell(member);

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
        isBusy = true;
        Sequence transition = DOTween.Sequence();

        for (int x = 0; x < width; x++)
        {
            int firstEmptyY = CompactColumn(x, transition);
            RefillColumn(x, firstEmptyY, transition);
        }

        transition.OnComplete(() => isBusy = false);
    }

    private int CompactColumn(int x, Sequence transition)
    {
        int targetY = 0;

        for (int y = 0; y < height; y++)
        {
            GridItem item = items[x, y];
            if (item == null)
                continue;

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

    private List<Cube> FindGroup(Cube start)
    {
        List<Cube> group = new List<Cube>();
        HashSet<Cube> visited = new HashSet<Cube>();
        Queue<Cube> queue = new Queue<Cube>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Cube current = queue.Dequeue();
            group.Add(current);

            foreach (Vector2Int direction in Directions)
            {
                int nx = current.X + direction.x;
                int ny = current.Y + direction.y;

                if (GetItem(nx, ny) is not Cube neighbor || neighbor.Type != start.Type || visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return group;
    }
}
