using System;
using DG.Tweening;
using UnityEngine;

public static class VaseBreakEffect
{
    private const int DustOrder = 9;
    private const int FragmentOrder = 10;
    private static readonly System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

    public static void Play(GridGenerator board, Vector3 position, Sprite[] fragments, Sprite dustSprite, bool shattered)
    {
        if (fragments == null || fragments.Length == 0 || Array.Exists(fragments, sprite => sprite == null))
        {
            Debug.LogWarning("Vase fragment sprites are missing. Assign the ceramic fragments on VasePrefab.", board);
            return;
        }

        float cell = Vector3.Distance(board.GetLocalPosition(0, 0), board.GetLocalPosition(1, 0));
        int count = shattered ? 10 : 5;

        for (int i = 0; i < count; i++)
        {
            Sprite sprite = fragments[(shattered ? i : i + 1) % fragments.Length];
            float size = shattered
                ? (i < 3 ? Range(0.30f, 0.40f) : i < 7 ? Range(0.16f, 0.27f) : Range(0.07f, 0.13f))
                : (i < 2 ? Range(0.16f, 0.24f) : Range(0.08f, 0.14f));
            float spread = shattered ? 0.22f : 0.1f;
            Vector3 start = position + new Vector3(Range(-spread, spread), Range(-spread, spread), 0f) * cell;
            float side = i % 2 == 0 ? -1f : 1f;
            Vector2 velocity = shattered
                ? new Vector2(side * Range(0.9f, 2.5f), Range(2.8f, 4.1f))
                : new Vector2(side * Range(0.6f, 1.3f), Range(1.9f, 3.1f));
            float lifetime = shattered ? Range(0.7f, 0.9f) : Range(0.5f, 0.65f);
            SpawnFragment(board, sprite, start, size * cell, velocity * cell, 24f * cell, lifetime);
        }

        if (dustSprite != null)
            SpawnDust(board, position, dustSprite, cell, shattered);
    }

    private static void SpawnFragment(GridGenerator board, Sprite sprite, Vector3 start, float size,
        Vector2 velocity, float gravity, float lifetime)
    {
        float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        SpriteRenderer fragment = EffectSprite.Create(board, sprite, Color.white, start, Vector3.one * scale, FragmentOrder);
        Transform piece = fragment.transform;
        float angle = Range(0f, 360f);
        float spin = Range(240f, 600f) * (random.Next(2) == 0 ? -1f : 1f);
        piece.localRotation = Quaternion.Euler(0f, 0f, angle);

        DOVirtual.Float(0f, lifetime, lifetime, time =>
            {
                float progress = time / lifetime;
                float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1f, progress));
                piece.localPosition = start + new Vector3(velocity.x * time, velocity.y * time - gravity * time * time * 0.5f, 0f);
                piece.localRotation = Quaternion.Euler(0f, 0f, angle + spin * time);
                piece.localScale = Vector3.one * (scale * Mathf.Lerp(1f, 0.35f, fade));
                fragment.color = new Color(1f, 1f, 1f, 1f - fade);
            })
            .SetEase(Ease.Linear)
            .SetLink(fragment.gameObject)
            .OnComplete(() => UnityEngine.Object.Destroy(fragment.gameObject));
    }

    private static void SpawnDust(GridGenerator board, Vector3 origin, Sprite sprite, float cell, bool shattered)
    {
        int count = shattered ? 5 : 2;
        float offset = Range(0f, Mathf.PI * 2f);

        for (int i = 0; i < count; i++)
        {
            float angle = offset + i * Mathf.PI * 2f / count + Range(-0.3f, 0.3f);
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            float distance = (shattered ? Range(0.25f, 0.4f) : Range(0.1f, 0.2f)) * cell;
            float size = (shattered ? Range(0.55f, 0.85f) : Range(0.25f, 0.4f)) * cell;
            float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            float alpha = shattered ? Range(0.45f, 0.6f) : Range(0.2f, 0.3f);
            Color tint = new Color(0.96f, 0.93f, 1f, alpha);
            SpriteRenderer puff = EffectSprite.Create(board, sprite, tint, origin + direction * distance * 0.2f,
                Vector3.one * (scale * 0.4f), DustOrder);
            puff.transform.localRotation = Quaternion.Euler(0f, 0f, Range(0f, 360f));

            DOVirtual.Float(0f, 1f, Range(0.25f, 0.35f), progress =>
                {
                    float expand = 1f - (1f - progress) * (1f - progress);
                    float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 1f, progress));
                    puff.transform.localPosition = origin + direction * distance * Mathf.Lerp(0.2f, 1f, expand);
                    puff.transform.localScale = Vector3.one * (scale * Mathf.Lerp(0.4f, 1.2f, expand));
                    puff.color = new Color(tint.r, tint.g, tint.b, alpha * (1f - fade));
                })
                .SetEase(Ease.Linear)
                .SetLink(puff.gameObject)
                .OnComplete(() => UnityEngine.Object.Destroy(puff.gameObject));
        }
    }

    private static float Range(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
}
