using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using static Players;
using static MenuManager;
using Photon.Realtime;

// Room management and interaction
public class RoomSettings : MonoBehaviourPunCallbacks
{
    private const float syncFrequency = 1f;
    private float cumulativeTime = 0;

    [SerializeField]
    private GameObject startButton;
    [SerializeField]
    private GameObject readyButton;
    [SerializeField]
    private GameObject leaveButton;
    [SerializeField]
    private Toggle fastModeFlag;
    [SerializeField]
    private Text roomNameField;
    [SerializeField]
    public Color readyColor;

    private void Start()
    {
        PhotonNetwork.NickName = Manager.PlayerName;
        PhotonNetwork.GameVersion = Options.GameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        Revert();
    }

    public override void OnConnectedToMaster()
    {
        JoinRoom();
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(
            Manager.RoomName,
            new RoomOptions { MaxPlayers = (byte)MaxPlayersCount, PublishUserId = true },
            TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        // set UI for host or guest
        roomNameField.text = PhotonNetwork.CurrentRoom.Name;
        readyButton.SetActive(true);
        leaveButton.SetActive(true);

        // load all players data
        ReloadPreparingCustomProperties();
        SetMyProperty("pos", GetFreeSlot());

        if (!PhotonNetwork.IsMasterClient)
            fastModeFlag.enabled = false;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        return;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        ClearSlotLocal(IndexOfPlayer(otherPlayer.UserId));
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PhotonNetwork.Disconnect();
    }

    private void FixedUpdate()
    {
        if (Myself is null) return;
        cumulativeTime += Time.fixedDeltaTime;

        if (cumulativeTime < syncFrequency)
            return;
        else
            cumulativeTime = 0;

        ReloadPreparingCustomProperties();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(IsEveryoneReady());
            fastModeFlag.enabled = PhotonNetwork.CurrentRoom.PlayerCount <= 1;
        }
    }

    // UI buttons events
    public void OnReady(bool value = true)
    {
        readyButton.SetActive(!value);
        SetMyProperty("ready", value);
    }

    public void OnStart()
    {
        if (fastModeFlag.enabled && fastModeFlag.isOn)
            Time.timeScale = 1.5f;
        else
            Time.timeScale = 1f;

        RandomizePlayersRaces();
        PhotonNetwork.LoadLevel(1);
    }

    public void OnLeave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Manager.Open(Menu.Main);
    }
}
