using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoardFrame : MonoBehaviour
{
    [SerializeField] private GridGenerator board;
    [SerializeField] private float padding = 0.2f;

    void Start()
    {
        Vector3 firstCell = board.GetLocalPosition(0, 0);
        Vector3 lastCell = board.GetLocalPosition(board.Width - 1, board.Height - 1);
        Vector3 cellSize = board.GetLocalPosition(1, 1) - firstCell;

        Vector3 center = (firstCell + lastCell) / 2f;
        float width = lastCell.x - firstCell.x + cellSize.x + padding * 2f;
        float height = lastCell.y - firstCell.y + cellSize.y + padding * 2f;

        transform.position = board.transform.TransformPoint(center);
        GetComponent<SpriteRenderer>().size = new Vector2(width, height);
    }
}
