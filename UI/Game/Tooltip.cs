using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Context;

public class Tooltip : MonoBehaviour
{
    private Text text;
    private AI Selected { get => InputController.Selected; }

    private void Start()
    {
        text = GetComponentInChildren<Text>();
    }

    // Смена тултипа при наведении на соответствующие элементы
    public void OnPointerBuildingButton(int index)
    {
        GameObject go;
        AI ai = null;
        int subindex = 0;

        if (Selected == null)
            go = Builder.ToolUp.GetBuildingPrefab(index, subindex);
        else // улучшение
            go = Builder.ToolUp.GetBuildingPrefab(Selected.ID.Id, Selected.ID.CountChildGrade((index - 3) / 4));

        if (go != null)
            ai = go.GetComponent<AI>();

        if (ai == null)
            return;

        text.text = ai.Tooltip;
    }

    public void OnPointerSpellIcon(int index)
    {
        if (Selected == null) return;
        if (Selected.Abilities.Count <= index) return;

        Ability spell = Selected.Abilities[index];
        text.text = spell.Tooltip;
    }

    public void OnPointerAuraIcon(int index)
    {
        if (Selected == null) return;
        if (Selected.Vulnerable.Affectables.Count <= index) return;

        Aura aura = Selected.Vulnerable.Affectables[index];
        text.text = aura.Tooltip;
    }

    public void OnPointerArmorType(ArmorType type)
    {
        List<float> damages = Resistances.GetDamages(type);
        text.text = "<b>" + type + "</b> защита получает:\n";

        for (int i = 0; i < damages.Count; i++)
            text.text += "<i>" + (DamageType)i + "</i> урон - <i>" + (damages[i] * 100) + "%</i>;\n";
    }

    public void OnPointerDamageType(DamageType type)
    {
        List<float> armors = Resistances.GetArmors(type);
        text.text = "<b>" + type + "</b> урон наносит:\n";

        for (int i = 0; i < armors.Count; i++)
            text.text += "<i>" + (ArmorType)i + "</i> защита - <i>" + (armors[i] * 100) + "%</i>;\n";
    }
}
