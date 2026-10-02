using System;
using DG.Tweening;
using UnityEngine;

public static class TntExplosionEffect
{
    private const int SmokeOrder = 6;
    private const int PlumeOrder = 7;
    private const int FlashOrder = 8;
    private const int RayOrder = 9;
    private static readonly int AspectId = Shader.PropertyToID("_Aspect");
    private static readonly System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

    public static void Play(GridGenerator board, Vector3 position, TntVfxSettings settings, float sizeMultiplier = 1f)
    {
        if (settings == null || settings.burstSprite == null || settings.smokeSprite == null
            || settings.raySprite == null || settings.additiveMaterial == null || settings.rayMaterial == null)
        {
            Debug.LogWarning("TNT explosion is missing VFX assets. Assign the burst, smoke, ray sprite and materials on TntPrefab.", board);
            return;
        }

        float cell = Vector3.Distance(board.GetLocalPosition(0, 0), board.GetLocalPosition(1, 0)) * sizeMultiplier;
        PlayFlash(board, position, settings, cell);
        PlayPlume(board, position, settings, cell);
        PlayRays(board, position, settings, cell);
        PlaySmoke(board, position, settings, cell);
    }

    private static void PlayFlash(GridGenerator board, Vector3 origin, TntVfxSettings settings, float cell)
    {
        for (int i = 0; i < 2; i++)
        {
            float size = settings.flashSize * cell * (i == 0 ? 1f : 0.85f);
            SpriteRenderer flash = Create(board, settings.burstSprite, settings.additiveMaterial,
                settings.flashColor, origin, size, FlashOrder);
            Vector3 scale = flash.transform.localScale;
            flash.transform.localScale = scale * 0.22f;

            Animate(flash, settings.flashDuration, progress =>
            {
                float expand = EaseOut(Mathf.Clamp01(progress / 0.2f));
                flash.transform.localScale = scale * Mathf.Lerp(0.22f, 1f, expand);
                flash.color = WithAlpha(settings.flashColor, settings.flashColor.a * Fade(progress, 0.3f));
            });
        }
    }

    private static void PlayPlume(GridGenerator board, Vector3 origin, TntVfxSettings settings, float cell)
    {
        float angleOffset = Range(0f, Mathf.PI * 2f);
        for (int i = 0; i < settings.plumeCount; i++)
        {
            Vector3 direction = Direction(angleOffset + i * Mathf.PI * 2f / settings.plumeCount + Range(-0.3f, 0.3f));
            float distance = settings.plumeRadius * (i < 3 ? Range(0.02f, 0.12f) : Range(0.4f, 1f)) * cell;
            float size = Pick(settings.plumeSize) * cell;
            float alpha = Pick(settings.plumeAlpha);
            Color color = WithAlpha(settings.plumeColor, alpha);
            SpriteRenderer puff = Create(board, settings.smokeSprite, settings.additiveMaterial,
                color, origin + direction * distance * 0.12f, size, PlumeOrder);
            Vector3 scale = puff.transform.localScale;
            puff.transform.localScale = scale * 0.45f;
            float angle = puff.transform.localEulerAngles.z;
            float spin = Range(-35f, 35f);

            Animate(puff, Pick(settings.plumeLifetime), progress =>
            {
                float spread = EaseOut(Mathf.Clamp01(progress / 0.55f));
                puff.transform.localPosition = origin + direction * distance * Mathf.Lerp(0.12f, 1f, spread);
                puff.transform.localScale = scale * Mathf.Lerp(0.45f, 1.2f, EaseOut(progress));
                puff.transform.localEulerAngles = new Vector3(0f, 0f, angle + spin * progress);
                puff.color = WithAlpha(color, alpha * Fade(progress, 0.28f));
            });
        }
    }

    private static void PlayRays(GridGenerator board, Vector3 origin, TntVfxSettings settings, float cell)
    {
        float angleOffset = Range(0f, Mathf.PI * 2f);
        int clusters = Mathf.CeilToInt(settings.rayCount / 3f);
        float clusterAngle = angleOffset;
        for (int i = 0; i < settings.rayCount; i++)
        {
            if (i % 3 == 0)
                clusterAngle = angleOffset + (i / 3) * Mathf.PI * 2f / clusters + Range(-0.3f, 0.3f);
            float angle = clusterAngle + Range(-0.12f, 0.12f);
            Vector3 direction = Direction(angle);
            float length = Pick(settings.rayLength) * cell;
            float width = Pick(settings.rayWidth) * cell;
            SpriteRenderer ray = Create(board, settings.raySprite, settings.rayMaterial,
                settings.rayColor, origin, width, RayOrder);
            Vector2 spriteSize = ray.sprite.bounds.size;
            Vector3 scale = new Vector3(width / spriteSize.x, length / spriteSize.y, 1f);
            ray.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg - 90f);
            ray.transform.localScale = scale * 0.15f;
            var properties = new MaterialPropertyBlock();
            properties.SetFloat(AspectId, length / width);
            ray.SetPropertyBlock(properties);

            Animate(ray, Pick(settings.rayLifetime), progress =>
            {
                float expand = Mathf.Lerp(0.15f, 1f, EaseOut(Mathf.Clamp01(progress / 0.35f)));
                float retract = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(0.45f, 1f, progress));
                float drift = Mathf.Lerp(0.02f, 0.65f, progress) * cell;
                ray.transform.localScale = new Vector3(scale.x, scale.y * retract, scale.z) * expand;
                ray.transform.localPosition = origin + direction * (length * expand * retract * 0.5f + drift);
                properties.SetFloat(AspectId, length * retract / width);
                ray.SetPropertyBlock(properties);
                Color tint = Color.Lerp(settings.rayColor, settings.plumeColor, progress * progress);
                ray.color = WithAlpha(tint, settings.rayColor.a * Fade(progress, 0.35f));
            });
        }
    }

    private static void PlaySmoke(GridGenerator board, Vector3 origin, TntVfxSettings settings, float cell)
    {
        float angleOffset = Range(0f, Mathf.PI * 2f);
        for (int i = 0; i < settings.smokeCount; i++)
        {
            Vector3 direction = Direction(angleOffset + i * Mathf.PI * 2f / settings.smokeCount + Range(-0.3f, 0.3f));
            float distance = Range(1f, 1.45f) * cell;
            Color color = new Color(1f, 0.98f, 0.9f, 0f);
            SpriteRenderer puff = Create(board, settings.smokeSprite, null,
                color, origin + direction * distance * 0.5f, Pick(settings.smokeSize) * cell, SmokeOrder);
            Vector3 scale = puff.transform.localScale;
            puff.transform.localScale = scale * 0.45f;

            Animate(puff, Pick(settings.smokeLifetime), progress =>
            {
                puff.transform.localPosition = origin + direction * distance * Mathf.Lerp(0.5f, 1f, EaseOut(progress));
                puff.transform.localScale = scale * Mathf.Lerp(0.45f, 1.15f, EaseOut(progress));
                float appear = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.15f));
                Color tint = Color.Lerp(color, new Color(0.65f, 0.63f, 0.58f), progress);
                puff.color = WithAlpha(tint, settings.smokeAlpha * appear * Fade(progress, 0.2f));
            });
        }
    }

    private static SpriteRenderer Create(GridGenerator board, Sprite sprite, Material material,
        Color color, Vector3 position, float size, int order)
    {
        float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        SpriteRenderer effect = EffectSprite.Create(board, sprite, color, position, Vector3.one * scale, order);
        if (material != null)
            effect.sharedMaterial = material;
        effect.transform.localRotation = Quaternion.Euler(0f, 0f, Range(0f, 360f));
        return effect;
    }

    private static void Animate(SpriteRenderer effect, float lifetime, Action<float> update)
    {
        DOVirtual.Float(0f, 1f, lifetime, progress => update(progress))
            .SetEase(Ease.Linear)
            .SetLink(effect.gameObject)
            .OnComplete(() => UnityEngine.Object.Destroy(effect.gameObject));
    }

    private static float Fade(float progress, float hold)
    {
        return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(hold, 1f, progress));
    }

    private static float EaseOut(float progress)
    {
        return 1f - (1f - progress) * (1f - progress);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private static Vector3 Direction(float angle)
    {
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
    }

    private static float Pick(Vector2 range) => Range(range.x, range.y);
    private static float Range(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
}
