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

    private static readonly Color ChalicePhaseColor = new Color(1f, 0.84f, 0f);

    [SerializeField] private int doorHealth = 1;

    public int ChalicesCollected { get; private set; }

    public ChaliceBoxPhase Phase => Health > 0 ? ChaliceBoxPhase.Door : ChaliceBoxPhase.Chalice;

    public override bool CanFall => false;
    public override Vector2Int Size => new Vector2Int(2, 2);
    public override bool IsCleared => ChalicesCollected >= ChaliceGoal;

    protected override int MaxHealth => doorHealth;

    public static bool TryGetTokenOffset(string token, out Vector2Int offset)
    {
        switch (token)
        {
            case "cbBL": offset = new Vector2Int(0, 0); return true;
            case "cbBR": offset = new Vector2Int(1, 0); return true;
            case "cbTL": offset = new Vector2Int(0, 1); return true;
            case "cbTR": offset = new Vector2Int(1, 1); return true;
            default: offset = Vector2Int.zero; return false;
        }
    }

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
            GetComponent<SpriteRenderer>().color = ChalicePhaseColor;

        Punch();
    }

    private void CollectChalices(int amount)
    {
        ChalicesCollected = Mathf.Min(ChalicesCollected + amount, ChaliceGoal);
        Debug.Log($"Chalices collected: {ChalicesCollected}/{ChaliceGoal}");
        Punch();
    }

    private void Punch()
    {
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f).SetLink(gameObject);
    }
}
