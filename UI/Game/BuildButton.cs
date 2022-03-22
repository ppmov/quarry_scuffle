using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Players;

public class BuildButton : MonoBehaviour
{
    [SerializeField]
    private int index;
    [SerializeField]
    private Button button;
    [SerializeField]
    private Text buttonTxt;
    [SerializeField]
    private Text costTxt;
    private int cost;

    public bool IsActive { get => gameObject.activeSelf; }

    private void OnGUI()
    {
        if (!gameObject.activeSelf)
            return;

        button.interactable = Myself.Stock >= cost;
        costTxt.text = cost.ToString();
    }

    public void OnButtonClick()
    {
        Builder.Instance.Fitting(index);
    }

    public void OnButtonPointerEnter()
    {
        Builder.Tooltip.OnPointerBuildingButton(index);
    }

    public void SetBuilding(string title, int cost)
    {
        gameObject.SetActive(true);
        buttonTxt.text = title;
        this.cost = cost;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        cost = 0;
    }
}
