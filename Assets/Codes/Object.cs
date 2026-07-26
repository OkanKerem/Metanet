using UnityEngine;

public class BalanceObject : MonoBehaviour
{
    [SerializeField] private int heightX = 1;
    [SerializeField] private int heightY = 1;
    [SerializeField] private Weight weight = Weight.Medium;
    public string DisplayName = "Balance Object";
    public int RotationStep => rotationStep;
    public int HeightX => rotationStep % 2 == 0 ? heightX : heightY;
    public int HeightY => rotationStep % 2 == 0 ? heightY : heightX;
    public Weight Weight => weight;
    public Vector2 GridCenter { get; private set; }

    private int rotationStep;
    private Quaternion baseLocalRotation;

    private Renderer[] objectRenderers;
    private Material[][] originalMaterials;
    private Collider[] objectColliders;
    
    private TurnManager turnManager;
    private BalanceObject representedPrefab;
    private bool isSelectable;

    private void Awake()
    {
        baseLocalRotation = transform.localRotation;

        objectRenderers = GetComponentsInChildren<Renderer>(true);
        objectColliders = GetComponentsInChildren<Collider>(true);
        originalMaterials = new Material[objectRenderers.Length][];

        for (int i = 0; i < objectRenderers.Length; i++)
            originalMaterials[i] = objectRenderers[i].sharedMaterials;
    }

    public void InitializeOption(TurnManager manager, BalanceObject prefab)
    {
        turnManager = manager;
        representedPrefab = prefab;
        isSelectable = true;

        SetPreviewColliders(true);
        ResetRotationState();
    }

    public void InitializePreviewRotation(Quaternion prefabLocalRotation)
    {
        baseLocalRotation = prefabLocalRotation;
        rotationStep = 0;
        transform.localRotation = baseLocalRotation;
    }

    private void OnMouseDown()
    {
        if (!isSelectable || turnManager == null || representedPrefab == null)
            return;

        turnManager.SelectObject(representedPrefab);
    }

    public void Rotate90Degrees()
    {
        rotationStep = (rotationStep + 1) % 4;
        ApplyRotation();
    }

    public void SetRotationStep(int newRotationStep)
    {
        rotationStep = ((newRotationStep % 4) + 4) % 4;
        ApplyRotation();
    }

    public void ResetRotationState()
    {
        rotationStep = 0;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, rotationStep * 90f, 0f);
    }

    public void SetGridCenter(float x, float z)
    {
        GridCenter = new Vector2(x, z);
    }

    public void OffsetGridCenter(float x, float z)
    {
        GridCenter += new Vector2(x, z);
    }

    public void SetSelectable(bool value)
    {
        isSelectable = value;
    }

    public void SetGhostMaterial(Material ghostMaterial)
    {
        if (ghostMaterial == null)
            return;

        for (int i = 0; i < objectRenderers.Length; i++)
        {
            Material[] ghostMaterials = new Material[originalMaterials[i].Length];

            for (int j = 0; j < ghostMaterials.Length; j++)
                ghostMaterials[j] = ghostMaterial;

            objectRenderers[i].sharedMaterials = ghostMaterials;
        }
    }

    public void RemoveGhostMaterial()
    {
        for (int i = 0; i < objectRenderers.Length; i++)
            objectRenderers[i].sharedMaterials = originalMaterials[i];
    }

    public void SetPreviewColliders(bool value)
    {
        for (int i = 0; i < objectColliders.Length; i++)
            objectColliders[i].enabled = value;
    }
}