using System.Collections.Generic;
using UnityEngine;
using Context;
using static Players;
using Photon.Pun;

// Строительство зданий
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

        // настроечный объект расы
        GameObject tool = Resources.Load<GameObject>("Races/" + Race + "/Ups");
        ToolUp = Instantiate(tool, transform).GetComponent<ToolUp>();

        // включаем дефолтный режим
        SetDefaultUI();
    }

    private void Start()
    {
        // добавим ботов при необходимости
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

    // окно строительства доступных зданий
    private void SetDefaultUI()
    {
        HideButtons();
        Selected = string.Empty;

        // активируем кнопки строительства зданий с уровнем 0
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

    // окно улучшения выбранного здания
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

        // активируем кнопки строительства зданий с уровнем выше 0
        Dictionary<Naming, string> texts = ToolUp.GetBuildableTexts(Selected);

        // заполняем 3, 7 и 11 кнопку следующими по грейду значениями
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
    
    // нажатие на кнопку строительства
    public void Fitting(int index)
    {
        if (Selected == string.Empty)
            StartFitting(index);
        else
            UpgradeBuilding(index);
    }

    // начало выбора места строительства
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

    // улучшение здания
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

        // обновляем позицию макета, если соблюдена зона строительства
        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, 1 << (int)Side + 11))
            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, (1 << 3) | (1 << 9)))
                fitter.Position = Fitter.RoundToBuildablePoint(hit.point);

        // на правую кнопку примерка отменяется
        if (Input.GetMouseButton(1))
            fitter.Disable();
        else
        // на левую кнопку строим здание
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
