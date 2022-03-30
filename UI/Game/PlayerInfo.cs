using Context;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Players;

public class PlayerInfo : MonoBehaviour
{
    public int Index { get => index; }
    public bool IsToggleOn { get => toggle.isOn; set => toggle.isOn = value; }

    [SerializeField]
    private int index;
    [SerializeField]
    private PlayerInfoContainer container;
    [SerializeField]
    private MeshRenderer flag;
    [SerializeField]
    private Text player;
    [SerializeField]
    private Text race;
    [SerializeField]
    private Text stock;
    [SerializeField]
    private Text units;
    [SerializeField]
    private Toggle toggle;

    private Player Player { get; set; }

    private void Start()
    {
        Player = GetPlayer((byte)index);

        if (Player.IsDummy)
            Destroy(gameObject);
        else
        {
            flag.material = PlayerColors[index].flags;
            player.text = Player.Name;
            race.text = Player.Race.ToString();
        }
    }

    private void OnGUI()
    {
        stock.text = "<color=yellow>" + Player.Stock + "</color> gold";
        units.text = "<color=red>" + Instantiator.GetPlayerUnitsCount(index) + "</color> count";
    }

    public void OnToggleClick()
    {
        container.OnToggleClick(index);
    }
}
