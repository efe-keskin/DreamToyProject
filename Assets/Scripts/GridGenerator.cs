using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private float spacing = 1.1f;
    [SerializeField] private float fallDuration = 0.3f;

    private const int MinGroupSize = 2;

    private Cube[,] cubes;
    private bool isBusy;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Start()
    {
        cubes = new Cube[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cubeObject = Instantiate(cubePrefab, transform);
                cubeObject.transform.localPosition = GetLocalPosition(x, y);

                CubeType randomType = (CubeType)Random.Range(0, 4);
                Cube cube = cubeObject.GetComponent<Cube>();
                cube.Initialize(randomType, x, y, this);
                cubes[x, y] = cube;
            }
        }
    }

    public void OnCubeClicked(Cube cube)
    {
        if (isBusy)
            return;

        List<Cube> group = FindGroup(cube);
        if (group.Count < MinGroupSize)
            return;

        foreach (Cube member in group)
            RemoveCube(member);

        ApplyGravity();
    }

    private Vector3 GetLocalPosition(int x, int y)
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;
        return new Vector3(x * spacing - offsetX, y * spacing - offsetY, 0f);
    }

    private void RemoveCube(Cube cube)
    {
        cubes[cube.X, cube.Y] = null;
        Destroy(cube.gameObject);
    }

    private void ApplyGravity()
    {
        Sequence fall = DOTween.Sequence();

        for (int x = 0; x < width; x++)
        {
            int targetY = 0;

            for (int y = 0; y < height; y++)
            {
                Cube cube = cubes[x, y];
                if (cube == null)
                    continue;

                if (y != targetY)
                {
                    cubes[x, targetY] = cube;
                    cubes[x, y] = null;
                    cube.SetGridPosition(x, targetY);
                    fall.Join(cube.MoveTo(GetLocalPosition(x, targetY), fallDuration));
                }

                targetY++;
            }
        }

        isBusy = true;
        fall.OnComplete(() => isBusy = false);
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

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                Cube neighbor = cubes[nx, ny];
                if (neighbor == null || neighbor.Type != start.Type || visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return group;
    }
}
