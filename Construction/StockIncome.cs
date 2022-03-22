using UnityEngine;
using Photon.Pun;
using static Players;

public class StockIncome : MonoBehaviour
{
    [SerializeField]
    private int exchangeTime = 25;
    [SerializeField]
    private int exchangeValue = 10;
    
    private float exchangeFixedTime = 0f;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            IncomeAllPlayers(exchangeValue * 4);
    }

    // постоянный доход всех игроков
    private void FixedUpdate()
    {
        ReloadRealtimeCustomProperties();

        if (PhotonNetwork.IsMasterClient)
        {
            exchangeFixedTime += Time.fixedDeltaTime;

            if (exchangeFixedTime < exchangeTime)
                return;
            else
                exchangeFixedTime = 0;

            IncomeAllPlayers(exchangeValue);
        }
    }
}
