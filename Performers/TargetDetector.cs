using System.Collections.Generic;
using UnityEngine;
using Context;

// Contains all hitboxes to make unit selection
public class TargetDetector : MonoBehaviour
{
    private List<Vulnerable> Objects = new List<Vulnerable>();
    public Vector3 Position { get => transform.position; }

    public Vulnerable FindByName(string name)
    {
        if (name == string.Empty)
            return null;

        FixedUpdate();

        foreach (Vulnerable vul in Objects)
            if (vul.gameObject.name == name)
                return vul;

        return Instantiator.GetPerformer(name);
    }

    public Vulnerable SelectNearest(Side side, PerformerType type, List<Vulnerable> exceptions = null)
    {
        FixedUpdate();
        float nearest = Mathf.Infinity;
        Vulnerable best = null;

        if (exceptions == null)
            exceptions = new List<Vulnerable>();

        for (int i = 0; i < Objects.Count; i++)
            if (!exceptions.Contains(Objects[i]))
                if (Objects[i].Side == side)
                    if (Objects[i].PerformerType == type || type == PerformerType.Anything)
                        if (Objects[i].DoesItReach(Position, nearest, out Vector3 hitVector))
                        {
                            nearest = hitVector.magnitude;
                            best = Objects[i];
                        }

        return best;
    }

    public Vulnerable SelectAny(Side side, PerformerType type, Vulnerable exception = null)
    {
        FixedUpdate();

        for (int i = 0; i < Objects.Count; i++)
            if (Objects[i].Side == side)
                if (Objects[i] != exception)
                    if (Objects[i].PerformerType == type || type == PerformerType.Anything)
                        return Objects[i];

        return null;
    }

    public List<Vulnerable> SelectAll(Side? side = null, PerformerType type = PerformerType.Anything, float range = Mathf.Infinity, Vector3 targetPosition4AngleCount = new Vector3(), Vulnerable exception = null)
    {
        FixedUpdate();
        List<Vulnerable> result = new List<Vulnerable>();

        for (int i = 0; i < Objects.Count; i++)
            if (Objects[i] != exception)
                if (Objects[i].Side == side || side == null)
                    if (Objects[i].PerformerType == type || type == PerformerType.Anything)
                        if (range == Mathf.Infinity || Objects[i].DoesItReach(Position, range, out _))
                            if (targetPosition4AngleCount == Vector3.zero || Vector3.Angle(targetPosition4AngleCount - Position, Objects[i].Position - Position) <= 90)
                                result.Add(Objects[i]);

        return result;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 8) // hitbox 
            return;

        Vulnerable unit = other.gameObject.GetComponentInParent<Vulnerable>();
        
        if (unit == null) 
            return;
        
        if (Objects.Contains(unit)) 
            return;

        Objects.Add(unit);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != 8) // hitbox
            return;
        
        Vulnerable unit = other.gameObject.GetComponentInParent<Vulnerable>();
        
        if (unit == null) 
            return;
        
        Objects.Remove(unit);
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < Objects.Count; i++)
            if (Objects[i] == null)
            {
                Objects.RemoveAt(i);
                i--;
            }
    }
}
