using Photon.Pun;
using UnityEngine;
using Context;
using static Players;

public class SummonTargetedAbility : TargetedAbility
{
    public override string Tooltip
    {
        get
        {
            string text = base.Tooltip;

            if (unit == null)
            {
                GameObject go = Resources.Load("Races/" + AI.ID.Race + "/Units/" + AI.ID.Trunc.Replace('u', 's')) as GameObject;

                if (go != null)
                    unit = go.GetComponent<AI>();
            }

            if (unit != null)
                text += " призывает " + unit.Tooltip;

            return text;
        }
    }

    private AI unit;

    protected override void Awake()
    {
        base.Awake();
        IsAnimated = true;
        CanMoveWhenCocked = false;
    }

    public override bool TryUse()
    {
        if (!base.TryUse())
            return false;

        Instantiator.CreateUnit(Naming.Variety.Summoned, AI.ID.Id, AI.ID.Grade, AI.Owner, transform.position + transform.right, transform.rotation);
        return true;
    }

    protected override Vulnerable SelectRequiredTarget()
    {
        return AI.SelectTarget(Sider.Invert(GetPlayer(AI.Owner).Side), Range.Value);
    }
}
