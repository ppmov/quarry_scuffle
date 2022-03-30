using System.Collections.Generic;
using UnityEngine;
using Context;

// Fraction setup
public class ToolUp : MonoBehaviour
{
    [SerializeField]
    private Race race;
    [SerializeField]
    private List<BuildingTool> buildings;

    [System.Serializable]
    private class BuildingTool
    {
        public Naming Id => new Naming(prefab.name);
        public bool IsBuildable => Id.Grade == 0;
        public GameObject Prefab => prefab;
        public int Cost => cost;

        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private int cost;

        public string Name { get; private set; }
        public string Description { get; private set; }

        public void Prepare()
        {
            IObjectReader obj = prefab.GetComponent<IObjectReader>();
            Name = obj.Name;
            Description = obj.Description;
        }
    }

    private void Awake()
    {
        foreach (BuildingTool buil in buildings)
            buil.Prepare();
    }

    public Dictionary<Naming, string> GetBuildableTexts(string parent = "")
    {
        Dictionary<Naming, string> texts = new Dictionary<Naming, string>();
        Naming parentName = parent;

        foreach (BuildingTool buil in buildings)
        {
            if (parent == string.Empty && buil.IsBuildable)
                texts.Add(buil.Id, buil.Name);
            else
            if (parentName.IsChild(buil.Id) && !buil.IsBuildable)
                texts.Add(buil.Id, buil.Name);
        }

        return texts;
    }

    public int GetBuildingCost(char index, int subindex)
    {
        foreach (BuildingTool buil in buildings)
            if (buil.Id.EndsWith(index + subindex.ToString()))
                return buil.Cost;

        return 0;
    }

    public GameObject GetBuildingPrefab(char index, int subindex)
    {
        foreach (BuildingTool buil in buildings)
            if (buil.Id.EndsWith(index + subindex.ToString()))
                return buil.Prefab;

        return null;
    }

    public GameObject GetBuildingPrefab(int index, int subindex)
    {
        return GetBuildingPrefab(Naming.Int2Hex(index), subindex);
    }
}
