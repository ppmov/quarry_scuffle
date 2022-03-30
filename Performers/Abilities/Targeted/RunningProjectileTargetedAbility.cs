using UnityEngine;
using Context;
using static Players;

public class RunningProjectileTargetedAbility : ProjectileTargetedAbility
{
    public override string Tooltip => base.Tooltip + " and keeps moving";

    [SerializeField]
    private float timeToMotion;
    private Vector3 moveVector;

    protected override void Awake()
    {
        base.Awake();
        CanMoveWhenCocked = true;
    }

    protected override Vulnerable SelectRequiredTarget()
    {
        Vulnerable target = AI.SelectTarget(Sider.Count(GetPlayer(AI.Owner).Side, Filter), Range.Value, AffectsOnly, 1);

        if (target == null)
            return null;

        // can't select target with big rotate angle
        target.DoesItReach(AI.Position, Range.Value, out Vector3 hitVector, false);

        if (Vector3.Angle(AI.transform.forward, hitVector) < 60f)
            return target;
        else
            return null;
    }

    protected override bool CanBeCocked()
    {
        if (AI == null || AI.Target == null)
            return false;

        if (AI.CurrentAbilityID != Id)
            return false;

        // determinate movespeed
        float v = AI.MoveSpeed;
        float angle = Vector3.Angle(AI.transform.forward, AI.Target.transform.forward);

        if (angle < 30f)
            v -= AI.Target.MoveSpeed;
        else
        if (angle > 150f)
            v += AI.Target.MoveSpeed;

        // true if close enough for animation start
        float t = (AI.Target.HitPosition - AI.Vulnerable.HitPosition).magnitude / v;
        return 0f < t && t <= timeToMotion;
    }

    protected override Vector3 GetMoveVector()
    {
        // get left move vector
        AI.CheckWayToTargetIsFree(AI.Target, Range.Value, out moveVector, 1);
        return moveVector;
    }

    protected override Vector3 GetLookVector()
    {
        return AI.WhereToGoWhenNoAbility - AI.Position;
    }
}