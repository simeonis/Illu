using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : Interactable
{
    [Header("Display")]
    [SerializeField] private Equipment displayEquipment;

    [Header("Animation")]
    [SerializeField] private float glassDissolveDuration;
    [SerializeField] private float scaleDuration;

    [Header("Spin Options")]
    [SerializeField] private float rotationPerSecond = 15.0f;
    
    [Header("Hover Options")]
    [SerializeField] private float frequency = 1f;
    private float amplitude = 0.05f;
    private float heightOffset;

    private BoxCollider glassCollider;
    private Material glassMaterial;

    private EquipmentSlot equipmentSlot;
    // Equipment's default scale
    private Vector3 equipmentScale;
    // Equipment scale to fit inside pedestal glass
    private Vector3 pedestalScale;

    private Vector3 currentPosition = new Vector3();
    private Vector3 currentRotation = new Vector3();
    private IEnumerator coroutine;

    void Start()
    {
        // Get pedestal's glass
        Transform glass = transform.GetChild(0);
        glassCollider = glass.GetComponent<BoxCollider>();
        glassMaterial = glass.GetComponent<Renderer>().material;

        equipmentSlot = new EquipmentSlot(glass);
        pedestalScale = new Vector3(0.5f, 0.5f, 0.5f);
        heightOffset = 0.45f - amplitude / 2.0f;

        // Default Full (equipment inside pedestal)
        if (displayEquipment)
        {
            displayEquipment.Disable();
            equipmentSlot.Equip(displayEquipment);
            equipmentScale = displayEquipment.transform.localScale;
            displayEquipment.transform.localScale = pedestalScale;
            glassMaterial.SetFloat("_Dissolve", 0);
        }
        // Default Empty (no equipment inside pedestal)
        else
        {
            glassMaterial.SetFloat("_Dissolve", 1);
        }
    }

    void Update()
    {
        if (equipmentSlot.HasEquipment())
        {
            // Hover up & down
            currentPosition.y = heightOffset + Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

            // Rotate around Y-axis
            currentRotation.y += Time.deltaTime * rotationPerSecond;

            equipmentSlot.ApplyTransform(currentPosition, Quaternion.Euler(currentRotation));
        }
    }

    public override void OnStartAuthority()
    {
        throw new System.NotImplementedException();
    }

    public override void Interaction(Interactor interactor)
    {
        // Place equipment on pedestal
        if (!equipmentSlot.HasEquipment() && interactor.equipmentSlot.HasEquipment())
        {
            // Place equipment
            equipmentSlot.TransferFrom(interactor.equipmentSlot);
            equipmentScale = equipmentSlot.GetEquipment().transform.localScale;

            // Scale equipment then render glass
            if (coroutine != null) StopCoroutine(coroutine);
            StartCoroutine(coroutine = Scale());
        }
        // Take equipment from pedestal
        else if (equipmentSlot.HasEquipment() && !interactor.equipmentSlot.HasEquipment())
        {
            // Dissolve glass
            if (coroutine != null) StopCoroutine(coroutine);
            StartCoroutine(coroutine = Dissolve(interactor));
        }
    }

    private IEnumerator Dissolve(Interactor interactor)
    {
        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / glassDissolveDuration;
            glassMaterial.SetFloat("_Dissolve", percent);
            yield return null;
        }

        // Reset equipment scale
        equipmentSlot.GetEquipment().transform.localScale = equipmentScale;
        // Take equipment
        interactor.equipmentSlot.TransferFrom(equipmentSlot);
        glassCollider.enabled = false;
    }

    private IEnumerator Condense()
    {
        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / glassDissolveDuration;
            glassMaterial.SetFloat("_Dissolve", 1 - percent);
            yield return null;
        }

        glassCollider.enabled = true;
    }

    private IEnumerator Scale()
    {
        Equipment equipment = equipmentSlot.GetEquipment();

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / scaleDuration;
            // Calculate scale
            Vector3 scale = Vector3.Lerp(equipmentScale, pedestalScale, percent);
            // Apply scale
            equipment.transform.localScale = scale;
            // Modify height offset based on current scale
            heightOffset = scale.y - amplitude / 2.0f;
            yield return null;
        }

        // Object is scale, now begin rendering glass
        StartCoroutine(coroutine = Condense());
    }
}