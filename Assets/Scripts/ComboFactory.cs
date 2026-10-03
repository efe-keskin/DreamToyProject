using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ComboFactory
{
    public static Combo Create(List<SpecialItem> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        List<Tnt> tnts = items.OfType<Tnt>().ToList();
        List<Rocket> rockets = items.OfType<Rocket>().ToList();

        if (tnts.Count >= 2)
            return new TntTntCombo(tnts[0].GetComponent<SpriteRenderer>());

        if (tnts.Count == 1 && rockets.Count >= 1)
            return new TntRocketCombo(rockets[0]);

        if (rockets.Count >= 2)
            return new RocketRocketCombo(rockets[0]);

        throw new ArgumentException(
            "A combo requires at least two supported special items: two TNTs, a TNT and a rocket, or two rockets.",
            nameof(items));
    }
}
