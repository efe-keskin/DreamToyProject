using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Vase : Obstacle
{
    protected override int MaxHealth => 2;

    protected override void OnDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color *= new Color(0.6f, 0.6f, 0.6f, 1f);
        transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetLink(gameObject);
    }
}
