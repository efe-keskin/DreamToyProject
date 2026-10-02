using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FailPopup : MonoBehaviour
{
    [SerializeField] private RectTransform ribbon;
    [SerializeField] private Image dimmer;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button closeButton;

    private Sequence showSequence;
    private Vector2 ribbonPosition;
    private Vector2 panelPosition;
    private Action onTryAgain;
    private Action onClose;
    private bool canChoose;

    void Awake()
    {
        ribbonPosition = ribbon.anchoredPosition;
        panelPosition = panel.anchoredPosition;
        tryAgainButton.onClick.AddListener(TryAgain);
        closeButton.onClick.AddListener(Close);
    }

    void OnDisable()
    {
        showSequence?.Kill();
        showSequence = null;
        canChoose = false;
        onTryAgain = null;
        onClose = null;
    }

    void OnDestroy()
    {
        if (tryAgainButton != null)
            tryAgainButton.onClick.RemoveListener(TryAgain);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public void Show(Action onTryAgain, Action onClose)
    {
        showSequence?.Kill();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        this.onTryAgain = onTryAgain;
        this.onClose = onClose;
        canChoose = false;
        tryAgainButton.interactable = false;
        closeButton.interactable = false;

        float halfHeight = ((RectTransform)transform).rect.height / 2f;
        float ribbonOffscreen = halfHeight + ribbon.rect.height / 2f + 40f;
        float panelOffscreen = halfHeight + panel.rect.height / 2f + 40f;
        ribbon.anchoredPosition = ribbonPosition + Vector2.up * ribbonOffscreen;
        ribbon.localScale = Vector3.one;
        ribbon.gameObject.SetActive(true);
        panel.anchoredPosition = panelPosition + Vector2.up * panelOffscreen;
        panel.localScale = Vector3.one;
        panel.gameObject.SetActive(false);
        dimmer.color = new Color(0f, 0f, 0f, 0f);

        showSequence = DOTween.Sequence()
            .Append(ribbon.DOAnchorPos(ribbonPosition, 0.4f).SetEase(Ease.OutBack, 0.7f))
            .AppendInterval(1.1f)
            .Append(ribbon.DOAnchorPosY(ribbonPosition.y + 24f, 0.08f).SetEase(Ease.OutQuad))
            .Append(ribbon.DOAnchorPosY(-ribbonOffscreen, 0.25f).SetEase(Ease.InQuad))
            .AppendCallback(() => ribbon.gameObject.SetActive(false))
            .AppendInterval(0.05f)
            .AppendCallback(() => panel.gameObject.SetActive(true))
            .Append(panel.DOAnchorPos(panelPosition, 0.35f).SetEase(Ease.OutBack, 0.7f))
            .Join(dimmer.DOFade(0.6f, 0.2f))
            .OnComplete(() =>
            {
                canChoose = true;
                tryAgainButton.interactable = true;
                closeButton.interactable = true;
            })
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void TryAgain()
    {
        Choose(onTryAgain);
    }

    private void Close()
    {
        Choose(onClose);
    }

    private void Choose(Action action)
    {
        if (!canChoose)
            return;

        canChoose = false;
        tryAgainButton.interactable = false;
        closeButton.interactable = false;
        onTryAgain = null;
        onClose = null;
        action?.Invoke();
    }
}
