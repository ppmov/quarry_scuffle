using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuraManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> blocks;
    private AI Current { get => InputController.Selected; }

    private void FixedUpdate()
    {
        if (Current == null || Current.Vulnerable == null)
            return;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (Current.Vulnerable.Affectables.Count > i)
            {
                if (blocks[i].gameObject.name == Current.Vulnerable.Affectables[i].Name && blocks[i].activeSelf)
                    continue;

                blocks[i].SetActive(true);
                blocks[i].gameObject.name = Current.Vulnerable.Affectables[i].Name;
                Text text = blocks[i].GetComponentInChildren<Text>();
                text.text = Current.Vulnerable.Affectables[i].Name;
            }
            else
                blocks[i].SetActive(false);
        }
    }

    public void OnAuraPointerEnter(int index)
    {
        Builder.Tooltip.OnPointerAuraIcon(index);
    }
}
