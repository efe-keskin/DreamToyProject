using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BoardCameraFitter : MonoBehaviour
{
    [SerializeField] private GridGenerator board;
    [SerializeField] private RectTransform hud;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private float padding = 0.5f;
    [SerializeField] private float gapBelowHud = 0.2f;

    private readonly Vector3[] hudCorners = new Vector3[4];
    private Camera targetCamera;
    private float fittedAspect;
    private float fittedHudFraction;

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
        if (!Mathf.Approximately(targetCamera.aspect, fittedAspect) || !Mathf.Approximately(HudScreenFraction(), fittedHudFraction))
            Fit();
    }

    private float HudScreenFraction()
    {
        hud.GetWorldCorners(hudCorners);
        return 1f - hudCorners[0].y / Screen.height;
    }

    private void Fit()
    {
        Vector3 bottomLeft = board.transform.TransformPoint(board.GetLocalPosition(0, 0));
        Vector3 topRight = board.transform.TransformPoint(board.GetLocalPosition(board.Width - 1, board.Height - 1));
        Vector3 cellSize = board.transform.TransformPoint(board.GetLocalPosition(1, 1)) - bottomLeft;

        float boardWidth = topRight.x - bottomLeft.x + cellSize.x;
        float boardHeight = topRight.y - bottomLeft.y + cellSize.y;
        float boardCenterX = (bottomLeft.x + topRight.x) / 2f;
        float boardTop = topRight.y + cellSize.y / 2f;
        float hudFraction = HudScreenFraction();

        float halfWidthNeeded = boardWidth / 2f + padding;
        float halfHeightNeeded = (boardHeight + gapBelowHud + padding) / (2f * (1f - hudFraction));
        float size = Mathf.Max(halfHeightNeeded, halfWidthNeeded / targetCamera.aspect);

        targetCamera.orthographicSize = size;
        transform.position = new Vector3(boardCenterX, boardTop + gapBelowHud - size * (1f - 2f * hudFraction), transform.position.z);
        CoverWithBackground(size);

        fittedAspect = targetCamera.aspect;
        fittedHudFraction = hudFraction;
    }

    private void CoverWithBackground(float size)
    {
        Vector2 spriteSize = background.sprite.bounds.size;
        float viewHeight = size * 2f;
        float viewWidth = viewHeight * targetCamera.aspect;
        float scale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);

        background.transform.position = new Vector3(transform.position.x, transform.position.y, background.transform.position.z);
        background.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
