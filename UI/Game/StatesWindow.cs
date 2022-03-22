using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatesWindow : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private Text uiName;
    [SerializeField]
    private Text txtArmorType;
    [SerializeField]
    private Text txtArmor;
    [SerializeField]
    private Text txtHealth;
    [SerializeField]
    private Image imgHealth;
    [SerializeField]
    private List<GameObject> abilityPanels;

    private bool IsOpened { get => panel.activeSelf; set => panel.SetActive(value); }

    private AI Selected { get => InputController.Selected; }
    private Vulnerable Vulnerable { get => Selected == null ? null : Selected.Vulnerable; }

    private void OnGUI()
    {
        if (Selected != null)
        {
            if (uiName.text != Selected.name)
                Open();

            FillInfo();
        }
        else
        if (IsOpened)
            Close();
    }

    // открытие окна с инф-ей по сущности
    private void Open()
    {
        IsOpened = true;
        Revert();

        // блоки способностей
        for (int i = 0; i < abilityPanels.Count; i++)
        {
            if (i < Selected.Abilities.Count)
                abilityPanels[i].SetActive(true);
            else
                abilityPanels[i].SetActive(false);
        }
    }

    // закрытие окна
    private void Close()
    {
        IsOpened = false;
        Revert();

        for (int i = 0; i < abilityPanels.Count; i++)
            abilityPanels[i].SetActive(false);
    }

    private void Revert()
    {
        imgHealth.fillAmount = 1;
        txtHealth.text = string.Empty;
        txtArmorType.text = "Неуязвимый";
        txtArmor.text = string.Empty;
    }

    private void FillInfo()
    {
        uiName.text = Selected.Name;

        if (Vulnerable != null)
        {
            imgHealth.fillAmount = Vulnerable.Health.Value / Vulnerable.Health.Initial;
            txtHealth.text = Vulnerable.Health.Value + " / " + Vulnerable.Health.Initial;
            txtArmorType.text = Vulnerable.ArmorType.ToString();
            txtArmor.text = "+ " + Vulnerable.Armor.Value + "% снижение урона";
        }
    }

    public void OnPointerArmorType()
    {
        if (Vulnerable != null)
            Builder.Tooltip.OnPointerArmorType(Vulnerable.ArmorType);
    }
}
