using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Context;

// Object outer contact with other objects
public class Vulnerable : MonoBehaviour
{
    public Naming ID => AI.ID;
    public byte Owner { get => AI.Owner; }
    public Side Side { get => Sider.GetByOwner(Owner); }
    public Vector3 Position { get => AI.Position; }
    public float MoveSpeed { get => AI.MoveSpeed; }
    public GameObject Hitbox { get => hitbox; }
    public Vector3 HitPosition { get => hitbox.transform.position; }
    public PerformerType PerformerType { get => ID.Type == Naming.Variety.Building ? PerformerType.Building : PerformerType.Unit; }
    public ArmorType ArmorType { get => armorType; }
    public List<Aura> Affectables { get => affectables; }
    public string Tooltip => "<color=maroon>" + health + "</color> health and <b>" + armor + "%</b> " + 
                             "<color=silver><i>" + armorType + "</i></color>" + " armor";

    public IPropertyReader Health { get => properties[Property.Type.Health]; }
    public IPropertyReader Armor { get => properties[Property.Type.Armor]; }

    [Header("Properties")]
    [SerializeField]
    private GameObject hitbox;
    [SerializeField]
    [Range(1f, 500f)]
    private float health;
    [SerializeField]
    [Range(0f, 100f)]
    private float armor;
    [SerializeField]
    private ArmorType armorType;

    private AI AI;
    private PhotonView photonView;
    private Collider[] hitboxColliders;

    private readonly List<Aura> affectables = new List<Aura>();
    private readonly Dictionary<Property.Type, Property> properties = new Dictionary<Property.Type, Property>();

    private void Awake()
    {
        if (AI == null)
            AI = GetComponent<AI>();

        if (photonView == null)
            photonView = GetComponent<PhotonView>();

        if (hitboxColliders == null)
            hitboxColliders = hitbox.GetComponents<Collider>();

        if (!properties.ContainsKey(Property.Type.Armor))
            properties.Add(Property.Type.Armor, new Property(armor, 0f, 100f));

        if (!properties.ContainsKey(Property.Type.Health))
            properties.Add(Property.Type.Health, new Property(health, 0f, health));
    }

    // проверка на достижение цели из начальной позиции
    // при closestPoint точка соприкосновения определяется на коллайдере, иначе Hitbox.transform
    public bool DoesItReach(Vector3 initialPos, float distance, out Vector3 hitVector, bool closestPoint = true)
    {
        hitVector = Vector3.zero;
        if (Hitbox == null) return false;
        hitVector = HitPosition - initialPos;

        if (closestPoint)
            if (hitboxColliders.Length > 0)
                foreach (Collider collider in hitboxColliders)
                    if (collider != null)
                    {
                        Vector3 tmpVector = collider.ClosestPointOnBounds(initialPos);
                        tmpVector -= initialPos;

                        if (tmpVector.magnitude < hitVector.magnitude)
                            hitVector = tmpVector;
                    }

        return hitVector.magnitude <= distance;
    }

    // инициализация нанесения урона
    public void InitiateDamage(float damage, DamageType damageType)
    {
        // запускаем только на мастере
        if (!PhotonNetwork.IsMasterClient) return;

        // подсчет уменьшения урона от брони
        damage = Resistances.CountUp(damage, damageType, armorType);
        damage -= damage * Armor.Value / 100f;

        // коммитим полученный урон
        photonView.RPC(nameof(DealDamage), RpcTarget.All, damage);
    }

    // инициация действия ауры
    public void InitiateAura(Aura aura)
    {
        // аура одного типа не может воздействовать дважды
        if (affectables.Exists(x => x.Id == aura.Id))
            return;

        StartCoroutine(AuraHandling(aura));
    }

    [PunRPC]
    private void DealDamage(float damage)
    {
        properties[Property.Type.Health] -= damage;
    }

    private IEnumerator AuraHandling(Aura aura)
    {
        // начинаем
        affectables.Add(aura);

        // поддерживаем
        for (int i = 0; i < aura.TickCount; i++)
        {
            foreach (Aura.Effect effect in aura.Effects)
            {
                // влияем на параметры защиты
                if (properties.TryGetValue(effect.property, out Property property))
                {
                    if (effect.isTemporary)
                        StartCoroutine(property.ChangeTemporary(aura.TickLength, effect.value));
                    else
                        property += effect.value;
                }
                // влияем на параметры способностей
                else
                {
                    foreach (Ability ability in AI.Abilities)
                    {
                        property = ability.GetProperty(effect.property);
                        
                        if (property == null)
                            continue;

                        if (effect.isTemporary)
                            StartCoroutine(property.ChangeTemporary(aura.TickLength, effect.value));
                        else
                            property += effect.value;
                    }
                }
            }

            yield return new WaitForSeconds(aura.TickLength);
        }

        // прекращаем
        affectables.Remove(aura);
    }
}
