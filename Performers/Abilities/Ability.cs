using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Context;

// Base ability class
public class Ability : MonoBehaviour, IObjectReader
{
    public string Id { get => id; }
    public virtual string Tooltip => 
        "<b>" + Name + ":</b>" + 
        ((Cooldown != null ? Cooldown.Value : cooldown) > 0f 
            ? " every " + (Cooldown != null ? Cooldown.Value : cooldown) + " sec" 
            : string.Empty);

    public string Name { get => fullname; } 
    public string Description { get => description; }
    public bool IsCocked { get => IsCurrent && IsAnimated && !isRefreshing && CanBeCocked(); }
    public bool IsAnimated { get; protected set; }
    public bool CanMoveWhenCocked { get; protected set; }
    public float Wasted { get; protected set; }
    public Property GetProperty(Property.Type type) => properties.TryGetValue(type, out Property value) ? value : null;
    public IPropertyReader Cooldown { get => properties.ContainsKey(Property.Type.Cooldown) ? properties[Property.Type.Cooldown] : null; }

    protected readonly Dictionary<Property.Type, Property> properties = new Dictionary<Property.Type, Property>();
    protected AI AI { get => ai; }
    protected virtual bool CanBeCocked() => true;

    [SerializeField]
    private AI ai;
    [SerializeField]
    private string id;
    [SerializeField]
    private string fullname;
    [SerializeField]
    private string description;
    [SerializeField]
    private int cooldown;
    
    private bool IsCurrent { get => id == AI.CurrentAbilityID; }
    private bool isRefreshing;

    protected virtual void Awake()
    {
        if (!properties.ContainsKey(Property.Type.Cooldown))
            AddProperty(Property.Type.Cooldown, cooldown, 0f, 100f);
    }

    protected virtual void OnEnable()
    {
        StartCoolDown();
    }

    public virtual bool CheckAvailability(out Vulnerable target)
    {
        target = null;

        if (!isActiveAndEnabled)
            return false;

        if (isRefreshing)
            return false;

        return true;
    }

    // main call
    public virtual bool TryUse()
    {
        if (!IsCurrent)
            return false;

        if (isRefreshing)
            return false;

        StartCoolDown();
        return true;
    }

    protected void AddProperty(Property.Type key, float value, float min, float max)
    {
        if (!properties.ContainsKey(key))
            properties.Add(key, new Property(value, min, max));
    }

    private void StartCoolDown()
    {
        isRefreshing = true;
        Wasted = 0;
        StartCoroutine(CoolDown());

        IEnumerator CoolDown()
        {
            while (Cooldown.Value > Wasted)
            {
                yield return new WaitForSeconds(0.2f);
                Wasted += 0.2f;
            }

            isRefreshing = false;
        }
    }
}
