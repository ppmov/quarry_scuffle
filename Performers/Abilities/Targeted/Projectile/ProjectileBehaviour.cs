using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Context;
using System.Collections;

public class ProjectileBehaviour : MonoBehaviour
{
    public Vulnerable Host { get; private set; } = null;
    public Vulnerable Target { get; private set; } = null;
    public TargetDetector Sight { get; private set; } = null;

    private Projectile.Parameters projectile;

    private bool recessive;
    private Vector3 targetLastPosition;
    private Vector3 myLastPosition;
    
    public void Throw(Vulnerable host, Vulnerable target, Projectile.Parameters projectile)
    {
        Host = host;
        Target = target;
        this.projectile = projectile;

        Sight = GetComponent<TargetDetector>();
        myLastPosition = transform.position;
        targetLastPosition = transform.position;

        if (projectile.technique == Technique.Melee)
            projectile.speed = 1f;
    }

    // all projectile are self-targeted
    private void FixedUpdate()
    {
        if (recessive) 
            return;

        // move until reaching
        Vector3 hitVector = targetLastPosition - transform.position;
        float reachDistance = (transform.position - myLastPosition).magnitude;

        if (Target == null)
        {
            if (hitVector.magnitude <= reachDistance)
            {
                EndFly();
                return;
            }
        }
        else
        {
            targetLastPosition = Target.transform.position;

            if (Target.DoesItReach(transform.position, reachDistance, out hitVector, false))
            {
                EndFly();
                return;
            }
        }

        myLastPosition = transform.position;
        transform.position += hitVector.normalized * projectile.speed;
    }

    private void EndFly()
    {
        recessive = true;

        if (Target != null)
        {
            Target.InitiateDamage(projectile.damage, projectile.damageType);

            foreach (Projectile.Modifier mod in projectile.modifiers)
            {
                if (mod.Fx != null)
                    Instantiate(mod.Fx, transform.position + mod.relativeFxVector, Quaternion.identity);

                if (mod.affectsOnly == PerformerType.Anything ||
                    mod.affectsOnly == Target.PerformerType)
                {
                    switch (mod.type)
                    {
                        case Projectile.Modifier.Type.Blow:
                            Blow(mod);
                            break;
                        case Projectile.Modifier.Type.Bounce:
                            Bounce(mod);
                            break;
                        case Projectile.Modifier.Type.Vampire:
                            Vampire(mod);
                            break;
                    }
                }
            }
        }

        StartCoroutine(SelfDestroyer());
    }

    private void Blow(Projectile.Modifier mod)
    {
        if (Sight == null || Target == null) 
            return;

        List<Vulnerable> targets = Sight.SelectAll(Target.Side, projectile.affectsOnly);

        foreach (Vulnerable unit in targets)
            unit.InitiateDamage(mod.damage, mod.damageType);
    }

    private void Bounce(Projectile.Modifier mod)
    {
        if (Sight == null || Target == null) 
            return;

        if (mod.damage < 0.15f)
            return;

        Projectile.Parameters bouncerParams = projectile;
        bouncerParams.spawnPosition = transform.position - projectile.shell.transform.position;
        bouncerParams.damage = mod.damage;
        bouncerParams.damageType = mod.damageType;
        bouncerParams.technique = mod.technique;
        
        // create new list to unchain parent changes
        bouncerParams.modifiers = new List<Projectile.Modifier>(projectile.modifiers.Count);
        mod.damage /= 2f;

        for (int i = 0; i < projectile.modifiers.Count; i++)
        {
            if (projectile.modifiers[i].type == Projectile.Modifier.Type.Bounce)
                bouncerParams.modifiers.Add(mod);
            else
                bouncerParams.modifiers.Add(projectile.modifiers[i]);
        }

        Projectile bouncer = new Projectile(bouncerParams);
        bouncer.Throw(Target, Sight.SelectAny(Target.Side, projectile.affectsOnly, Target));
    }

    private void Vampire(Projectile.Modifier mod)
    {
        if (Host == null) 
            return;

        Projectile.Parameters vampParams = projectile;
        // push back
        vampParams.spawnPosition = transform.position - Host.HitPosition;
        vampParams.damage = mod.damage;
        vampParams.damageType = mod.damageType;
        vampParams.technique = mod.technique;
        vampParams.affectsOnly = Host.PerformerType;
        vampParams.modifiers = new List<Projectile.Modifier>(0);

        Projectile vamp = new Projectile(vampParams);
        vamp.Throw(Target, Host);
    }

    private IEnumerator SelfDestroyer()
    {
        TryGetComponent(out MeshRenderer mesh);
        if (mesh != null)
            mesh.enabled = false;

        Behaviour halo = (Behaviour)GetComponent("Halo");
        if (halo != null)
            halo.enabled = false;

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
