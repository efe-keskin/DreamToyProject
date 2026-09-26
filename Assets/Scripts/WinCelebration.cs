using System;
using DG.Tweening;
using UnityEngine;

public class WinCelebration : MonoBehaviour
{
    [SerializeField] private RectTransform banner;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float holdDuration = 1.5f;

    public void Play(Action onFinished)
    {
        gameObject.SetActive(true);
        particles.Play();

        banner.localScale = Vector3.zero;
        DOTween.Sequence()
            .Append(banner.DOScale(1f, 0.4f).SetEase(Ease.OutBack))
            .Append(banner.DOPunchScale(Vector3.one * 0.15f, 0.4f))
            .AppendInterval(holdDuration)
            .OnComplete(() => onFinished())
            .SetLink(gameObject);
    }
}
