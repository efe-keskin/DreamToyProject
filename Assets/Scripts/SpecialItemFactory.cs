using UnityEngine;

[System.Serializable]
public class SpecialItemFactory
{
    private const int RocketMinGroupSize = 4;
    private const int TntMinGroupSize = 6;

    [SerializeField] private Rocket rocketPrefab;
    [SerializeField] private Tnt tntPrefab;

    public SpecialItem CreateForGroupSize(int groupSize, Transform parent)
    {
        if (groupSize >= TntMinGroupSize)
            return Object.Instantiate(tntPrefab, parent);

        if (groupSize >= RocketMinGroupSize)
        {
            Rocket rocket = Object.Instantiate(rocketPrefab, parent);
            RocketOrientation orientation = Random.value < 0.5f ? RocketOrientation.Horizontal : RocketOrientation.Vertical;
            rocket.SetOrientation(orientation);
            return rocket;
        }

        return null;
    }
}
