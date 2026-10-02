using System;
using DG.Tweening;
using UnityEngine;

public static class ChaliceBoxEffect
{
    private static readonly System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

    public static void BreakDoors(GridGenerator board, Vector3 position, Sprite[] fragments, Sprite dustSprite)
    {
        Break(board, position, fragments, dustSprite, false);
    }

    public static void BreakCabinet(GridGenerator board, Vector3 position, Sprite[] fragments, Sprite dustSprite,
        Sprite sparkleSprite, Material sparkleMaterial)
    {
        Break(board, position, fragments, dustSprite, true);
        if (sparkleSprite != null && sparkleMaterial != null)
        {
            float cell = CellSize(board);
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = new Vector3(Range(-0.6f, 0.6f), Range(-0.6f, 0.6f), 0f) * cell;
                Sparkle(board, position + offset, sparkleSprite, sparkleMaterial, cell);
            }
        }
    }

    public static void Collect(GridGenerator board, Vector3 position, Sprite sprite, float startHeight,
        Sprite sparkleSprite, Material sparkleMaterial, float direction, float delay)
    {
        if (sprite == null)
            return;

        float cell = CellSize(board);
        Vector2 velocity = new Vector2(direction * Range(1.8f, 3.8f), Range(9f, 10.5f)) * cell;
        float gravity = 32f * cell;
        float lifetime = Range(0.85f, 1f);
        float startScale = startHeight / sprite.bounds.size.y;
        float scale = 0.7f * cell / sprite.bounds.size.y;
        float spin = -direction * Range(280f, 420f);
        float nextSparkle = 0f;
        SpriteRenderer cup = EffectSprite.Create(board, sprite, Color.white, position, Vector3.one * startScale, 12);

        DOVirtual.Float(0f, lifetime, lifetime, time =>
            {
                float progress = time / lifetime;
                float grow = 1f - Mathf.Pow(1f - Mathf.Clamp01(time / 0.1f), 2f);
                float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.75f, 1f, progress));
                cup.transform.localPosition = FlightPosition(position, velocity, gravity, time);
                cup.transform.localRotation = Quaternion.Euler(0f, 0f, spin * time);
                cup.transform.localScale = Vector3.one * (Mathf.Lerp(startScale, scale, grow) * Mathf.Lerp(1f, 0.6f, fade));
                cup.color = new Color(1f, 1f, 1f, 1f - fade);

                if (sparkleSprite != null && sparkleMaterial != null)
                {
                    while (nextSparkle <= time && nextSparkle < 0.28f)
                    {
                        Sparkle(board, FlightPosition(position, velocity, gravity, nextSparkle), sparkleSprite, sparkleMaterial, cell);
                        nextSparkle += 0.035f;
                    }
                }
            })
            .SetDelay(delay)
            .SetEase(Ease.Linear)
            .SetLink(cup.gameObject)
            .OnComplete(() => UnityEngine.Object.Destroy(cup.gameObject));
    }

    private static void Break(GridGenerator board, Vector3 origin, Sprite[] fragments, Sprite dustSprite, bool cabinet)
    {
        if (fragments == null || fragments.Length == 0 || Array.Exists(fragments, sprite => sprite == null))
        {
            Debug.LogWarning("Chalice box fragments are missing. Assign the door and base fragments on ChaliceBoxPrefab.", board);
            return;
        }

        float cell = CellSize(board);
        int smallCount = cabinet ? 12 : 6;
        for (int i = 0; i < fragments.Length + smallCount; i++)
        {
            Sprite sprite = fragments[i % fragments.Length];
            bool small = i >= fragments.Length;
            float size = small ? Range(0.12f, 0.25f)
                : cabinet ? (i == 0 ? 1.2f : Range(0.55f, 0.85f))
                : i == 9 ? 0.78f : (i >= 3 && i <= 6 ? Range(0.6f, 0.85f) : Range(0.2f, 0.4f));
            Vector3 offset = new Vector3(Range(-0.75f, 0.75f), Range(-0.65f, 0.65f), 0f);
            if (!cabinet && i == 9)
                offset = Vector3.zero;
            float side = offset.x == 0f ? (random.Next(2) == 0 ? -1f : 1f) : Mathf.Sign(offset.x);
            Vector2 velocity = new Vector2(side * Range(1.2f, 2.8f), Range(2.2f, 4f)) * cell;
            Fragment(board, sprite, origin + offset * cell, size * cell, velocity, 24f * cell);
        }

        if (dustSprite != null)
            Dust(board, origin, dustSprite, cell, cabinet);
    }

    private static void Fragment(GridGenerator board, Sprite sprite, Vector3 origin, float size, Vector2 velocity, float gravity)
    {
        float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float lifetime = Range(0.7f, 0.95f);
        float angle = Range(-30f, 30f);
        float spin = Range(160f, 380f) * (random.Next(2) == 0 ? -1f : 1f);
        SpriteRenderer piece = EffectSprite.Create(board, sprite, Color.white, origin, Vector3.one * scale, 10);
        piece.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        DOVirtual.Float(0f, lifetime, lifetime, time =>
            {
                float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1f, time / lifetime));
                piece.transform.localPosition = FlightPosition(origin, velocity, gravity, time);
                piece.transform.localRotation = Quaternion.Euler(0f, 0f, angle + spin * time);
                piece.transform.localScale = Vector3.one * (scale * Mathf.Lerp(1f, 0.4f, fade));
                piece.color = new Color(1f, 1f, 1f, 1f - fade);
            })
            .SetEase(Ease.Linear)
            .SetLink(piece.gameObject)
            .OnComplete(() => UnityEngine.Object.Destroy(piece.gameObject));
    }

    private static void Dust(GridGenerator board, Vector3 origin, Sprite sprite, float cell, bool cabinet)
    {
        int count = cabinet ? 5 : 3;
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(Range(-0.7f, 0.7f), Range(-0.7f, 0.7f), 0f) * cell;
            float size = Range(0.65f, 1.1f) * cell;
            float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            float alpha = Range(0.2f, 0.35f);
            Color tint = new Color(0.94f, 0.97f, 0.91f, alpha);
            SpriteRenderer puff = EffectSprite.Create(board, sprite, tint, origin + offset * 0.5f,
                Vector3.one * (scale * 0.4f), 8);
            puff.transform.localRotation = Quaternion.Euler(0f, 0f, Range(0f, 360f));

            DOVirtual.Float(0f, 1f, Range(0.18f, 0.28f), progress =>
                {
                    float expand = 1f - (1f - progress) * (1f - progress);
                    float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 1f, progress));
                    puff.transform.localPosition = origin + offset * Mathf.Lerp(0.5f, 1.2f, expand);
                    puff.transform.localScale = Vector3.one * (scale * Mathf.Lerp(0.4f, 1.2f, expand));
                    puff.color = new Color(tint.r, tint.g, tint.b, alpha * (1f - fade));
                })
                .SetEase(Ease.Linear)
                .SetLink(puff.gameObject)
                .OnComplete(() => UnityEngine.Object.Destroy(puff.gameObject));
        }
    }

    private static void Sparkle(GridGenerator board, Vector3 position, Sprite sprite, Material material, float cell)
    {
        float scale = Range(0.07f, 0.12f) * cell / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        Vector3 offset = new Vector3(Range(-0.1f, 0.1f), Range(-0.1f, 0.1f), 0f) * cell;
        SpriteRenderer star = EffectSprite.Create(board, sprite, new Color(1f, 0.99f, 0.9f), position + offset,
            Vector3.one * scale, 11);
        star.sharedMaterial = material;
        star.transform.localRotation = Quaternion.Euler(0f, 0f, Range(0f, 360f));

        DOVirtual.Float(0f, 1f, Range(0.18f, 0.28f), progress =>
            {
                float fade = Mathf.SmoothStep(0f, 1f, progress);
                star.transform.localScale = Vector3.one * (scale * Mathf.Lerp(1f, 0.5f, fade));
                star.color = new Color(1f, 0.99f, 0.9f, 1f - fade);
            })
            .SetEase(Ease.Linear)
            .SetLink(star.gameObject)
            .OnComplete(() => UnityEngine.Object.Destroy(star.gameObject));
    }

    private static Vector3 FlightPosition(Vector3 origin, Vector2 velocity, float gravity, float time)
    {
        return origin + new Vector3(velocity.x * time, velocity.y * time - gravity * time * time * 0.5f, 0f);
    }

    private static float CellSize(GridGenerator board) => Vector3.Distance(board.GetLocalPosition(0, 0), board.GetLocalPosition(1, 0));
    private static float Range(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
}
