using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Context;
using static Players;

[System.Serializable]
public class Fitter
{
    [SerializeField]
    private GameObject prefab;
    private GameObject fitter;

    public GameObject Prefab { get => prefab; }

    public char Id { get; private set; }
    public bool IsHandling { get => fitter != null; }
    public Vector3 Position { get => fitter.transform.position; set => fitter.transform.position = value; }
    public Quaternion Rotation { get => fitter.transform.rotation; set => fitter.transform.rotation = value; }

    public void Enable(char fitId, Mesh mesh)
    {
        Id = fitId;
        fitter = Object.Instantiate(prefab, Vector3.zero, Quaternion.Euler(-90f, 0f, Myself.Side == Side.Right ? 180f : 0f));
        fitter.GetComponent<MeshFilter>().mesh = mesh;
        fitter.GetComponent<MeshRenderer>().material = Myself.Color.building;
    }

    public void Disable()
    {
        Object.Destroy(fitter);
        fitter = null;
    }

    public static Vector3 RoundToBuildablePoint(Vector3 point)
    {
        point.x = Mathf.Round(point.x / 10) * 10;
        point.y = 0;
        point.z = Mathf.Round(point.z / 10) * 10;
        return point;
    }
}
