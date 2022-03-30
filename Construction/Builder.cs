using System.Collections.Generic;
using UnityEngine;
using Context;
using static Players;
using Photon.Pun;

// Building construction
public class Builder : MonoBehaviour
{
    [SerializeField]
    private GameObject buildMenu;
    [SerializeField]
    private Tooltip tooltip;
    [SerializeField]
    private Fitter fitter = new Fitter();

    private Camera cam;
    private BuildButton[] buttons;

    private Side Side { get => Myself.Side; }
    private Race Race { get => Myself.Race; }
    private Naming Selected { get; set; }

    public static Builder Instance { get; private set; }
    public static ToolUp ToolUp { get; private set; }
    public static Tooltip Tooltip { get; private set; }

    private void Awake()
    {
        cam = Camera.main;
        Instance = this;
        Tooltip = tooltip;

        buttons = buildMenu.GetComponentsInChildren<BuildButton>();
        ReloadPreparingCustomProperties(true);

        // fraction setting object
        GameObject tool = Resources.Load<GameObject>("Races/" + Race + "/Ups");
        ToolUp = Instantiate(tool, transform).GetComponent<ToolUp>();

        SetDefaultUI();
    }

    private void Start()
    {
        // add AI's if needed
        if (PhotonNetwork.IsMasterClient)
            foreach (Player bot in GetBotPlayers())
                gameObject.AddComponent<BotBuilder>().SetBot(bot);
    }

    private void OnGUI()
    {
        if (InputController.Selected == null)
        {
            if (Selected != string.Empty)
                SetDefaultUI();

            return;
        }
        
        if (InputController.Selected.ID != Selected)
            SetPerformerUI(InputController.Selected.ID);
    }

    // available build window
    private void SetDefaultUI()
    {
        HideButtons();
        Selected = string.Empty;

        // enable build buttons for initial level buildings
        Dictionary<Naming, string> texts = ToolUp.GetBuildableTexts();

        for (int i = 0; i < buttons.Length; i++)
        {
            foreach (Naming id in texts.Keys)
                if (id.Id == Naming.Int2Hex(i))
                {
                    buttons[i].SetBuilding(texts[id], ToolUp.GetBuildingCost(id.Id, 0));
                    break;
                }

            if (!buttons[i].IsActive)
                break;
        }
    }

    // build improvement window
    private void SetPerformerUI(Naming selected)
    {
        HideButtons();
        Selected = selected;

        if (Selected.Type != Naming.Variety.Building)
            return;

        if (Selected.Race != Race)
            return;

        if (Selected.Owner != IndexOfPlayer(Myself.Id))
            return;

        // enable build buttons for not initial level buildings
        Dictionary<Naming, string> texts = ToolUp.GetBuildableTexts(Selected);

        // fill 3, 7 and 11 buttons with next grade values
        for (int i = 3; i <= 11; i += 4)
        {
            foreach (Naming id in texts.Keys)
                if (id.Grade == i % 3 + 1)
                {
                    buttons[i].SetBuilding(texts[id], ToolUp.GetBuildingCost(id.Id, id.Grade));
                    break;
                }

            if (!buttons[i].IsActive)
                break;
        }
    }

    private void HideButtons()
    {
        foreach (BuildButton button in buttons)
            button.Disable();
    }
    
    // on building button click
    public void Fitting(int index)
    {
        if (Selected == string.Empty)
            StartFitting(index);
        else
            UpgradeBuilding(index);
    }

    // build position selection
    private void StartFitting(int buttonIndex)
    {
        if (fitter.IsHandling) 
            return;

        char fitId = Naming.Int2Hex(buttonIndex);

        if (Myself.Stock < ToolUp.GetBuildingCost(fitId, 0)) 
            return;

        GameObject prefab = ToolUp.GetBuildingPrefab(buttonIndex, 0);

        if (prefab == null) 
            return;

        fitter.Enable(fitId, prefab.GetComponent<MeshFilter>().sharedMesh);
    }

    private void UpgradeBuilding(int buttonIndex)
    {
        char fitId = Selected.Id;
        int gradeId = Selected.CountChildGrade((buttonIndex - 3) / 4);

        if (!SpendMyStock(ToolUp.GetBuildingCost(fitId, gradeId)))
            return;

        Instantiator.UpgradeBuilding(Selected, gradeId);
    }

    private void Update()
    {
        if (!fitter.IsHandling)
            return;

        // update fitter position within the construction site
        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, 1 << (int)Side + 11))
            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, (1 << 3) | (1 << 9)))
                fitter.Position = Fitter.RoundToBuildablePoint(hit.point);

        // cancel on right button
        if (Input.GetMouseButton(1))
            fitter.Disable();
        else
        // build on left button
        if (Input.GetMouseButtonDown(0) && fitter.Position != Vector3.zero)
            if (SpendMyStock(ToolUp.GetBuildingCost(fitter.Id, 0)))
            {
                Instantiator.CreateBuilding(fitter.Id, 0, IndexOfPlayer(Myself.Id), fitter.Position, fitter.Rotation);
                fitter.Disable();
            }
    }

    public void EndGame(Side loser, Vector3 castlePosition, Quaternion castleRotation)
    {
        GameObject ruin = Instantiate(fitter.Prefab, castlePosition, castleRotation);
        Instantiator.SetCastle(loser, ruin, null);
        Time.timeScale = 0;
    }
}
