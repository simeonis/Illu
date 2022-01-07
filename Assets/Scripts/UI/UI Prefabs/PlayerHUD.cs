using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private TextMeshProUGUI interactUI;
    [SerializeField] private StringVariable interactMessage;

    public void UpdateInteractMessage()
    {
        interactUI.text = interactMessage.Value;
    }
}
