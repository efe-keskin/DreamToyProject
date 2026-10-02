using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SparkleBurst : MonoBehaviour
{
    [SerializeField] private Image sparklePrefab;
    [SerializeField] private int minCount = 12;
    [SerializeField] private int maxCount = 16;
    [SerializeField] private float startRadius = 20f;
    [SerializeField] private float minDistance = 140f;
    [SerializeField] private float maxDistance = 240f;
    [SerializeField] private float minSize = 18f;
    [SerializeField] private float maxSize = 45f;
    [SerializeField] private float minMoveDuration = 0.45f;
    [SerializeField] private float maxMoveDuration = 0.7f;
    [SerializeField] private float appearDuration = 0.2f;
    [SerializeField] private float maxDelay = 0.3f;
    [SerializeField] private float maxSpin = 90f;
    [SerializeField] private float minTwinkleDuration = 0.5f;
    [SerializeField] private float maxTwinkleDuration = 1f;
    [SerializeField] private float minTwinkleAlpha = 0.55f;
    [SerializeField] private float minTwinkleScale = 0.8f;
    [SerializeField] private float maxTwinkleScale = 1.15f;
    [SerializeField] private Color tint = new Color(1f, 0.85f, 0.35f, 1f);

    private readonly List<Image> activeSparkles = new List<Image>();

    void OnDisable()
    {
        Clear();
    }

    public void Play()
    {
        Clear();

        int count = Random.Range(minCount, maxCount + 1);

        for (int i = 0; i < count; i++)
            Spawn();
    }

    public void Clear()
    {
        foreach (Image sparkle in activeSparkles)
        {
            sparkle.DOKill();

            if (sparkle != null)
                Destroy(sparkle.gameObject);
        }

        activeSparkles.Clear();
    }

    private void Spawn()
    {
        Image sparkle = Instantiate(sparklePrefab, transform);
        RectTransform rect = sparkle.rectTransform;
        activeSparkles.Add(sparkle);

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float size = Random.Range(minSize, maxSize);
        float moveDuration = Random.Range(minMoveDuration, maxMoveDuration);

        rect.anchoredPosition = direction * Random.Range(0f, startRadius);
        rect.sizeDelta = new Vector2(size, size);
        rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
        rect.localScale = Vector3.zero;
        sparkle.color = new Color(tint.r, tint.g, tint.b, 0f);

        DOTween.Sequence()
            .SetDelay(Random.Range(0f, maxDelay))
            .Append(rect.DOAnchorPos(direction * Random.Range(minDistance, maxDistance), moveDuration).SetEase(Ease.OutBack))
            .Join(rect.DOLocalRotate(new Vector3(0f, 0f, Random.Range(-maxSpin, maxSpin)), moveDuration, RotateMode.LocalAxisAdd))
            .Join(rect.DOScale(1f, appearDuration).SetEase(Ease.OutBack))
            .Join(sparkle.DOFade(1f, appearDuration))
            .SetTarget(sparkle)
            .OnComplete(() => StartTwinkle(sparkle));
    }

    private void StartTwinkle(Image sparkle)
    {
        RectTransform rect = sparkle.rectTransform;
        float duration = Random.Range(minTwinkleDuration, maxTwinkleDuration);

        sparkle.DOFade(minTwinkleAlpha, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        rect.DOScale(maxTwinkleScale, duration / 2f)
            .SetEase(Ease.OutSine)
            .SetTarget(sparkle)
            .OnComplete(() => rect.DOScale(minTwinkleScale, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(sparkle));
    }
}
