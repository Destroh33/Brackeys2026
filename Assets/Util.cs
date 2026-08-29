using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static bool Contains(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }

    public static Vector3 GetRandomDirectionInCone(float maxAngleRad)
    {
        var point = UnityEngine.Random.insideUnitCircle * Mathf.Tan(maxAngleRad);
        return new Vector3(point.x, point.y, 1.0f).normalized;
    }

    public static T Sample<T>(this IReadOnlyList<T> values)
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("Tried to sample on empty list");
        }
        return values[UnityEngine.Random.Range(0, values.Count)];
    }

    public static List<T> Sample<T>(this IReadOnlyList<T> values, int n)
    {
        List<T> sampled = new(n);
        for (int i = 0; i < n; ++i)
        {
            sampled.Add(values.Sample());
        }
        return sampled;
    }

    public static List<T> SampleUnique<T>(this IReadOnlyList<T> values, int n)
    {
        if (n > values.Count)
        {
            throw new InvalidOperationException($"Tried to sample {n} unique elements when only {values.Count} in list.");
        }
        List<T> sampled = new(n);
        for (int i = 0; i < values.Count; ++i)
        {
            int sampledRemaining = n - sampled.Count;
            if (sampledRemaining == 0)
            {
                break;
            }
            int valuesRemaining = values.Count - i;
            float probability = (float) sampledRemaining / valuesRemaining;
            if (UnityEngine.Random.value <= probability)
            {
                sampled.Add(values[i]);
            }
        }
        return sampled;
    }

    public static void Shuffle<T>(this IList<T> values)
    {
        for (int i = values.Count - 1; i > 0; --i)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    public static void Shuffle<T>(this IList<T> values, System.Random random)
    {
        for (int i = values.Count - 1; i > 0; --i)
        {
            int j = random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    public static int CombineSeed(int seed, int index)
    {
        unchecked
        {
            int hash = seed * 73856093;
            hash ^= (index + 1) * 19349663;
            hash ^= hash >> 13;
            return hash * 83492791;
        }
    }
}
