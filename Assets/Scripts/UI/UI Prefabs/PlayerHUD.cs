using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] Animator crosshairAnimator;
    [SerializeField] private TextMeshProUGUI crosshairMessage;

    public void SetCrosshairText(string message)
    {
        crosshairMessage.text = message;
    }

    public void ClearCrosshairText() => SetCrosshairText("");

    public void RotateCrosshair() => crosshairAnimator.SetTrigger("interaction");

    public bool CrosshairTextEqual(string message)
    {
        return crosshairMessage.text == message;
    }
}