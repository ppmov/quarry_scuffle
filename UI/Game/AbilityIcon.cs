using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour
{
    [SerializeField]
    private int id;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Text abilityName;
    [SerializeField]
    private Text damageType;

    private AI Current { get => InputController.Selected; }
    private Ability ability;
    private Color iconColor;

    private void Start()
    {
        iconColor = icon.color;
        damageType.gameObject.SetActive(false);
    }

    private void OnGUI()
    {
        if (Current == null) 
            return;

        if (id >= Current.Abilities.Count) 
            return;

        ability = Current.Abilities[id];

        // заполнение элементов UI
        abilityName.text = ability.Name;
        icon.fillAmount = ability.Cooldown.Value > 0f ? ((float)ability.Wasted / ability.Cooldown.Value) : 1f;

        // цвет иконки меняется при активации
        if (ability.IsCocked)
            icon.color = Color.blue;
        else
            icon.color = iconColor;

        // тип урона
        if (ability is ProjectileTargetedAbility dmgAbility)
        {
            damageType.gameObject.SetActive(true);
            damageType.text = dmgAbility.DamageType.ToString();
        }
        else
        {
            damageType.gameObject.SetActive(false);
        }
    }

    public void OnNamePointerEnter()
    {
        Builder.Tooltip.OnPointerSpellIcon(id);
    }

    public void OnDamagePointerEnter()
    {
        if (ability is ProjectileTargetedAbility dmgAbility)
            Builder.Tooltip.OnPointerDamageType(dmgAbility.DamageType);
    }
}