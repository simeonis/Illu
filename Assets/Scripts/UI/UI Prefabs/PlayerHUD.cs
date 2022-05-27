using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] Animator crosshairAnimator;
    [SerializeField] private TextMeshProUGUI crosshairText;

    [Header("Grappling Hook")]
    [SerializeField] private TextMeshProUGUI ropeRemainingText;
    [SerializeField] private TextMeshProUGUI grappleDistanceText;

    [Header("Scriptable Object")]
    [SerializeField] StringVariable _interactMessage;
    [SerializeField] TriggerVariable _rotateCrosshair;
    [SerializeField] FloatVariable _grappleDistance;

    void Start()
    {
        _interactMessage.AddListener(SetCrosshairText);
        _rotateCrosshair.AddListener(RotateCrosshair);
        _grappleDistance.AddListener(SetGrappleDistance);
    }

    // Crosshair
    void SetCrosshairText() => crosshairText.text = _interactMessage.Value;
    void RotateCrosshair() => crosshairAnimator.SetTrigger("interaction");

    // Ammo
    void SetGrappleDistance() => grappleDistanceText.text = $"({_grappleDistance.Value.ToString("0.#")}m)";
}