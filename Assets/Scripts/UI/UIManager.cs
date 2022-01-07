using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Interact Message")]
    [SerializeField] private StringVariable interactMessage;

    /*  --------------------------
    *          Error Screen
    *   -------------------------- */

    // public void Error(string title, string message)
    // {
    //     GameObject popup = Instantiate(errorPopupPrefab, screens.Error.transform);

    //     ErrorPopup popupDetails = popup.GetComponent<ErrorPopup>();

    //     popupDetails.title.text = title;
    //     popupDetails.message.text = message;
    //     popupDetails.dismissButton.onClick.AddListener(delegate {
    //         screens.Error.SetActive(false);
    //         Destroy(popup.gameObject);
    //     });

    //     //BackToRoot();
    //     screens.Error.SetActive(true);
    //     OnErrorScreen?.Invoke();
    // }
}
