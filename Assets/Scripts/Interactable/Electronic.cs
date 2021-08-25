using UnityEngine;

public class Electronic : MonoBehaviour
{
    [Header("Powered Settings")]
    [SerializeField, ColorUsage(true, true)] private Color poweredOnColor;
    [SerializeField] private bool powered;

    private Material material;
    private Interactable interactable;
    private string interactMessage;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        if (interactable) interactMessage = interactable.interactMessage;

        // Create a new unique material
        material = new Material(Shader.Find("Shader Graphs/Toon"));
        material.SetColor("MainColor", Color.gray);
        material.SetColor("SecondaryColor", Color.gray);
        material.SetColor("RimColor", poweredOnColor);

        // Determine electronic state
        if (powered) PowerOn();
        else PowerOff();

        // Get renderer
        Renderer renderer = GetComponent<Renderer>();
        
        // Replace material
        Material[] materials = renderer.materials;

        for(int i=0; i<materials.Length; i++)
        {
            if (materials[i].name.Replace(" (Instance)", "") == "Default_Powered")
            {
                materials[i] = material;
            }
        }

        renderer.materials = materials;
    }

    public void PowerOn()
    {
        powered = true;
        material.SetFloat("RimAmount", 0f);
        if (interactable)
        {
            interactable.enabled = true;
            interactable.interactMessage = interactMessage;
        }
    }

    public void PowerOff()
    {
        powered = false;
        material.SetFloat("RimAmount", 1f);
        if (interactable)
        {
            interactable.enabled = false;
            interactable.interactMessage = "Unpowered";
        }
    }
}
