using UnityEngine;
using Context;

public class MainCastle : MonoBehaviour
{
    [SerializeField]
    private Side side;
    [SerializeField]
    private Builder builder;
    [SerializeField]
    private AI gate;
    [SerializeField]
    private BoxCollider site;
    [SerializeField]
    private GameObject fitter;

    private AI performer;

    void Awake()
    {
        performer = GetComponent<AI>();
        byte owner = (byte)side;

        while (owner < Players.MaxPlayersCount)
        {
            if (Players.GetPlayer(owner).IsDummy)
                owner += 2;
            else
                break;
        }

        if (owner > Players.MaxPlayersCount)
            owner = (byte)side;

        performer.ChangeOwner(owner);
        gate.ChangeOwner(owner);

        Instantiator.SetCastle(side, gameObject, site);
        Instantiator.AddPerformer(performer);
    }

    void OnDestroy()
    {
        // если крепость противоположной стороны уничтожена, значит победителя нет
        if (Instantiator.GetCastle(Sider.Invert(side)) != null)
            builder.EndGame(side, transform.position, transform.rotation);
    }
}
