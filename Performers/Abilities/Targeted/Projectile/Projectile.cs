using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Context;

public class Projectile
{
    [System.Serializable]
    public struct Parameters
    {
        public GameObject shell;
        public Vector3 spawnPosition;
        public float damage;
        public DamageType damageType;
        public float speed;
        public PerformerType affectsOnly;
        public Technique technique;
        public List<Modifier> modifiers;
    }

    [System.Serializable]
    public struct Modifier
    {
        public enum Type { Взрывной, Отскакивающий, Вампиризм }
        public Type type;
        public DamageType damageType;
        public float damage;
        public Technique technique;
        public PerformerType affectsOnly;
        public GameObject Fx;
        public Vector3 relativeFxVector;
    }

    private Parameters parameters;

    public PerformerType AffectsOnly { get => parameters.affectsOnly;}
    public Technique Technique { get => parameters.technique; }

    public Projectile(Parameters parameters)
    {
        this.parameters = parameters;
    }

    public void Throw(Vulnerable host, Vulnerable target)
    {
        if (host == null || target == null)
            return;

        Vector3 shellPos = Technique == Technique.Телекинез
            ? target.HitPosition
            : parameters.shell.transform.position;

        Instantiate(host, target, shellPos + parameters.spawnPosition);
    }

    public void ThrowSplash(Vulnerable host, Vulnerable target, List<Vulnerable> targets)
    {
        if (host == null || target == null)
            return;

        Vector3 shellPos = Technique == Technique.Телекинез
            ? target.HitPosition
            : parameters.shell.transform.position;

        foreach (Vulnerable vul in targets)
        {
            if (Technique == Technique.Телекинез)
                shellPos = vul.HitPosition;

            Instantiate(host, vul, shellPos + parameters.spawnPosition);
        }
    }

    private void Instantiate(Vulnerable host, Vulnerable target, Vector3 position)
    {
        // создадим снаряд и направим его непосредственно к цели
        target.DoesItReach(position, 0, out Vector3 hitVector, false);
        GameObject flyShell = Object.Instantiate(parameters.shell, position, hitVector == Vector3.zero
                                            ? Quaternion.identity
                                            : Quaternion.LookRotation(hitVector.normalized) * Quaternion.Euler(-90f, 0f, 0f));

        // активация снаряда
        flyShell.SetActive(true);

        if (parameters.modifiers.Count > 0)
            flyShell.AddComponent<TargetDetector>();
        else
        {
            Collider collider = flyShell.GetComponent<Collider>(); 

            if (collider != null)
                collider.enabled = false;
        }

        flyShell.AddComponent<ProjectileBehaviour>().Throw(host, target, parameters);
    }
}
