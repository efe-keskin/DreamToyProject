using System;
using System.Collections.Generic;
using UnityEngine;

public static class CubeShuffle
{
    private const int ShuffleAttempts = 8;

    // Each result index identifies the original cube to place in the corresponding cell.
    public static int[] Create(IReadOnlyList<CubeType> colors, IReadOnlyList<Vector2Int> cells)
    {
        if (colors.Count != cells.Count)
            throw new ArgumentException("Each cube must have a corresponding cell.", nameof(cells));

        int matchingA = -1;
        int matchingB = -1;
        int adjacentA = -1;
        int adjacentB = -1;

        for (int i = 0; i < colors.Count; i++)
        {
            for (int j = i + 1; j < colors.Count; j++)
            {
                if (matchingA < 0 && colors[i] == colors[j])
                {
                    matchingA = i;
                    matchingB = j;
                }

                if (adjacentA < 0 && AreAdjacent(cells[i], cells[j]))
                {
                    adjacentA = i;
                    adjacentB = j;
                }
            }
        }

        if (matchingA < 0 || adjacentA < 0)
            return null;

        int[] order = new int[colors.Count];
        for (int i = 0; i < order.Length; i++)
            order[i] = i;

        for (int attempt = 0; attempt < ShuffleAttempts; attempt++)
        {
            for (int i = order.Length - 1; i > 0; i--)
                Swap(order, i, UnityEngine.Random.Range(0, i + 1));

            if (HasMatch(order, colors, cells))
                return order;
        }

        // A matching pair and adjacent slots guarantee a valid arrangement, even if random retries miss it.
        Swap(order, adjacentA, Array.IndexOf(order, matchingA));
        Swap(order, adjacentB, Array.IndexOf(order, matchingB));
        return order;
    }

    private static bool HasMatch(int[] order, IReadOnlyList<CubeType> colors, IReadOnlyList<Vector2Int> cells)
    {
        for (int i = 0; i < order.Length; i++)
        {
            for (int j = i + 1; j < order.Length; j++)
            {
                if (colors[order[i]] == colors[order[j]] && AreAdjacent(cells[i], cells[j]))
                    return true;
            }
        }

        return false;
    }

    private static bool AreAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    private static void Swap(int[] order, int a, int b)
    {
        int previous = order[a];
        order[a] = order[b];
        order[b] = previous;
    }
}
