using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Context;

public class Property : IPropertyReader
{
    public enum Type { Health, Armor, Damage, Cooldown, Range, Frequency, Speed }

    public float Initial { get; private set; }

    public float Value
    {
        get
        {
            float v = stableValue;

            foreach (float infl in influences.Values)
                v += infl;

            if (v < minValue)
                v = minValue;
            else
            if (v > maxValue)
                v = maxValue;

            return v;
        }

        set
        {
            if (value < minValue)
                stableValue = minValue;
            else
            if (value > maxValue)
                stableValue = maxValue;
            else
                stableValue = value;
        }
    }

    private float stableValue;
    private readonly float minValue;
    private readonly float maxValue;
    private readonly Dictionary<int, float> influences = new Dictionary<int, float>();

    public Property(float value, float min, float max)
    {
        stableValue = Initial = value;
        minValue = min;
        maxValue = max;
    }

    public IEnumerator ChangeTemporary(float length, float delta)
    {
        for (int i = 0; i <= influences.Count; i++)
            if (!influences.ContainsKey(i))
            {
                influences.Add(i, delta);
                yield return new WaitForSeconds(length);
                influences.Remove(i);
                break;
            }
    }

    public static bool operator >(Property v1, float v2) => v1.Value > v2;
    public static bool operator <(Property v1, float v2) => v1.Value < v2;

    public static Property operator +(Property v1, float v2)
    {
        v1.Value += v2;
        return v1;
    }

    public static Property operator -(Property v1, float v2)
    {
        v1.Value -= v2;
        return v1;
    }
}
