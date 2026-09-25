using UnityEngine;

public enum RocketOrientation
{
    Horizontal,
    Vertical
}

public class Rocket : SpecialItem
{
    public RocketOrientation Orientation { get; private set; }

    public void SetOrientation(RocketOrientation orientation)
    {
        Orientation = orientation;

        Vector3 scale = transform.localScale;
        transform.localScale = orientation == RocketOrientation.Horizontal
            ? new Vector3(scale.x, scale.y * 0.5f, scale.z)
            : new Vector3(scale.x * 0.5f, scale.y, scale.z);
    }
}
