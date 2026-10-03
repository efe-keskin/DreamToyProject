using System;
using DG.Tweening;
using UnityEngine;

public static class StoneBreakEffect
{
    private const int ChunkCount = 6;
    private const int ChipCount = 6;
    private static readonly System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

    public static void Play(GridGenerator board, Vector3 position, Sprite[] fragments, Sprite dustSprite)
    {
        if (fragments == null || fragments.Length == 0 || Array.Exists(fragments, sprite => sprite == null))
        {
            Debug.LogWarning("Stone fragment sprites are missing. Assign the rock fragments on StonePrefab.", board);
            return;
        }

        float cell = Vector3.Distance(board.GetLocalPosition(0, 0), board.GetLocalPosition(1, 0));

        for (int i = 0; i < ChunkCount + ChipCount; i++)
        {
            bool chip = i >= ChunkCount;
            float x = chip ? Range(-0.3f, 0.3f) : (i % 3 - 1) * 0.27f + Range(-0.04f, 0.04f);
            float y = chip ? Range(-0.3f, 0.3f) : (i / 3 == 0 ? 0.23f : -0.23f) + Range(-0.04f, 0.04f);
            float side = Mathf.Abs(x) < 0.08f ? (i % 2 == 0 ? -1f : 1f) : Mathf.Sign(x);
            float size = chip ? Range(0.08f, 0.18f) : Range(0.30f, 0.44f);
            float lifetime = chip ? Range(0.75f, 1f) : Range(1.05f, 1.25f);
            Vector2 velocity = new Vector2(side * Range(chip ? 1.4f : 0.9f, chip ? 3.2f : 2.2f),
                Range(chip ? 3.4f : 4.5f, 6f)) * cell;
            Vector3 start = position + new Vector3(x, y, 0f) * cell;
            SpawnFragment(board, fragments[i % fragments.Length], start, size * cell, velocity,
                20f * cell, lifetime, chip);
        }

        if (dustSprite != null)
            SpawnDust(board, position, dustSprite, cell);
    }

    private static void SpawnFragment(GridGenerator board, Sprite sprite, Vector3 start, float size,
        Vector2 velocity, float gravity, float lifetime, bool chip)
    {
        float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        SpriteRenderer fragment = EffectSprite.Create(board, sprite, Color.white, start, Vector3.one * scale, 10);
        Transform piece = fragment.transform;
        float angle = Range(-40f, 40f);
        float spin = Range(chip ? 220f : 100f, chip ? 440f : 260f) * (random.Next(2) == 0 ? -1f : 1f);
        piece.localRotation = Quaternion.Euler(0f, 0f, angle);

        DOVirtual.Float(0f, lifetime, lifetime, time =>
            {
                float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, time / lifetime));
                piece.localPosition = start + new Vector3(velocity.x * time, velocity.y * time - gravity * time * time * 0.5f, 0f);
                piece.localRotation = Quaternion.Euler(0f, 0f, angle + spin * time);
                piece.localScale = Vector3.one * (scale * Mathf.Lerp(1f, chip ? 0.65f : 0.85f, fade));
                fragment.color = new Color(1f, 1f, 1f, 1f - fade);
            })
            .SetEase(Ease.Linear)
            .SetLink(fragment.gameObject)
            .OnComplete(() => UnityEngine.Object.Destroy(fragment.gameObject));
    }

    private static void SpawnDust(GridGenerator board, Vector3 origin, Sprite sprite, float cell)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = new Vector3((i - 1) * 0.2f, Range(-0.15f, 0.15f), 0f) * cell;
            Vector3 drift = new Vector3((i - 1) * 0.18f, Range(0.08f, 0.18f), 0f) * cell;
            float scale = Range(0.48f, 0.68f) * cell / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            Color tint = new Color(0.95f, 0.91f, 0.84f, Range(0.16f, 0.24f));
            SpriteRenderer puff = EffectSprite.Create(board, sprite, tint, origin + offset, Vector3.one * (scale * 0.6f), 9);
            puff.transform.localRotation = Quaternion.Euler(0f, 0f, Range(0f, 360f));

            DOVirtual.Float(0f, 1f, Range(0.22f, 0.3f), progress =>
                {
                    float expand = 1f - (1f - progress) * (1f - progress);
                    float fade = Mathf.SmoothStep(0f, 1f, progress);
                    puff.transform.localPosition = origin + offset + drift * expand;
                    puff.transform.localScale = Vector3.one * (scale * Mathf.Lerp(0.6f, 1.25f, expand));
                    puff.color = new Color(tint.r, tint.g, tint.b, tint.a * (1f - fade));
                })
                .SetEase(Ease.Linear)
                .SetLink(puff.gameObject)
                .OnComplete(() => UnityEngine.Object.Destroy(puff.gameObject));
        }
    }

    private static float Range(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
}
