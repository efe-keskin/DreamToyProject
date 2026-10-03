using DG.Tweening;
using UnityEngine;

public enum ChaliceBoxPhase
{
    Door,
    Chalice
}

[RequireComponent(typeof(SpriteRenderer))]
public class ChaliceBox : Obstacle
{
    public const int ChaliceGoal = 10;

    [SerializeField] private int doorHealth = 1;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite chaliceSprite;
    [SerializeField] private Sprite[] doorFragments;
    [SerializeField] private Sprite[] baseFragments;
    [SerializeField] private Sprite dustSprite;
    [SerializeField] private Sprite sparkleSprite;
    [SerializeField] private Material sparkleMaterial;

    private readonly SpriteRenderer[] chalices = new SpriteRenderer[ChaliceGoal];

    public int ChalicesCollected { get; private set; }

    public ChaliceBoxPhase Phase => Health > 0 ? ChaliceBoxPhase.Door : ChaliceBoxPhase.Chalice;

    public override bool CanFall => false;
    public override Vector2Int Size => new Vector2Int(2, 2);
    public override bool IsCleared => ChalicesCollected >= ChaliceGoal;
    public override GoalType GoalType => GoalType.Chalice;
    public override int RemainingGoalCount => ChaliceGoal - ChalicesCollected;

    protected override int MaxHealth => doorHealth;

    protected override void OnBlast(DamageTransaction transaction, int adjacentCubeCount)
    {
        if (Phase == ChaliceBoxPhase.Door)
            TakeDamageOnce(transaction);
        else if (!transaction.HasDamaged(this))
            CollectChalices(adjacentCubeCount);
    }

    protected override void OnSpecialHit(DamageTransaction transaction, Vector2Int cell)
    {
        if (Phase == ChaliceBoxPhase.Door)
            TakeDamageOnce(transaction);
        else if (!transaction.HasDamaged(this) && transaction.TryMarkCell(cell))
            CollectChalices(1);
    }

    protected override void OnDamaged()
    {
        if (Phase == ChaliceBoxPhase.Chalice)
        {
            GetComponent<SpriteRenderer>().sprite = openSprite;
            ShowChalices();
            ChaliceBoxEffect.BreakDoors(Board, transform.localPosition, doorFragments, dustSprite);
        }

        Punch();
    }

    private void CollectChalices(int amount)
    {
        int previousRemaining = RemainingGoalCount;
        ChalicesCollected = Mathf.Min(ChalicesCollected + amount, ChaliceGoal);
        int collected = previousRemaining - RemainingGoalCount;

        for (int i = 0; i < collected; i++)
        {
            int slot = previousRemaining - 1 - i;
            SpriteRenderer chalice = chalices[slot];
            if (chalice == null)
                continue;

            Vector3 position = Board.transform.InverseTransformPoint(chalice.transform.position);
            float height = Board.transform.InverseTransformVector(
                chalice.transform.TransformVector(Vector3.up * chalice.sprite.bounds.size.y)).magnitude;
            chalice.enabled = false;
            ChaliceBoxEffect.Collect(Board, position, chaliceSprite, height, sparkleSprite, sparkleMaterial, i * 0.035f);
        }

        PlaceChalices();
        Debug.Log($"Chalices collected: {ChalicesCollected}/{ChaliceGoal}");
        Punch();
    }

    public override void PlayBreakEffect()
    {
        ChaliceBoxEffect.BreakCabinet(Board, transform.localPosition, baseFragments, dustSprite, sparkleSprite, sparkleMaterial);
    }

    private void ShowChalices()
    {
        if (chaliceSprite == null)
            return;

        int order = GetComponent<SpriteRenderer>().sortingOrder;
        for (int i = 0; i < ChaliceGoal; i++)
        {
            GameObject item = new GameObject("Chalice");
            item.transform.SetParent(transform, false);
            item.transform.localScale = Vector3.one * (0.29f / chaliceSprite.bounds.size.y);
            SpriteRenderer chalice = item.AddComponent<SpriteRenderer>();
            chalice.sprite = chaliceSprite;
            chalice.sortingOrder = order + 1 + i % 5;
            chalices[i] = chalice;
        }

        PlaceChalices();
    }

    private void PlaceChalices()
    {
        for (int i = 0; i < RemainingGoalCount; i++)
        {
            if (chalices[i] == null)
                continue;

            int row = i / 5;
            int rowCount = Mathf.Min(5, RemainingGoalCount - row * 5);
            float x = rowCount == 1 ? 0f : Mathf.Lerp(-0.26f, 0.26f, (i % 5) / (rowCount - 1f));
            chalices[i].transform.localPosition = new Vector3(x, row == 0 ? 0.21f : -0.215f, 0f);
        }
    }

    private void Punch()
    {
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f).SetLink(gameObject);
    }
}
