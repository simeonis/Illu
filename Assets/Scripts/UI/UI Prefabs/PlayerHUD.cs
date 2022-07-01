using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] PlayerCamera playerCamera;

    [Header("Screens")]
    [SerializeField] GameObject resumeScreen;
    [SerializeField] GameObject pauseScreen;

    [Header("Crosshair")]
    [SerializeField] Animator crosshairAnimator;
    [SerializeField] private TextMeshProUGUI crosshairText;

    [Header("Grappling Hook")]
    [SerializeField] private TextMeshProUGUI ropeRemainingText;
    [SerializeField] private TextMeshProUGUI grappleDistanceText;

    [Header("Scriptable Object")]
    [SerializeField] StringVariable _interactMessage;
    [SerializeField] TriggerVariable _rotateCrosshair;
    [SerializeField] FloatVariable _ropeRemaining;
    [SerializeField] FloatVariable _grappleDistance;

    void Awake()
    {
        _interactMessage.AddListener(SetCrosshairText);
        _rotateCrosshair.AddListener(RotateCrosshair);
        _ropeRemaining.AddListener(SetRopeRemaining);
        _grappleDistance.AddListener(SetGrappleDistance);

        GameResumed();
    }

    void OnEnable()
    {
        GameManager.Instance.AddListener(GameManager.Event.GameResumed, GameResumed);
        GameManager.Instance.AddListener(GameManager.Event.GamePaused, GamePaused);
    }

    void OnDisable()
    {
        GameManager.Instance.RemoveListener(GameManager.Event.GameResumed, GameResumed);
        GameManager.Instance.RemoveListener(GameManager.Event.GamePaused, GamePaused);
    }

    public void GameResumed()
    {
        resumeScreen.SetActive(true);
        pauseScreen.SetActive(false);
        playerCamera.LockCinemachine(false);
        InputManager.Instance.TogglePlayer();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GamePaused()
    {
        resumeScreen.SetActive(false);
        pauseScreen.SetActive(true);
        playerCamera.LockCinemachine(true);
        InputManager.Instance.ToggleMenu();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GameLeft() => SceneManager.LoadScene("Main Menu");

    // Crosshair
    void SetCrosshairText() => crosshairText.text = _interactMessage.Value;
    void RotateCrosshair() => crosshairAnimator.SetTrigger("interaction");

    // Ammo
    void SetRopeRemaining() => ropeRemainingText.text = $"{_ropeRemaining.Value.ToString("0.#")}m";
    void SetGrappleDistance() => grappleDistanceText.text = $"({_grappleDistance.Value.ToString("0.#")}m)";
}