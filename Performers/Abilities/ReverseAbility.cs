using UnityEngine;
using Context;

public class ReverseAbility : Ability
{
    public override string Tooltip => "При расстоянии менее " + toBaseRange + " м до ближайшей базы разворачивается и бежит в обратную сторону";
    private const float toBaseRange = 25f;
    private Side direction;

    private GameObject Current => direction == Side.Левые ? Instantiator.GetCastle(Side.Левые) : Instantiator.GetCastle(Side.Правые);

    public override bool CheckAvailability(out Vulnerable target)
    {
        if (!base.CheckAvailability(out target))
            return false;

        if ((Current.transform.position - transform.position).magnitude > toBaseRange)
            return false;

        return true;
    }

    public override bool TryUse()
    {
        if (!base.TryUse())
            return false;

        // поворачиваем персонажа
        direction = Sider.Invert(direction);
        AI.WhereToGoWhenNoAbility = Current.transform.position;
        return true;
    }

    protected override void Awake()
    {
        base.Awake();
        IsAnimated = false;
        CanMoveWhenCocked = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        direction = Sider.Invert(AI.Side); // стартовое направление
    }
}
