using Photon.Pun;
using UnityEngine;

public class HiringAbility : Ability
{
    public override string Tooltip
    {
        get
        {
            string text = base.Tooltip;

            if (unit == null)
            {
                GameObject go = Resources.Load("Races/" + AI.ID.Race + "/Units/" + AI.ID.Trunc.Replace('b', 'u')) as GameObject;
                
                if (go != null)
                    unit = go.GetComponent<AI>();
            }

            if (unit != null)
                text += " summons " + unit.Tooltip;

            return text;
        }
    }

    private AI unit;                               

    protected override void Awake()
    {
        base.Awake();
        IsAnimated = false;
        CanMoveWhenCocked = false;
    }

    public override bool TryUse()
    {
        if (!base.TryUse())
            return false;

        Instantiator.CreateUnit(Naming.Variety.Unit, AI.ID.Id, AI.ID.Grade, AI.Owner, transform.position + transform.right, transform.rotation);
        return true;
    }
}
