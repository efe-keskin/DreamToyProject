using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RocketBeamStrip : MonoBehaviour
{
    private static readonly int EndFadeId = Shader.PropertyToID("_EndFade");

    private SpriteRenderer body;
    private MaterialPropertyBlock properties;
    private Transform head;
    private Vector3 origin;
    private float width;
    private float fadeDuration;
    private bool fading;

    public void Initialize(Transform head, Vector3 forward, float width, float fadeDuration)
    {
        body = GetComponent<SpriteRenderer>();
        properties = new MaterialPropertyBlock();
        this.head = head;
        origin = head.localPosition;
        this.width = width;
        this.fadeDuration = fadeDuration;

        // Keep UV.x across the core and UV.y along its path for every direction.
        float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        Stretch(origin);
    }

    void LateUpdate()
    {
        if (fading)
            return;

        if (head == null)
        {
            FadeOut();
            return;
        }

        Stretch(head.localPosition);
    }

    private void Stretch(Vector3 headPosition)
    {
        float length = Vector3.Distance(origin, headPosition);
        body.enabled = length > 0.0001f;
        if (!body.enabled)
            return;

        Vector2 spriteSize = body.sprite.bounds.size;

        transform.localPosition = (origin + headPosition) / 2f;
        transform.localScale = new Vector3(width / spriteSize.x, length / spriteSize.y, 1f);

        // Keep the soft caps a fixed physical size as the travelled path grows.
        body.GetPropertyBlock(properties);
        properties.SetFloat(EndFadeId, Mathf.Min(0.5f, width * 0.75f / length));
        body.SetPropertyBlock(properties);
    }

    private void FadeOut()
    {
        fading = true;
        body.DOFade(0f, fadeDuration)
            .SetLink(gameObject)
            .OnComplete(() => Destroy(gameObject));
    }
}
