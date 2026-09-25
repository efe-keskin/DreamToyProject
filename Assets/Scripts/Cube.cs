using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CubeType
{
    Red,
    Green,
    Blue,
    Yellow
}

[RequireComponent(typeof(SpriteRenderer))]
public class Cube : MonoBehaviour, IPointerClickHandler
{
    public CubeType Type { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }

    private GridGenerator board;

    public void Initialize(CubeType type, int x, int y, GridGenerator board)
    {
        Type = type;
        this.board = board;
        SetGridPosition(x, y);
        GetComponent<SpriteRenderer>().color = GetColor(type);
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
        name = $"Cube_{x}_{y}";
    }

    public Tween MoveTo(Vector3 localPosition, float duration)
    {
        return transform.DOLocalMove(localPosition, duration)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        board.OnCubeClicked(this);
    }

    private static Color GetColor(CubeType type)
    {
        switch (type)
        {
            case CubeType.Red: return Color.red;
            case CubeType.Green: return Color.green;
            case CubeType.Blue: return Color.blue;
            case CubeType.Yellow: return Color.yellow;
            default: return Color.white;
        }
    }
}
