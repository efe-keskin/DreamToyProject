using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FailPopup : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button closeButton;

    public void Show(Action onTryAgain, Action onClose)
    {
        gameObject.SetActive(true);
        tryAgainButton.onClick.AddListener(() => Choose(onTryAgain));
        closeButton.onClick.AddListener(() => Choose(onClose));

        panel.localScale = Vector3.zero;
        panel.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    private void Choose(Action action)
    {
        tryAgainButton.interactable = false;
        closeButton.interactable = false;
        action();
    }
}
