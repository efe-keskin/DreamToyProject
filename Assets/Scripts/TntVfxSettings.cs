using UnityEngine;

[System.Serializable]
public class TntVfxSettings
{
    [Header("Assets")]
    public Sprite burstSprite;
    public Sprite smokeSprite;
    public Sprite raySprite;
    public Material additiveMaterial;
    public Material rayMaterial;

    [Header("Impact Flash")]
    [Tooltip("Size in cells, including the burst texture's transparent border.")]
    public float flashSize = 6f;
    public float flashDuration = 0.36f;
    public Color flashColor = new Color(1f, 0.99f, 0.68f);

    [Header("Yellow Plume")]
    public int plumeCount = 18;
    public float plumeRadius = 0.75f;
    public Vector2 plumeSize = new Vector2(1.5f, 2f);
    public Vector2 plumeLifetime = new Vector2(0.32f, 0.52f);
    public Vector2 plumeAlpha = new Vector2(0.85f, 1f);
    public Color plumeColor = new Color(1f, 0.95f, 0.02f);

    [Header("Radial Streaks")]
    public int rayCount = 15;
    public Vector2 rayLength = new Vector2(0.9f, 1.9f);
    public Vector2 rayWidth = new Vector2(0.05f, 0.095f);
    public Vector2 rayLifetime = new Vector2(0.35f, 0.50f);
    public Color rayColor = new Color(1f, 1f, 0.96f);

    [Header("Outer Wisps")]
    public int smokeCount = 6;
    public Vector2 smokeSize = new Vector2(0.65f, 1.05f);
    public Vector2 smokeLifetime = new Vector2(0.35f, 0.50f);
    [Range(0f, 1f)] public float smokeAlpha = 0.18f;
}
