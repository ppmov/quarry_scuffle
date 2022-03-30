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

    [Header("Effects")]
    [SerializeField]
    private List<Effect> effects;

    [Header("Duration time")]
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
                text += (i == 0 ? " effect of " : " and") + (effects[i].value > 0 ? " + " : " ") + effects[i].value + " " + effects[i].property;

            return "<b>" + Name + ":</b> applies " + (TickCount == 1 ? "on " : TickCount + " times for ") + TickLength + " sec" + text;
        }
    }
}