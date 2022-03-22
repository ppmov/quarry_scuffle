using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveAbility : Ability
{
    public override string Tooltip => base.Tooltip + " возрождает случайный труп в качестве союзника";
    private GameObject lastCorpse = null;

    protected override void Awake()
    {
        base.Awake();
        IsAnimated = false;
        CanMoveWhenCocked = false;
    }

    public override bool CheckAvailability(out Vulnerable target)
    {
        if (!base.CheckAvailability(out target))
            return false;

        lastCorpse = Instantiator.GetRandomDeadUnit();
        return lastCorpse != null;
    }

    public override bool TryUse()
    {
        if (!base.TryUse())
            return false;

        if (lastCorpse == null)
            return false;

        Naming naming = lastCorpse.name;
        Instantiator.RemovePerformer(naming);

        Instantiator.CreateUnit(naming.Type,
                                naming.Id,
                                naming.Grade,
                                AI.Owner,
                                lastCorpse.transform.position,
                                lastCorpse.transform.rotation,
                                naming.Race);

        Destroy(lastCorpse);
        return true;
    }
}
