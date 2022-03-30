using System.Collections;
using UnityEngine;
using Context;
using static Players;

public class AuraSource : MonoBehaviour, IObjectReader
{
    [SerializeField]
    private Filter filter;
    [SerializeField]
    private PerformerType affectsOnly;
    [SerializeField]
    private Aura aura;

    private Side side;
    private TargetDetector sight;
    private Vulnerable myself;
    private MeshCollider coll;

    private MeshCollider LineOfSight { get => coll == null ? coll = GetComponent<MeshCollider>() : coll; }
    private float AuraRange { get => (LineOfSight.bounds.max - LineOfSight.bounds.center).magnitude; }

    public string Name => aura.Name;
    public string Description => aura.Description;
    public string Tooltip { get => aura.Tooltip + " to <i>" + filter + " " + affectsOnly + "</i>" +
            (AuraRange <= 0 ? string.Empty : " within <i>" + AuraRange + "</i> range"); }

    private void Start()
    {
        sight = GetComponent<TargetDetector>();
        myself = GetComponentInParent<Vulnerable>();
        side = Sider.Count(GetPlayer(myself.Owner).Side, filter);
        StartCoroutine(Search4Targets());
    }

    private IEnumerator Search4Targets()
    {
        for (; ; )
        {
            if (filter != Filter.Own)
                foreach (Vulnerable vul in sight.SelectAll(side, affectsOnly, Mathf.Infinity, new Vector3(), myself))
                    vul.InitiateAura(aura);

            if (filter != Filter.Enemy)
                if (myself != null)
                    if (affectsOnly == myself.PerformerType || affectsOnly == PerformerType.Anything)
                        myself.InitiateAura(aura);

            yield return new WaitForSeconds(1f);
        }
    }
}