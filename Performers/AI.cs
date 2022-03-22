using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Context;
using static UnityEngine.Mathf;
using static Players;
using static Options;

// Управление объектом - анимация, передвижение, выбор действий
public class AI : MonoBehaviour, IPunInstantiateMagicCallback, IObjectReader
{
    private const int defaultAct = 10;
    private const float decisionRange = 6f;

    [SerializeField]
    private string fullname;
    [SerializeField]
    private string description;
    [SerializeField]
    private TargetDetector sight;
    [SerializeField]
    private GameObject substrate;
    [SerializeField]
    private GameObject abilityContainer;
    [SerializeField]
    private GameObject auraContainer;
    [SerializeField]
    private Material material;

    public string Name { get => fullname; }
    public string Description { get => description; }
    public string Tooltip 
    {
        get
        {
            string text = "<b>" + Name + "</b>\n";

            if (photonView == null)
                Awake();

            if (Vulnerable != null)
                text += Vulnerable.Tooltip + "\n";

            foreach (Ability ability in Abilities)
                text += "- " + ability.Tooltip + "\n";

            foreach (AuraSource aura in Auras)
                text += "- " + aura.Tooltip + "\n";

            return text;
        }
    }

    public Naming ID { get => new Naming(gameObject.name); }
    public Vector3 Position { get => Sight == null ? transform.position : Sight.transform.position; }
    public Side Side { get => Sider.GetByOwner(Owner); }
    public TargetDetector Sight { get => sight; }
    public byte Owner { get; private set; }
    public Vulnerable Vulnerable { get; private set; }
    public Vulnerable Target { get; private set; } = null;
    public List<Ability> Abilities { get; private set; } = new List<Ability>();
    public float MoveSpeed { get => nav == null ? 0f : nav.desiredVelocity.magnitude; }
    public string CurrentAbilityID { get => (act >= defaultAct) ? string.Empty : Ability.Id; }
    public Vector3 WhereToGoWhenNoAbility { get; set; } = Vector3.zero;

    private Ability Ability { get => (act >= defaultAct) ? null : Abilities[act]; }
    private List<AuraSource> Auras { get; set; } = new List<AuraSource>();

    private int act = defaultAct + 1;
    private NavMeshAgent nav;
    private Animator anim;
    private PhotonView photonView;
    private int fixedTick = maxFixedTicks - 1;
    private float endMovingTime4Rotation; // необходимо для плавного поворота

    // инициация объекта после создания
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (info.photonView.InstantiationData == null) return;
        // владелец
        ChangeOwner((byte)info.photonView.InstantiationData[0]);
        int index = (byte)info.photonView.InstantiationData[1];
        // идентификатор
        gameObject.name = gameObject.name.Replace("(Clone)", string.Empty) + '.' + Owner + index;
        Instantiator.AddPerformer(this);

        if (WhereToGoWhenNoAbility == Vector3.zero)
            WhereToGoWhenNoAbility = Instantiator.GetCastle(Sider.Invert(Side)) == null ? Position : Instantiator.GetCastle(Sider.Invert(Side)).transform.position;
    }

    private void SetMaterial()
    {
        if (material == null)
            material = ID.Type == Naming.Variety.Building ? PlayerColors[Owner].building : PlayerColors[Owner].units;
        
        if (GetComponent<MeshRenderer>() != null)
            GetComponent<MeshRenderer>().material = material;

        foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
            mesh.material = material;

        foreach (SkinnedMeshRenderer mesh in GetComponentsInChildren<SkinnedMeshRenderer>())
            mesh.material = material;
    }

    private void Awake()
    {
        if (photonView == null) // без этого объекта AI не работает
            photonView = GetComponent<PhotonView>();

        if (Vulnerable == null)
            Vulnerable = GetComponent<Vulnerable>();
        
        if (anim == null)
            anim = GetComponent<Animator>();
        
        if (nav == null)
            nav = GetComponent<NavMeshAgent>();

        if (abilityContainer != null && Abilities.Count == 0)
        {
            Ability[] tmp = abilityContainer.GetComponents<Ability>();

            for (int i = 0; i < tmp.Length; i++)
                if (tmp[i].enabled)
                    Abilities.Add(tmp[i]);
        }

        if (auraContainer != null)
        {
            AuraSource[] tmp = auraContainer.GetComponents<AuraSource>();

            for (int i = 0; i < tmp.Length; i++)
                if (tmp[i].enabled)
                    Auras.Add(tmp[i]);
        }
    }

    // принятие решений
    private void FixedUpdate()
    {
        // отрабатываем только на мастере
        if (!PhotonNetwork.IsMasterClient) return;
        if (Sight == null) return;

        // отрабатываем только каждый maxFixedTicks вызов
        fixedTick++;

        if (fixedTick < maxFixedTicks) 
            return;
        else
            fixedTick = 0;

        string targetName = string.Empty;
        int nextAct = defaultAct;

        // активируем первую доступную способность
        for (int i = 0; i < Abilities.Count; i++)
            if (Abilities[i].CheckAvailability(out Vulnerable target))
            {
                if (target == null)
                    targetName = string.Empty; // цель не нужна
                else
                    targetName = target.gameObject.name;

                nextAct = i;
                break;
            }

        // не вызываем RPC если ни действие ни цель не изменились
        if (nextAct == act)
        {
            if (Target == null)
            {
                if (targetName == string.Empty)
                    return;
            }
            else
            if (targetName == Target.gameObject.name)
                return;
        }
        
        // сменим действие и цель для всех игроков
        photonView.RPC(nameof(ChangeAct), RpcTarget.All, nextAct, targetName);
    }

    [PunRPC]
    // вызывается из MasterFixedUpdate для смены действия на всех клиентах
    private void ChangeAct(int next, string targetName)
    {
        Target = Sight.FindByName(targetName);
        act = next;
        endMovingTime4Rotation = Time.time;

        // активируем способность без анимации
        if (Ability != null && !Ability.IsAnimated)
            Ability.TryUse();
    }

    // выполнение действий
    private void Update()
    {
        if (Vulnerable != null)
            if (Vulnerable.Health.Value <= 0f)
            {
                Death();
                return;
            }

        // анимация
        UpdateAbilityAnimation();
        // перемешение
        UpdatePositionAndRotation();
    }

    // анимация текущей способности
    private void UpdateAbilityAnimation()
    {
        if (anim == null || !anim.enabled) 
            return;

        if (Ability == null || !Ability.IsAnimated)
        {
            anim.SetBool("ability", false);
            anim.SetInteger("ability_index", defaultAct);
        }
        else
        {
            anim.SetBool("ability", Ability.IsCocked);
            anim.SetInteger("ability_index", act);
        }

        if (nav != null)
        {
            // включаем анимацию передвижения если способность находится в нужном режиме
            bool isMovementAnimatedByAbility = Ability == null ? false : (Ability.CanMoveWhenCocked && Ability.IsCocked);
            anim.SetBool("move", nav.hasPath && !isMovementAnimatedByAbility);
        }
    }

    // перемещение по логике текущей способности
    private void UpdatePositionAndRotation()
    {
        if (act == defaultAct)
            SetMovePoint(WhereToGoWhenNoAbility);

        if (Target == null || Ability == null)
            return;

        if (Ability is TargetedAbility)
        {
            TargetedAbility rAbility = (TargetedAbility)Ability;

            // направляем к необходимой позиции
            if (!Target.DoesItReach(Position, rAbility.Range.Value, out Vector3 hitVector))
            {
                rAbility.UpdateBeforeReaching(out hitVector);
                SetMovePoint(Position + hitVector);
                endMovingTime4Rotation = Time.time;
            }
            // достаточная близость
            else
            {
                rAbility.UpdateAtReaching(out hitVector);

                if (rAbility.CanMoveWhenCocked)
                {
                    SetMovePoint(Position + hitVector);
                }
                else
                {
                    LerpLookRotation(hitVector);
                    Stop();
                }
            }
        }
    }

    // перемещение
    private void SetMovePoint(Vector3 point)
    {
        if (nav == null) 
            return;

        if (point == Vector3.zero) 
            return;

        if (nav.destination == point) 
            return;

        // нельзя двигаться во время способности
        if (anim != null)
            if (Ability == null || !Ability.CanMoveWhenCocked)
                if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Ability"))
                    return;

        if (HasSubstrate)
            Debug.DrawLine(Position, point);
        
        nav.SetDestination(point);
    }

    private void Stop()
    {
        if (nav == null)
            return;

        nav.ResetPath();
    }

    // плавый поворот к цели
    private void LerpLookRotation(Vector3 direction)
    {
        if (direction == Vector3.zero) 
            return;

        Quaternion look = Quaternion.LookRotation(direction.normalized);
        look.eulerAngles.Set(0f, look.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, (Time.time - endMovingTime4Rotation) / 1f);
    }

    // вызывается из анимации для завершения выполнения способности
    private void UseAbilityInTime()
    {
        if (Ability != null)
            Ability.TryUse();
    }

    // выбор цели для атаки
    public Vulnerable SelectTarget(Side side, float range, PerformerType affectsOnly = PerformerType.Нечто, int vectorIndex = -1)
    {
        List<Vulnerable> exceptions = new List<Vulnerable>();

        // отдаем приоритет текущей цели
        if (Target != null)
        {
            bool hasAvailableTarget = false;

            if (Target.Side == side)
                if (Target.PerformerType == affectsOnly || affectsOnly == PerformerType.Нечто)
                    if (CheckWayToTargetIsFree(Target, range, out Vector3 hitVector, vectorIndex))
                    {
                        if (hitVector.magnitude <= decisionRange)
                            return Target; // поздно выбирать другую цель
                        else
                            hasAvailableTarget = true;
                    }

            if (!hasAvailableTarget)
                exceptions.Add(Target);
        }

        while (true)
        {
            Vulnerable target = Sight.SelectNearest(side, affectsOnly, exceptions);

            if (target == null)
                return null;

            if (CheckWayToTargetIsFree(target, range, out _, vectorIndex))
                return target;
            else
                exceptions.Add(target);
        }
    }

    // смена владельца
    public void ChangeOwner(byte owner)
    {
        if (owner < MaxPlayersCount)
            Owner = owner;

        SetMaterial();
    }

    // выделение объекта, отрисовка контура
    public bool HasSubstrate
    {
        get
        {
            if (substrate == null) return false;
            return substrate.activeSelf;
        }

        set
        {
            if (substrate == null) return;
            substrate.SetActive(value);
        }
    }

    // корректировка пути к цели, выбор свободного направления
    public bool CheckWayToTargetIsFree(Vulnerable target, float range, out Vector3 hitVector, int vectorIndex = -1)
    {
        hitVector = Vector3.zero;

        if (target == null) 
            return false;

        // путь свободен если цель уже на достаточной дистанции
        if (target.DoesItReach(Position, range, out hitVector))
            return true;

        // дальний бой игнорирует дальнейшую логику
        if (range > MaxMeleeRange && vectorIndex <= 0)
            return true;

        // поиск точек диагонали квадрата при известных точках другой диагонали
        Vector2 a = new Vector2(Position.x + hitVector.x, Position.z + hitVector.z);
        Vector2 b = new Vector2(target.HitPosition.x, target.HitPosition.z);
        Vector2 c;

        List<Vector3> hitVectors = new List<Vector3>();
        hitVectors.Add(hitVector); // прямой вектор к цели
        int sign = 1;

        // два решения функции
        while (true)
        {
            c = new Vector2(0.5f * (a.x + b.x + sign * Sqrt(3f) * (a.y - b.y)),
                            0.5f * (a.y + b.y + sign * Sqrt(3f) * (b.x - a.x)));

            hitVectors.Add(new Vector3(c.x - Position.x, hitVector.y, c.y - Position.z));

            if (sign == -1)
                break;

            sign *= -1;
        }

        // при необходимости выбора конкретного подступа к цели
        if (vectorIndex > -1)
        {
            hitVector = hitVectors[vectorIndex];
            hitVectors.Clear();
            hitVectors.Add(hitVector);
        }

        // проверим для каждого направления свободен ли путь до цели толщиной с диаметр юнита
        foreach (Vector3 vector in hitVectors)
        {
            RaycastHit[] hits = Physics.SphereCastAll(Position, nav.radius, vector, vector.magnitude, (1 << 3) | (1 << 8)); // стены и хитбоксы
            bool isUnhindered = true;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform == Vulnerable.Hitbox.transform)
                    continue; // сам себе не помеха
                else
                if (hit.collider.transform == target.Hitbox.transform)
                    continue; // противник не помеха
                else
                    isUnhindered = false;
            }

            if (substrate.activeSelf)
                Debug.DrawLine(Position, vector + Position, isUnhindered ? Color.white : Color.red);

            if (isUnhindered)
            {
                // подходящее направление выбрано
                hitVector = vector;
                return true;
            }
        }

        return false;
    }

    private void Death()
    {
        HasSubstrate = false;
        Destroy(nav);
        Destroy(photonView);
        Destroy(GetComponent<PhotonTransformViewClassic>());

        if (Vulnerable != null)
            Destroy(Vulnerable);

        if (Sight != null)
            Destroy(Sight.gameObject);

        foreach (Ability spell in Abilities)
            Destroy(spell);

        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponentInChildren<Healthbar>());

        if (gameObject.name.Contains("[dead]"))
        {
            Debug.LogWarning((string)ID + "dead two times");
            gameObject.name = gameObject.name.Replace("[dead]", string.Empty);
        }

        if (anim == null)
        {
            Instantiator.RemovePerformer(gameObject.name);
            Destroy(gameObject);
        }
        else
        {
            // конечное уничтожение объекта произойдет в конце последней анимации
            Instantiator.MoveUnitToGarbage(this);
            gameObject.name += "[dead]";

            anim.fireEvents = false;
            anim.SetTrigger("death");
            anim.SetBool("ability", false);
            Destroy(this);
        }
    }
}
