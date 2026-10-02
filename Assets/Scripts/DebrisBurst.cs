using DG.Tweening;
using UnityEngine;

public static class DebrisBurst
{
    private readonly struct PieceTier
    {
        public readonly int MinCount;
        public readonly int MaxCount;
        public readonly float MinSize;
        public readonly float MaxSize;
        public readonly float MinSpeed;
        public readonly float MaxSpeed;

        public PieceTier(int minCount, int maxCount, float minSize, float maxSize, float minSpeed, float maxSpeed)
        {
            MinCount = minCount;
            MaxCount = maxCount;
            MinSize = minSize;
            MaxSize = maxSize;
            MinSpeed = minSpeed;
            MaxSpeed = maxSpeed;
        }
    }

    private static readonly PieceTier Large = new PieceTier(1, 2, 0.28f, 0.35f, 3.5f, 5f);
    private static readonly PieceTier Medium = new PieceTier(2, 3, 0.18f, 0.26f, 4f, 6f);
    private static readonly PieceTier Tiny = new PieceTier(2, 3, 0.10f, 0.14f, 5f, 7f);

    private const float Gravity = 18f;
    private const float Lifetime = 0.55f;
    private const float StartSpread = 0.2f;
    private const float MinLaunchAngle = 15f;
    private const float MaxLaunchAngle = 165f;
    private const float MinSpin = 270f;
    private const float MaxSpin = 720f;

    public static void Play(GridGenerator board, Vector3 localPosition, float itemSize, params Sprite[] sprites)
    {
        SpawnTier(board, localPosition, itemSize, sprites, Large);
        SpawnTier(board, localPosition, itemSize, sprites, Medium);
        SpawnTier(board, localPosition, itemSize, sprites, Tiny);
    }

    private static void SpawnTier(GridGenerator board, Vector3 localPosition, float itemSize, Sprite[] sprites, PieceTier tier)
    {
        int count = Random.Range(tier.MinCount, tier.MaxCount + 1);

        for (int i = 0; i < count; i++)
        {
            Sprite sprite = sprites[Random.Range(0, sprites.Length)];
            float size = itemSize * Random.Range(tier.MinSize, tier.MaxSize);
            float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            Vector3 start = localPosition + (Vector3)(Random.insideUnitCircle * StartSpread);

            Transform piece = EffectSprite.Create(board, sprite, Color.white, start, Vector3.one * scale).transform;
            piece.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

            float angle = Random.Range(MinLaunchAngle, MaxLaunchAngle) * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(tier.MinSpeed, tier.MaxSpeed);
            float spin = Random.Range(MinSpin, MaxSpin) * (Random.value < 0.5f ? -1f : 1f);

            Launch(piece, velocity, spin, Lifetime * Random.Range(0.85f, 1.15f));
        }
    }

    private static void Launch(Transform piece, Vector2 velocity, float spin, float duration)
    {
        Vector3 start = piece.localPosition;
        Vector3 startScale = piece.localScale;
        float startAngle = piece.localEulerAngles.z;

        DOVirtual.Float(0f, duration, duration, time =>
            {
                float progress = time / duration;
                piece.localPosition = start + new Vector3(velocity.x * time, velocity.y * time - Gravity * time * time / 2f, 0f);
                piece.localEulerAngles = new Vector3(0f, 0f, startAngle + spin * time);
                piece.localScale = startScale * (1f - progress * progress * progress);
            })
            .SetEase(Ease.Linear)
            .SetLink(piece.gameObject)
            .OnComplete(() => Object.Destroy(piece.gameObject));
    }
}
