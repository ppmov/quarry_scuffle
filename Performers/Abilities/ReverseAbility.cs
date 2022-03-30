using UnityEngine;
using Context;

public class ReverseAbility : Ability
{
    public override string Tooltip => "Within " + toBaseRange + " range to nearest castle turns around";
    private const float toBaseRange = 25f;
    private Side direction;

    private GameObject Current => direction == Side.Left ? Instantiator.GetCastle(Side.Left) : Instantiator.GetCastle(Side.Right);

    public override bool CheckAvailability(out Vulnerable target)
    {
        if (!base.CheckAvailability(out target))
            return false;

        if ((Current.transform.position - transform.position).magnitude > toBaseRange)
            return false;

        return true;
    }

    public override bool TryUse()
    {
        if (!base.TryUse())
            return false;

        // turn around
        direction = Sider.Invert(direction);
        AI.WhereToGoWhenNoAbility = Current.transform.position;
        return true;
    }

    protected override void Awake()
    {
        base.Awake();
        IsAnimated = false;
        CanMoveWhenCocked = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        direction = Sider.Invert(AI.Side); // initial direction
    }
}
