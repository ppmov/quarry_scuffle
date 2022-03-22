using Context;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTargetedAbility : TargetedAbility
{
    [SerializeField]
    private Filter filter;

    [Header("����������� ������")]
    [SerializeField]
    private Projectile.Parameters parameters;
    protected Projectile projectile;

    public override string Tooltip
    {
        get
        {
            string text = base.Tooltip;

            text += " ������� <color=maroon>" + (Damage != null ? Damage.Value : parameters.damage) + "</color> ��. " + 
                    "���� <color=navy><i>" + DamageType + "</i></color> " + 
                    "� <i>" + Filter + " " + AffectsOnly + "</i>";

            foreach (Projectile.Modifier mod in Modifiers)
                text += " + ��� ���� " + "<i>" + mod.type + "</i> �� <color=maroon>" + mod.damage + "</color> ��." + 
                        "���� <color=navy><i>" + mod.damageType + "</i></color>";

            return text;
        }
    }

    public IPropertyReader Damage { get => GetProperty(Property.Type.����); }
    public IPropertyReader Speed { get => GetProperty(Property.Type.��������); }

    public Filter Filter { get => filter; }
    public PerformerType AffectsOnly { get => parameters.affectsOnly; }
    public DamageType DamageType { get => parameters.damageType; }
    public Technique Technique { get => parameters.technique; }
    public List<Projectile.Modifier> Modifiers { get => parameters.modifiers; }

    protected override void Awake()
    {
        base.Awake();
        IsAnimated = AI.ID.Type != Naming.Variety.Building;
        CanMoveWhenCocked = false;

        if (projectile == null)
            projectile = new Projectile(parameters);

        if (!properties.ContainsKey(Property.Type.����))
            AddProperty(Property.Type.����, parameters.damage, -255f, 255f);

        if (!properties.ContainsKey(Property.Type.��������))
            AddProperty(Property.Type.��������, parameters.speed, 0.1f, 1f);
    }

    public override bool TryUse()
    {
        if (!base.TryUse())
            return false;

        parameters.damage = properties[Property.Type.����].Value;
        parameters.speed = properties[Property.Type.��������].Value;

        Throw();
        return true;
    }

    protected virtual void Throw()
    {
        projectile.Throw(AI.Vulnerable, AI.Target);
    }

    protected override Vulnerable SelectRequiredTarget()
    {
        if (Range.Initial == Mathf.Infinity)
            return Instantiator.GetPerformerWithFilter(Sider.Count(AI.Side, filter), projectile.AffectsOnly);
        else
            return AI.SelectTarget(Sider.Count(Players.GetPlayer(AI.Owner).Side, Filter), Range.Value, AffectsOnly);
    }

    protected override Vector3 GetLookVector()
    {
        if (IsAnimated)
            return base.GetLookVector();
        else
            return Vector3.zero;
    }
}
