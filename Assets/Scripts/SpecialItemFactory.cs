using UnityEngine;

public enum SpecialItemKind
{
    None,
    Rocket,
    Tnt
}

[System.Serializable]
public class SpecialItemFactory
{
    private const int RocketMinGroupSize = 4;
    private const int TntMinGroupSize = 6;

    [SerializeField] private Rocket rocketPrefab;
    [SerializeField] private Tnt tntPrefab;

    public static SpecialItemKind GetKindForGroupSize(int groupSize)
    {
        if (groupSize >= TntMinGroupSize)
            return SpecialItemKind.Tnt;

        if (groupSize >= RocketMinGroupSize)
            return SpecialItemKind.Rocket;

        return SpecialItemKind.None;
    }

    public SpecialItem CreateForGroupSize(int groupSize, Transform parent)
    {
        switch (GetKindForGroupSize(groupSize))
        {
            case SpecialItemKind.Tnt:
                return Object.Instantiate(tntPrefab, parent);

            case SpecialItemKind.Rocket:
                Rocket rocket = Object.Instantiate(rocketPrefab, parent);
                RocketOrientation orientation = Random.value < 0.5f ? RocketOrientation.Horizontal : RocketOrientation.Vertical;
                rocket.SetOrientation(orientation);
                return rocket;

            default:
                return null;
        }
    }
}
