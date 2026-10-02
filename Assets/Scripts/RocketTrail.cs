using System;
using DG.Tweening;
using UnityEngine;

public class RocketTrail : MonoBehaviour
{
    private const int SmokeSortingOrder = 7;
    private const int CoreSortingOrder = 8;
    private const int StarSortingOrder = 9;

    // Particle density must not advance the random state used by board refills.
    private readonly System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

    private GridGenerator board;
    private RocketTrailSettings settings;
    private Vector3 forward;
    private Vector3 side;
    private Sprite starSprite;
    private Sprite smokeSprite;
    private Material particleMaterial;
    private float cellSize;
    private Vector3 previousHead;
    private float nextStarDistance;
    private float nextSmokeDistance;

    public void Initialize(GridGenerator board, Vector2Int direction, Rocket source)
    {
        this.board = board;
        settings = source.TrailSettings;
        starSprite = source.StarSprite;
        smokeSprite = source.SmokeSprite;
        particleMaterial = source.ParticleMaterial;
        forward = new Vector3(direction.x, direction.y, 0f);
        side = new Vector3(-direction.y, direction.x, 0f);
        cellSize = board.GetLocalPosition(1, 0).x - board.GetLocalPosition(0, 0).x;
        previousHead = transform.localPosition;

        if (starSprite == null || source.BeamSprite == null || source.BeamMaterial == null || particleMaterial == null
            || (settings.smokeEnabled && smokeSprite == null))
        {
            Debug.LogError("Rocket trail is missing a reference. Assign Star Sprite, Beam Sprite, Beam Material and Particle Material (and Smoke Sprite if smoke is enabled) on the rocket prefab.", this);
            enabled = false;
            return;
        }

        CreateStrip(source, settings.coreColor, settings.coreAlpha, settings.coreWidth, CoreSortingOrder);
    }

    void LateUpdate()
    {
        Vector3 head = transform.localPosition;
        nextStarDistance = EmitAlong(previousHead, head, nextStarDistance, settings.starSpacing, EmitStar);

        if (settings.smokeEnabled)
            nextSmokeDistance = EmitAlong(previousHead, head, nextSmokeDistance, settings.smokeSpacing, EmitSmoke);

        previousHead = head;
    }

    private void CreateStrip(Rocket source, Color color, float alpha, float width, int sortingOrder)
    {
        color.a = alpha;
        SpriteRenderer strip = EffectSprite.Create(board, source.BeamSprite, color, previousHead, Vector3.one, sortingOrder);
        strip.sharedMaterial = source.BeamMaterial;
        strip.gameObject.AddComponent<RocketBeamStrip>().Initialize(transform, forward, width * cellSize, settings.beamFadeDuration);
    }

    private float EmitAlong(Vector3 from, Vector3 to, float nextEmission, Vector2 spacing, Action<Vector3> emit)
    {
        float length = Vector3.Distance(from, to);
        if (length <= 0f)
            return nextEmission;

        while (nextEmission <= length)
        {
            emit(Vector3.Lerp(from, to, nextEmission / length));
            nextEmission += Mathf.Max(Pick(spacing), 0.01f) * cellSize;
        }

        return nextEmission - length;
    }

    private void EmitStar(Vector3 point)
    {
        Vector3 position = point + side * (Range(-settings.starLateralSpread, settings.starLateralSpread) * cellSize);
        Vector3 drift = (-forward * Range(0f, settings.starMaxBackDrift)
            + side * Range(-settings.starMaxSideDrift, settings.starMaxSideDrift)) * cellSize;
        Color tint = random.NextDouble() < settings.starAccentChance ? settings.starAccentColor : settings.starColor;

        Spawn(starSprite, tint, position, Pick(settings.starSize) * cellSize, drift, StarSortingOrder,
            spin: Range(-settings.starMaxSpin, settings.starMaxSpin),
            endScale: settings.starEndScale,
            fadeStart: settings.starFadeStart,
            lifetime: Pick(settings.starLifetime));
    }

    private void EmitSmoke(Vector3 point)
    {
        Vector3 position = point + side * (Range(-settings.smokeLateralJitter, settings.smokeLateralJitter) * cellSize);
        Color tint = settings.smokeColor;
        tint.a = PickVaried(settings.smokeAlpha, settings.smokeAlphaVariation);

        Spawn(smokeSprite, tint, position, PickVaried(settings.smokeSize, settings.smokeSizeVariation) * cellSize, Vector3.zero, SmokeSortingOrder,
            spin: Range(-settings.smokeMaxSpin, settings.smokeMaxSpin),
            endScale: settings.smokeEndScale,
            fadeStart: settings.smokeFadeStart,
            lifetime: Pick(settings.smokeLifetime));
    }

    private void Spawn(Sprite sprite, Color tint, Vector3 position, float size, Vector3 drift, int sortingOrder,
        float spin, float endScale, float fadeStart, float lifetime)
    {
        float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float startAngle = Range(0f, 360f);
        SpriteRenderer particle = EffectSprite.Create(board, sprite, tint, position, Vector3.one * scale, sortingOrder);
        particle.sharedMaterial = particleMaterial;
        Transform particleTransform = particle.transform;
        particleTransform.localEulerAngles = new Vector3(0f, 0f, startAngle);

        // VFX own their lifetime; they are never joined to the gameplay sequence.
        DOVirtual.Float(0f, 1f, lifetime, progress =>
            {
                particleTransform.localPosition = position + drift * progress;
                particleTransform.localEulerAngles = new Vector3(0f, 0f, startAngle + spin * progress);
                particleTransform.localScale = Vector3.one * (scale * Mathf.Lerp(1f, endScale, progress));
                float fade = Mathf.InverseLerp(fadeStart, 1f, progress);
                particle.color = new Color(tint.r, tint.g, tint.b, tint.a * (1f - Mathf.SmoothStep(0f, 1f, fade)));
            })
            .SetEase(Ease.Linear)
            .SetLink(particle.gameObject)
            .OnComplete(() => Destroy(particle.gameObject));
    }

    private float Range(float min, float max)
    {
        return Mathf.Lerp(min, max, (float)random.NextDouble());
    }

    private float Pick(Vector2 range)
    {
        return Range(range.x, range.y);
    }

    private float PickVaried(Vector2 range, float variation)
    {
        // Add per-particle variation while respecting the configured size/alpha limits.
        return Mathf.Clamp(Pick(range) * Range(1f - variation, 1f + variation), range.x, range.y);
    }
}
