using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class GridItem : MonoBehaviour, IPointerClickHandler
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public virtual bool CanFall => true;
    public virtual Vector2Int Size => Vector2Int.one;

    protected GridGenerator Board => board;

    private GridGenerator board;

    public void AttachToBoard(GridGenerator board, int x, int y)
    {
        this.board = board;
        SetGridPosition(x, y);
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
        name = $"{GetType().Name}_{x}_{y}";
    }

    public Tween MoveTo(Vector3 localPosition, float duration, Ease ease = Ease.InQuad)
    {
        return transform.DOLocalMove(localPosition, duration)
            .SetEase(ease)
            .SetLink(gameObject);
    }

    public Tween FallTo(Vector3 localPosition, float duration, float bounceHeight, float bounceDuration)
    {
        if (bounceHeight <= 0f || bounceDuration <= 0f)
            return MoveTo(localPosition, duration);

        return DOTween.Sequence()
            .Append(transform.DOLocalMove(localPosition, duration).SetEase(Ease.InQuad))
            .Append(transform.DOLocalMoveY(localPosition.y + bounceHeight, bounceDuration * 0.5f).SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMoveY(localPosition.y, bounceDuration * 0.5f).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    public Tween PlayAppear(float duration)
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        return transform.DOScale(targetScale, duration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
    }

    public virtual void PlayBreakEffect()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        board.OnItemClicked(this);
    }
}
