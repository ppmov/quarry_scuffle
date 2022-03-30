using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoContainer : MonoBehaviour
{
    [SerializeField]
    private List<PlayerInfo> infos;
    [SerializeField]
    private GameObject unitScroll;
    [SerializeField]
    private Text unitList;

    private void Start()
    {
        unitList.text = string.Empty;
    }

    private void FixedUpdate()
    {
        PlayerInfo selected = null;

        foreach (PlayerInfo info in infos)
            if (info.IsToggleOn)
                selected = info;

        unitScroll.SetActive(selected != null);

        if (selected == null) 
            return;

        // sort unit list
        Dictionary<string, int> pairs = Instantiator.GetPlayerUnits(selected.Index);
        List<string> sorted = new List<string>(pairs.Count);

        for (int i = 0; i < sorted.Capacity; i++)
        {
            int max = 0;
            string next = string.Empty;

            foreach (string naming in pairs.Keys)
                if (pairs[naming] > max)
                {
                    max = pairs[naming];
                    next = naming;
                }

            sorted.Add(next);
            pairs.Remove(next);
        }

        // return sorted list
        pairs = Instantiator.GetPlayerUnits(selected.Index);
        unitList.text = string.Empty;

        foreach (string naming in sorted)
        {
            string space;

            if (pairs[naming] < 10)
                space = "     "; // yes that's indent
            else
            if (pairs[naming] < 100)
                space = "   ";
            else
                space = " ";

            unitList.text += pairs[naming] + space + naming + "\n";
        }
    }

    public void OnToggleClick(int owner)
    {
        bool state = false;

        foreach (PlayerInfo info in infos)
            if (info.Index == owner)
                state = info.IsToggleOn;

        if (state == true)
            foreach (PlayerInfo info in infos)
                if (info.Index != owner)
                    info.IsToggleOn = false;
    }
}
