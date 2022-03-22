using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Context;

public class TargetedAbility : Ability
{
    [SerializeField]
    [Range(Options.MeleeRange, 20f)]
    protected float range = Mathf.Infinity;
    private bool isReached = false;

    public override string Tooltip => base.Tooltip + ((Range != null ? Range.Value : range) != Mathf.Infinity ? (" не более " + (Range != null ? Range.Value : range) + " м от цели") : string.Empty);
    public IPropertyReader Range { get => GetProperty(Property.Type.Дальность); }

    protected override void Awake()
    {
        base.Awake();

        if (!properties.ContainsKey(Property.Type.Дальность))
            AddProperty(Property.Type.Дальность, range, Options.MeleeRange, Mathf.Infinity);
    }

    public override bool CheckAvailability(out Vulnerable target)
    {
        if (!base.CheckAvailability(out target))
            return false;

        target = SelectRequiredTarget();
        return target != null;
    }

    public override bool TryUse()
    {
        if (AI.Target == null)
            return false;

        if (!AI.Target.DoesItReach(AI.Position, Range.Value + 0.1f * Range.Value, out _))
            return false;

        if (!base.TryUse())
            return false;

        return true;
    }

    // выбор цели
    protected virtual Vulnerable SelectRequiredTarget() => null;

    protected override bool CanBeCocked()
    {
        if (AI.CurrentAbilityID != Id)
            return false;

        return isReached;
    }

    // вызывается каждый кадр, если цель далеко (необходимо определить вектор движения)
    public void UpdateBeforeReaching(out Vector3 moveVector)
    {
        moveVector = GetMoveVector();
        isReached = false;
    }

    protected virtual Vector3 GetMoveVector()
    {
        AI.CheckWayToTargetIsFree(AI.Target, Range.Value, out Vector3 moveVector);
        return moveVector;
    }

    // вызывается каждый кадр, если цель близко (необходимо определить вектор направления)
    public void UpdateAtReaching(out Vector3 lookVector)
    {
        lookVector = GetLookVector();
        isReached = true;

        if (!IsAnimated)
            TryUse();
    }

    protected virtual Vector3 GetLookVector()
    {
        AI.Target.DoesItReach(AI.Position, 0, out Vector3 lookVector, false);
        return lookVector;
    }
}
