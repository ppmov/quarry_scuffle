using System.Collections.Generic;
using Context;

// Dealing damage counting
public static class Resistances
{
    private static readonly List<List<float>> combinations;

    static Resistances()
    {
        combinations = new List<List<float>>()
        {
                    /* Armor:   natu, ligh, heav, anti, fort  */// Damage:
            new List<float>() { 1.3f,   1f, 0.7f, 1.2f, 0.3f }, // normal
            new List<float>() {   1f, 1.3f, 0.7f, 1.2f, 0.3f }, // pierce
            new List<float>() {   1f,   1f, 1.3f, 0.5f, 0.3f }, // magic
            new List<float>() { 0.7f, 0.7f, 0.7f, 0.7f, 1.3f }  // siege
        };
    }

    public static float CountUp(float damage, DamageType damageType, ArmorType armorType)
    {
        if (combinations.Count <= (int)damageType)
            return damage;

        if (combinations[(int)damageType].Count <= (int)armorType)
            return damage;

        return damage * combinations[(int)damageType][(int)armorType];
    }

    public static List<float> GetArmors(DamageType damageType)
    {
        return combinations[(int)damageType];
    }

    public static List<float> GetDamages(ArmorType armorType)
    {
        List<float> damages = new List<float>();

        for (int i = 0; i < combinations.Count; i++)
            damages.Add(combinations[i][(int)armorType]);

        return damages;
    }
}