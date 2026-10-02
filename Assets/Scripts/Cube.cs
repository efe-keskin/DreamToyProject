using UnityEngine;

public enum CubeType
{
    Red,
    Green,
    Blue,
    Yellow
}

[RequireComponent(typeof(SpriteRenderer))]
public class Cube : GridItem
{
    [System.Serializable]
    private struct CubeSprites
    {
        public CubeType type;
        public Sprite normal;
        public Sprite rocketHint;
        public Sprite tntHint;
        public Sprite particle;
    }

    [SerializeField] private CubeSprites[] spriteSets;

    public CubeType Type { get; private set; }

    private SpriteRenderer body;
    private CubeSprites sprites;

    void Awake()
    {
        body = GetComponent<SpriteRenderer>();
    }

    public void Initialize(CubeType type, int x, int y, GridGenerator board)
    {
        Type = type;
        AttachToBoard(board, x, y);
        sprites = FindSprites(type);
        body.color = Color.white;
        ApplySprite(sprites.normal, "normal");
    }

    public void ShowHint(SpecialItemKind hint)
    {
        switch (hint)
        {
            case SpecialItemKind.Rocket:
                ApplySprite(sprites.rocketHint, "rocket hint");
                break;
            case SpecialItemKind.Tnt:
                ApplySprite(sprites.tntHint, "TNT hint");
                break;
            default:
                ApplySprite(sprites.normal, "normal");
                break;
        }
    }

    public override void PlayBreakEffect()
    {
        if (sprites.particle == null)
        {
            Debug.LogError($"Cube has no particle sprite for {Type}. Add it to the {Type} entry in Sprite Sets on the cube prefab.", this);
            return;
        }

        DebrisBurst.Play(Board, transform.localPosition, body.sprite.bounds.size.x, sprites.particle);
    }

    private CubeSprites FindSprites(CubeType type)
    {
        foreach (CubeSprites set in spriteSets)
        {
            if (set.type == type)
                return set;
        }

        return default;
    }

    private void ApplySprite(Sprite sprite, string state)
    {
        if (sprite == null)
        {
            Debug.LogError($"Cube has no {state} sprite for {Type}. Add a {Type} entry with all three sprites to Sprite Sets on the cube prefab.", this);
            return;
        }

        body.sprite = sprite;
    }
}
