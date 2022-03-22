using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Context;

// Индикатор здоровьев
public class Healthbar : MonoBehaviour
{
    private Vulnerable vulner;
    private Image image;
    private Vector3 abovePosition = new Vector3(0f, 2f, 0.5f);
    private Quaternion defaultRotation;

    private void Start()
    {
        image = GetComponentInChildren<Image>();
        vulner = GetComponentInParent<Vulnerable>();

        switch (vulner.Side)
        {
            case Side.Левые:
                defaultRotation = new Quaternion(-0.4f, 0f, 0f, 0.9f);
                image.fillOrigin--;
                break;
            case Side.Правые:
                defaultRotation = new Quaternion(0f, 0.9f, 0.4f, 0f);
                break;
            default:
                defaultRotation = transform.rotation;
                break;
        }
    }

    private void Update()
    {
        image.enabled = Options.IsHealthBarsVisible;
        transform.position = vulner.transform.position + abovePosition;
        transform.rotation = vulner.Side == Side.Правые ? defaultRotation : Quaternion.Inverse(defaultRotation);
        image.fillAmount = vulner.Health.Value / vulner.Health.Initial;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
