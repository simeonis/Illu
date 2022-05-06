using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] Animator crosshairAnimator;

    [Header("Interaction Message")]
    [SerializeField] private TextMeshProUGUI interactionMessageUI;
    [SerializeField] private StringVariable interactionMessage;

    public void UpdateInteractMessage()
    {
        interactionMessageUI.text = interactionMessage.Value;
    }

    public void AnimationCrosshairInteraction()
    {
        crosshairAnimator.SetTrigger("interaction");
    }
}