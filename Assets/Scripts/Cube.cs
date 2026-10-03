using UnityEngine;
using UnityEngine.Rendering;

public enum CubeType
{
    Red,
    Green,
    Blue,
    Yellow
}

[RequireComponent(typeof(SpriteRenderer), typeof(SortingGroup))]
public class Cube : GridItem
{
    [System.Serializable]
    private struct CubeSprites
    {
        public CubeType type;
        public Sprite body;
        public Sprite normal;
        public Sprite rocketHint;
        public Sprite tntHint;
        public Sprite particle;
    }

    [SerializeField] private CubeSprites[] spriteSets;
    [SerializeField] private SpriteRenderer symbol;

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
        body.sprite = sprites.body;
        symbol.color = Color.white;
        symbol.sortingLayerID = body.sortingLayerID;
        symbol.sortingOrder = body.sortingOrder + 1;

        if (sprites.body == null)
            Debug.LogError($"Cube has no body sprite for {Type}. Add it to the {Type} entry in Sprite Sets on the cube prefab.", this);

        ShowHint(SpecialItemKind.None);
    }

    public void ShowHint(SpecialItemKind hint)
    {
        switch (hint)
        {
            case SpecialItemKind.Rocket:
                ApplySymbol(sprites.rocketHint, "rocket hint");
                break;
            case SpecialItemKind.Tnt:
                ApplySymbol(sprites.tntHint, "TNT hint");
                break;
            default:
                ApplySymbol(sprites.normal, "normal");
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

    private void ApplySymbol(Sprite sprite, string state)
    {
        symbol.sprite = sprite;
        symbol.enabled = sprite != null;

        if (sprite == null)
            Debug.LogError($"Cube has no {state} symbol for {Type}. Add it to the {Type} entry in Sprite Sets on the cube prefab.", this);
    }
}
