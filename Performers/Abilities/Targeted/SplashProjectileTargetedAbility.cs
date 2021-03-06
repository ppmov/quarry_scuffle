using System.Collections.Generic;
using UnityEngine;
using Context;
using static Players;

public class SplashProjectileTargetedAbility : ProjectileTargetedAbility
{
    public override string Tooltip => base.Tooltip + " and also affects several targets";

    protected override void Throw()
    {
        projectile.ThrowSplash(AI.Vulnerable, AI.Target, AI.Sight.SelectAll(Sider.Count(GetPlayer(AI.Owner).Side, Filter), AffectsOnly, Range.Value, AI.Target.Position));
    }
}
