using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Context;
using static Instantiator;

public class BotBuilder : MonoBehaviour
{
    private Player player;
    private ToolUp raceTool;
    private int buildIndex = 0;

    public void SetBot(Player player)
    {
        this.player = player;
        GameObject tool = Resources.Load<GameObject>("Races/" + player.Race + "/Ups");
        raceTool = Instantiate(tool, transform).GetComponent<ToolUp>();
        StartCoroutine(SecondUpdate());
    }

    private IEnumerator SecondUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            TryBuildSomething();
        }
    }

    private void TryBuildSomething()
    {
        if (GetConstructionPoints(player.Side).Count == 0)
            return;

        if (buildIndex >= GetConstructionPoints(player.Side).Count)
            buildIndex = 0;

        Vector3 buildPos = GetConstructionPoints(player.Side)[buildIndex];

        if (Physics.Linecast(buildPos + Vector3.up * 10f, buildPos, 1 << 9))
            buildIndex++;
        else
        {
            char fitId = Naming.Int2Hex(Random.Range(1, raceTool.GetBuildableTexts().Count));
            int cost = raceTool.GetBuildingCost(fitId, 0);

            if (player.Stock >= cost)
            {
                player.Stock -= cost;
                CreateBuilding(fitId, 0, Players.IndexOfPlayer(player.Id), buildPos, Quaternion.Euler(-90f, 0f, player.Side == Side.Right ? 180f : 0f));
                buildIndex++;
            }
        }
    }
}
