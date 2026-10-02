using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WinCelebration : MonoBehaviour
{
    [SerializeField] private RectTransform banner;
    [SerializeField] private SparkleBurst sparkles;
    [SerializeField] private Button continueButton;
    [SerializeField] private RectTransform tapToContinue;

    private Action onContinue;
    private bool continued;

    void Awake()
    {
        continueButton.onClick.AddListener(Continue);
    }

    void OnDestroy()
    {
        continueButton.onClick.RemoveListener(Continue);
    }

    public void Play(Action onContinue)
    {
        this.onContinue = onContinue;
        continued = false;

        gameObject.SetActive(true);
        continueButton.interactable = false;
        tapToContinue.gameObject.SetActive(false);
        sparkles.Play();

        banner.localScale = Vector3.zero;
        DOTween.Sequence()
            .Append(banner.DOScale(1f, 0.4f).SetEase(Ease.OutBack))
            .Append(banner.DOPunchScale(Vector3.one * 0.15f, 0.4f))
            .OnComplete(ShowContinuePrompt)
            .SetLink(gameObject);
    }

    private void ShowContinuePrompt()
    {
        tapToContinue.gameObject.SetActive(true);
        tapToContinue.localScale = Vector3.one;
        tapToContinue.DOScale(1.08f, 0.6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);

        continueButton.interactable = true;
    }

    private void Continue()
    {
        if (continued)
            return;

        continued = true;
        continueButton.interactable = false;
        tapToContinue.DOKill();
        onContinue();
    }
}
