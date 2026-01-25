using System.Collections.Generic;
using UnityEngine.Splines;
using UnityEngine;

public static class TreeGeneratorExtension
{
    public static int FindIndex(this IReadOnlyList<Spline> splines, Spline spline)
    {
        for (int i = 0; i < splines.Count; i++)
        {
            if (splines[i] == spline)
            {
                return i;
            }
        }

        return -1;
    }
}
