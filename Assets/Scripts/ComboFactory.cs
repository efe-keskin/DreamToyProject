using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ComboFactory
{
    public static Combo Create(List<SpecialItem> items)
    {
        List<Tnt> tnts = items.OfType<Tnt>().ToList();
        List<Rocket> rockets = items.OfType<Rocket>().ToList();

        if (tnts.Count >= 2)
            return new TntTntCombo(tnts[0].GetComponent<SpriteRenderer>());

        if (tnts.Count == 1)
            return new TntRocketCombo(rockets[0]);

        return new RocketRocketCombo(rockets[0]);
    }
}
