using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BoardCameraFitter : MonoBehaviour
{
    [SerializeField] private GridGenerator board;
    [SerializeField] private float padding = 0.5f;
    [SerializeField] private float spaceAboveBoard = 1f;

    private Camera targetCamera;
    private float fittedAspect;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    void Start()
    {
        Fit();
    }

    void LateUpdate()
    {
        if (!Mathf.Approximately(targetCamera.aspect, fittedAspect))
            Fit();
    }

    private void Fit()
    {
        Vector3 bottomLeft = board.transform.TransformPoint(board.GetLocalPosition(0, 0));
        Vector3 topRight = board.transform.TransformPoint(board.GetLocalPosition(board.Width - 1, board.Height - 1));
        Vector3 cellSize = board.transform.TransformPoint(board.GetLocalPosition(1, 1)) - bottomLeft;

        float boardWidth = topRight.x - bottomLeft.x + cellSize.x;
        float boardHeight = topRight.y - bottomLeft.y + cellSize.y;
        Vector3 boardCenter = (bottomLeft + topRight) / 2f;

        float halfHeightNeeded = boardHeight / 2f + padding + spaceAboveBoard / 2f;
        float halfWidthNeeded = boardWidth / 2f + padding;

        targetCamera.orthographicSize = Mathf.Max(halfHeightNeeded, halfWidthNeeded / targetCamera.aspect);
        transform.position = new Vector3(boardCenter.x, boardCenter.y + spaceAboveBoard / 2f, transform.position.z);
        fittedAspect = targetCamera.aspect;
    }
}
