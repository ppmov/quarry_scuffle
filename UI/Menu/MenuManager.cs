using UnityEngine;

// Управление окнами меню
public class MenuManager : MonoBehaviour
{
    public enum Menu { Main, Room }
    public static MenuManager Manager { get; private set; }

    public string PlayerName { get; set; }
    public string RoomName { get; set; }

    [SerializeField]
    private GameObject mainMenuPrefab;
    [SerializeField]
    private GameObject roomMenuPrefab;

    private GameObject mainMenu;
    private GameObject roomMenu;

    private void Awake()
    {
        Manager = this;
    }

    private void Start()
    {
        Time.timeScale = 1;
        Open(Menu.Main); 
    }

    public void Open(Menu menu)
    {
        switch (menu)
        {
            case Menu.Main:
                if (mainMenu != null)
                    return;

                if (roomMenu != null)
                    Destroy(roomMenu);

                mainMenu = Instantiate(mainMenuPrefab, transform);
                break;

            case Menu.Room:
                if (roomMenu != null)
                    return;

                if (mainMenu != null)
                    Destroy(mainMenu);

                roomMenu = Instantiate(roomMenuPrefab, transform);
                break;

            default:
                break;
        }
    }
}
