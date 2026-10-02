using UnityEngine;

[System.Serializable]
public class RocketTrailSettings
{
    [Header("Feathered Core")]
    public float beamFadeDuration = 0.15f;
    public float coreWidth = 0.17f;
    public Color coreColor = new Color(1f, 0.98f, 0.30f);
    [Range(0f, 1f)] public float coreAlpha = 0.75f;

    [Header("Stars")]
    public Vector2 starSpacing = new Vector2(0.045f, 0.065f);
    public Vector2 starSize = new Vector2(0.13f, 0.21f);
    public Vector2 starLifetime = new Vector2(0.40f, 0.50f);
    public Color starColor = new Color(1f, 0.99f, 0.94f);
    public Color starAccentColor = new Color(1f, 254f / 255f, 67f / 255f);
    [Range(0f, 1f)] public float starAccentChance = 0.3f;
    public float starLateralSpread = 0.24f;
    public float starMaxBackDrift = 0.01f;
    public float starMaxSideDrift = 0.02f;
    public float starMaxSpin = 120f;
    public float starEndScale = 0.9f;
    [Range(0f, 0.99f)] public float starFadeStart = 0.75f;

    [Header("Yellow Plume")]
    public bool smokeEnabled = true;
    public Vector2 smokeSpacing = new Vector2(0.05f, 0.07f);
    public Vector2 smokeSize = new Vector2(0.65f, 0.80f);
    public Vector2 smokeLifetime = new Vector2(0.45f, 0.55f);
    public Vector2 smokeAlpha = new Vector2(0.45f, 0.60f);
    public Color smokeColor = new Color(1f, 0.94f, 0.02f);
    public float smokeLateralJitter = 0.09f;
    [Range(0f, 1f)] public float smokeSizeVariation = 0.15f;
    [Range(0f, 1f)] public float smokeAlphaVariation = 0.15f;
    public float smokeMaxSpin = 30f;
    public float smokeEndScale = 1f;
    [Range(0f, 0.99f)] public float smokeFadeStart = 0.65f;
}
