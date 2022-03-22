using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Context;

public static class Instantiator
{
    public static GameObject GetCastle(Side side) => side == Side.Левые ? LeftCastle : RightCastle;
    public static List<Vector3> GetConstructionPoints(Side side) => side == Side.Левые ? LeftConstructionPoints : RightConstructionPoints;

    // массивы активных юнитов и зданий
    private static Dictionary<Naming, GameObject> Buildings { get; set; } = new Dictionary<Naming, GameObject>();
    private static Dictionary<Naming, GameObject> Units { get; set; } = new Dictionary<Naming, GameObject>();
    private static List<GameObject> Garbage { get; set; } = new List<GameObject>();
    private static GameObject LeftCastle { get; set; }
    private static List<Vector3> LeftConstructionPoints { get; set; }
    private static GameObject RightCastle { get; set; }
    private static List<Vector3> RightConstructionPoints { get; set; }

    private static Dictionary<int, Dictionary<string, int>> UnitsCounter { get; set; } = new Dictionary<int, Dictionary<string, int>>();

    // создание объектов
    public static void CreateBuilding(char fitId, int gradeId, byte owner, Vector3 position, Quaternion rotation)
    {
        Naming name = new Naming(Naming.Variety.Building, Players.GetPlayer(owner).Race, fitId, gradeId);
        byte i;

        for (i = byte.MinValue; i <= byte.MaxValue; i++)
            if (!Buildings.ContainsKey(name + '.' + owner + i))
                break;

        if (i == byte.MaxValue)
            return;

        object[] data = new object[2];
        data[0] = owner;
        data[1] = i;
        
        PhotonNetwork.Instantiate("Races/" + Players.GetPlayer(owner).Race + "/Buildings/" + name, position, rotation, 0, data);
    }

    public static void UpgradeBuilding(Naming parent, int gradeId)
    {
        Naming name = new Naming(Naming.Variety.Building, parent.Race, parent.Id, gradeId);
        byte i;

        if (!Buildings.TryGetValue(parent, out GameObject building))
            return;

        for (i = byte.MinValue; i <= byte.MaxValue; i++)
            if (!Buildings.ContainsKey(name + '.' + parent.Owner + i))
                break;

        if (i == byte.MaxValue)
            return;

        object[] data = new object[2];
        data[0] = (byte)parent.Owner;
        data[1] = i;

        PhotonNetwork.Instantiate("Races/" + parent.Race + "/Buildings/" + name, 
                                  building.transform.position, building.transform.rotation, 0, data);

        RemovePerformer(parent);
        PhotonNetwork.Destroy(building);
    }

    public static void CreateUnit(Naming.Variety type, char fitId, int gradeId, byte owner, Vector3 position, Quaternion rotation, Race race = Race.Случайно)
    {
        // юниты создаются только на мастере
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (race == Race.Случайно)
            race = Players.GetPlayer(owner).Race;

        Naming name = new Naming(type, race, fitId, gradeId);
        byte i;

        for (i = byte.MinValue; i <= byte.MaxValue; i++)
            if (!Units.ContainsKey(name + '.' + owner + i))
                break;

        if (i == byte.MaxValue)
            return;

        object[] data = new object[2];
        data[0] = owner;
        data[1] = i;

        PhotonNetwork.Instantiate("Races/" + race + "/Units/" + name, position, rotation, 0, data);
    }

    // управление буфером объектов
    public static void AddPerformer(AI ai)
    {
        if (ai == null) 
            return;

        if (ai.ID.Type == Naming.Variety.Building)
            Buildings.Add(ai.ID, ai.gameObject);
        else
        {
            Units.Add(ai.ID, ai.gameObject);

            if (!UnitsCounter.ContainsKey(ai.Owner))
                UnitsCounter.Add(ai.Owner, new Dictionary<string, int>());

            if (UnitsCounter[ai.Owner].ContainsKey(ai.Name))
                UnitsCounter[ai.Owner][ai.Name] += 1;
            else
                UnitsCounter[ai.Owner].Add(ai.Name, 1);
        }
    }

    public static void MoveUnitToGarbage(AI ai)
    {
        Garbage.Add(Units[ai.ID]);
        Units.Remove(ai.ID);

        if (UnitsCounter.ContainsKey(ai.Owner))
            if (UnitsCounter[ai.Owner].ContainsKey(ai.Name))
                UnitsCounter[ai.Owner][ai.Name] -= 1;

        if (UnitsCounter[ai.Owner][ai.Name] <= 0)
            UnitsCounter[ai.Owner].Remove(ai.Name);
    }

    public static void RemovePerformer(Naming id)
    {
        if (id.Type == Naming.Variety.Building)
            Buildings.Remove(id);
        else
        {
            for (int i = Garbage.Count - 1; i >= 0; i--)
                if (Garbage[i] == null || id == Garbage[i].name.Replace("[dead]", string.Empty))
                    Garbage.Remove(Garbage[i]);
        }
    }

    public static Dictionary<string, int> GetPlayerUnits(int owner)
    {
        Dictionary<string, int> list = new Dictionary<string, int>();

        if (UnitsCounter.ContainsKey(owner))
            foreach (string name in UnitsCounter[owner].Keys)
                list.Add(name, UnitsCounter[owner][name]);
        
        return list;
    }

    public static int GetPlayerUnitsCount(int owner)
    {
        int count = 0;

        if (UnitsCounter.ContainsKey(owner))
            foreach (int i in UnitsCounter[owner].Values)
                count += i;

        return count;
    }

    public static Vulnerable GetPerformer(string id)
    {
        if (id == string.Empty)
            return null;

        foreach (GameObject performer in id[0] == 'b' ? Buildings.Values : Units.Values)
        {
            if (performer == null)
                continue;

            if (performer.name != id)
                continue;

            Vulnerable vul = performer.GetComponent<Vulnerable>();

            if (vul != null)
                return vul;
            else
                break;
        }

        return null;
    }

    public static GameObject GetRandomDeadUnit()
    {
        if (Garbage.Count == 0)
            return null;

        return Garbage[Random.Range(0, Garbage.Count)];
    }

    public static Vulnerable GetPerformerWithFilter(Side side, PerformerType affectsOnly = PerformerType.Нечто)
    {
        List<int> exceptions = new List<int>();
        GameObject[] performers = new GameObject[(affectsOnly != PerformerType.Здание ? Units.Count : 0) + (affectsOnly != PerformerType.Создание ? Buildings.Count : 0)];
        int last = 0;

        if (affectsOnly != PerformerType.Создание)
        {
            Buildings.Values.CopyTo(performers, last);
            last += Buildings.Count;
        }

        if (affectsOnly != PerformerType.Здание)
        {
            Units.Values.CopyTo(performers, last);
            last += Buildings.Count;
        }

        // пока кол-во исключений меньше чем кол-во объектов
        while (exceptions.Count < performers.Length)
        {
            // возьмем случайную позицию
            int index = Random.Range(0, performers.Length);

            // сдвинем ее пока не вылезем из исключений
            for (; ; )
            {
                if (exceptions.Count == 0)
                    break;
                else
                if (performers.Length <= index)
                    index = 0;
                else
                if (exceptions.Contains(index))
                    index++;
                else
                    break;
            }

            // вернем объект если он соответствует условиям
            Naming name = performers[index].name;

            if (name.Side == side)
            {
                Vulnerable vul = performers[index].GetComponent<Vulnerable>();

                if (vul != null)
                    return vul;
            }

            exceptions.Add(index);
        }

        return null;
    }

    // единоразовая запись главных зданий
    public static void SetCastle(Side side, GameObject castle, BoxCollider site)
    {
        switch (side)
        {
            case Side.Левые:
                LeftCastle = castle;
                LeftConstructionPoints = FindBuildablePoints(side, site);
                break;
            case Side.Правые:
                RightCastle = castle;
                RightConstructionPoints = FindBuildablePoints(side, site);
                break;
            default:
                break;
        }
    }

    public static void Revert()
    {
        Buildings.Clear();
        Units.Clear();
        LeftCastle = null;
        RightCastle = null;
        LeftConstructionPoints = null;
        RightConstructionPoints = null;
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();
    }

    // позиции строительства
    private static List<Vector3> FindBuildablePoints(Side side, BoxCollider site)
    {
        if (site == null)
            return null;

        List<Vector3> poses = new List<Vector3>();
        int sign = side == Side.Левые ? -1 : 1;
        int xCount = Mathf.RoundToInt(site.size.x / 10f);
        int zCount = Mathf.RoundToInt(site.size.z / 10f);

        for (int x = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                Vector3 pos = new Vector3();
                pos.x = site.transform.position.x + site.center.x + sign * (site.size.x / 2f - 10f * x);
                pos.z = site.transform.position.z + site.center.z + sign * (site.size.z / 2f - 10f * z);
                pos = Fitter.RoundToBuildablePoint(pos);
                poses.Add(pos);
            }
        }

        return poses;
    }
}
