using Context;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Aura : IObjectReader
{
    [Serializable]
    public struct Effect
    {
        public Property.Type property;
        public bool isTemporary;
        public float value;

        public Effect(Property.Type property, bool isTemporary, float value)
        {
            this.property = property;
            this.isTemporary = isTemporary;
            this.value = value;
        }
    }

    [SerializeField]
    private string id;
    [SerializeField]
    private string name;
    [SerializeField]
    private string description;

    [Header("Ёффекты")]
    [SerializeField]
    private List<Effect> effects;

    [Header("¬рем€ действи€")]
    [SerializeField]
    private int tick;
    [SerializeField]
    private float duration;

    public string Id { get => id; }
    public List<Effect> Effects { get => effects; }
    public int TickCount { get => tick; }
    public float TickLength { get => duration / tick; }

    public string Name => name;
    public string Description => description;

    public string Tooltip
    {
        get
        {
            string text = string.Empty;

            for (int i = 0; i < effects.Count; i++)
                text += (i == 0 ? " эффект " : " и ") + (effects[i].value > 0 ? "+" : string.Empty) + effects[i].value + " " + effects[i].property;

            return "<b>" + Name + ":</b> накладывает " + (TickCount == 1 ? "на " : TickCount + " раз по ") + TickLength + " сек" + text;
        }
    }
}