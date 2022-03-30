using UnityEngine;
using UnityEngine.UI;
using static MenuManager;

public class StartMenu : MonoBehaviour
{
    [SerializeField]
    private Text playerNameField;
    [SerializeField]
    private Text playerHolderField;
    [SerializeField]
    private Text roomNameField;
    [SerializeField]
    private Text roomHolderField;

    public void OnPlay()
    {
        if (!IsNamesCorrect())
            return;

        Manager.PlayerName = playerNameField.text;
        Manager.RoomName = roomNameField.text;
        Manager.Open(Menu.Room);
    }

    public void OnExit()
    {
        Application.Quit();
    }

    private bool IsNamesCorrect()
    {
        if (playerNameField.text == "")
        {
            playerHolderField.color = new Color(1, 0, 0);
            return false;
        }

        if (roomNameField.text == "")
        {
            roomHolderField.color = new Color(1, 0, 0);
            return false;
        }

        return true;
    }
}
