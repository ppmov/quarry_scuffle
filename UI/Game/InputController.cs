using UnityEngine;
using Context;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviourPunCallbacks
{
    public static AI Selected { get; private set; }
    private const float scrollSpeed = 2f;
    private const float defaultCamSpeed = 0.4f;

    [Header("Camera")]
    [SerializeField]
    private Vector2 xMinMax;
    [SerializeField]
    private Vector2 yMinMax;
    [SerializeField]
    private Vector2 dMinMax;
    [SerializeField]
    private EventSystem eventer;

    private Camera cam;
    private Vector3 moveVector = Vector3.zero;
    private float CamSpeed { get => cam.fieldOfView / dMinMax.y * defaultCamSpeed; }

    private void Start()
    {
        cam = Camera.main;
        ResetCameraPosition((int)Players.Myself.Side);
    }

    private void Update()
    {
        // camera moving
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x + moveVector.x * CamSpeed, xMinMax.x, xMinMax.y),
            transform.position.y,
            Mathf.Clamp(transform.position.z + moveVector.y * CamSpeed, yMinMax.x, yMinMax.y));

        // health bars displaying
        if (Input.GetKeyUp(KeyCode.LeftAlt))
            Options.IsHealthBarsVisible = !Options.IsHealthBarsVisible;

        // windows management
        if (Input.GetMouseButtonUp(0) && !eventer.IsPointerOverGameObject())
        {
            if (Selected != null)
                Selected.HasSubstrate = false;

            // unit click
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, 1 << 8))
            {
                Selected = hit.collider.gameObject.GetComponentInParent<AI>();
                
                if (Selected != null)
                    Selected.HasSubstrate = true;
            }
            else
            {
                Selected = null;
            }
        }

        // zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0f && !eventer.IsPointerOverGameObject())
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")) * scrollSpeed, dMinMax.x, dMinMax.y);
    }

    public void OnLeave()
    {
        Time.timeScale = 1;

        if (PhotonNetwork.IsConnectedAndReady)
            PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Instantiator.Revert();
        SceneManager.LoadScene(0);
    }

    public void ResetCameraPosition(int side)
    {
        // initial camera position - ally castle
        Vector3 startPos = Instantiator.GetCastle((Side)side).transform.position;
        startPos.y = cam.transform.position.y;
        startPos.z -= 20f;
        cam.transform.position = startPos;
    }

    public void CornerEnter(string anchor)
    {
        int k = 1;

        if (anchor.EndsWith("+"))
        {
            anchor = anchor.Trim('+');
            k++;
        }

        switch (anchor)
        {
            case "left":
                moveVector.x = -k;
                break;
            case "right":
                moveVector.x = k;
                break;
            case "up":
                moveVector.y = k;
                break;
            case "down":
                moveVector.y = -k;
                break;
            default:
                break;
        }
    }

    public void CornerExit(string anchor)
    {
        switch (anchor)
        {
            case "left":
                moveVector.x = 0;
                break;
            case "right":
                moveVector.x = 0;
                break;
            case "up":
                moveVector.y = 0;
                break;
            case "down":
                moveVector.y = 0;
                break;
            default:
                break;
        }
    }
}
