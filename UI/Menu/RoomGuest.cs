using UnityEngine;
using UnityEngine.UI;
using Context;
using Photon.Pun;
using static Players;
using System.Collections.Generic;

// Player cell for room menu
public class RoomGuest : MonoBehaviour
{
    [SerializeField]
    private byte index;
    [SerializeField]
    private Text playerName;
    [SerializeField]
    private Button button;
    [SerializeField]
    private Button aiButton;
    [SerializeField]
    private Dropdown race;
    [SerializeField]
    private MeshRenderer colorMesh;

    private RoomSettings settings;
    private Text buttonType;
    private Image background;

    private Color defaultColor;
    private Color readyColor;
    private List<Race> Races;

    public bool ReadyFlag
    {
        get => background.color == readyColor;
        set => background.color = value ? readyColor : defaultColor;
    }

    public string ButtonType
    {
        get => buttonType.text;
        private set
        {
            buttonType.text = value;
            button.gameObject.SetActive(value != string.Empty);
        }
    }

    private void Start()
    {
        settings = GetComponentInParent<RoomSettings>();
        background = GetComponent<Image>();
        buttonType = button.GetComponentInChildren<Text>();

        colorMesh.material = PlayerColors[index].flags;
        defaultColor = background.color;
        readyColor = settings.readyColor;

        System.Array array = System.Enum.GetValues(typeof(Race));
        Races = new List<Race>(array.Length);
        List<string> races = new List<string>(array.Length);

        foreach (Race race in array)
        {
            Races.Add(race);
            races.Add(race.ToString());
        }

        race.ClearOptions();
        race.AddOptions(races);
        race.value = 0;
    }

    private void OnGUI()
    {
        if (Myself is null) return;
        Player player = GetPlayer(index);

        if (player.IsDummy)
            SetFree();
        else
            SetPlayer(player);
    }

    private void SetFree()
    {
        playerName.text = "Free place";
        ButtonType = "Take";
        race.interactable = false;
        race.value = 0;
        ReadyFlag = false;
        aiButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    private void SetPlayer(Player player)
    {
        playerName.text = player.Name;
        ReadyFlag = player.Ready;
        aiButton.gameObject.SetActive(false);

        if (player.Id == Myself.Id)
        {
            race.interactable = true;
            race.value = Races.IndexOf(player.Race);
            ButtonType = string.Empty;
        }
        else
        {
            race.interactable = false;
            race.value = Races.IndexOf(player.Race);

            if (PhotonNetwork.IsMasterClient)
                ButtonType = "Kick";
            else
                ButtonType = string.Empty;
        }
    }

    public void OnDropdownSelect(int value)
    {
        if (IndexOfPlayer(Myself.Id) == index)
            SetMyProperty("race", (int)(char)Races[value]);
    }

    public void OnButtonClick()
    {
        switch (ButtonType)
        {
            case "Take":
                settings.OnReady(false);
                SetMyProperty("pos", index);
                break;
            case "Kick":
                KickPlayer(index);
                break;
            default:
                break;
        }
    }

    public void OnAIButtonClick()
    {
        UpdateBot(index);
    }
}
