using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Illu.Steam;

[System.Serializable]
public class LanLobbyMember : MonoBehaviour
{
    //Indicator
    [SerializeField] Image indicator;
    [SerializeField] TMP_Text Name;

    public void Create(LanUI.LanPlayer user)
    {
        // Prefab name
        this.name = user.id.ToString();
        // Name
        Name.text = user.Name;
    }

    public void SetIndicator(bool status) => indicator.color = status ? Color.green : Color.red;

}
