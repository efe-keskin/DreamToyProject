using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private float spacing = 1.1f;

    void Start()
    {
        float offsetX = (width - 1) * spacing / 2f;
        float offsetY = (height - 1) * spacing / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cube = Instantiate(cubePrefab, transform);
                cube.transform.localPosition = new Vector3(x * spacing - offsetX, y * spacing - offsetY, 0f);
                cube.name = $"Cube_{x}_{y}";

                CubeType randomType = (CubeType)Random.Range(0, 4); // max is exclusive: 0..3
                cube.GetComponent<Cube>().Initialize(randomType);
            }
        }
    }
}
