using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private GridController gridController;

    [Header("Options")]
    [SerializeField] private BalanceObject[] objectPrefabs;
    [SerializeField] private Transform[] optionPositions;

    [Header("Parents")]
    [SerializeField] private Transform ghostObjectsParent;
    [SerializeField] private Transform placedObjectsParent;

    public int roundNumber = 0;
    [SerializeField] private float optionMoveDuration = 0.4f;
    private BalanceObject[] currentOptions;
    private BalanceObject currentPreviewObject;
    private BalanceObject currentPreviewPrefab;

    private Vector3 defaultOptionPositionScale = new Vector3(0.5f, 0.5f, 0.5f);

    private readonly Dictionary<BalanceObject, List<BalanceObject>> previewPool = new();

    private void Start()
    {
        if (optionPositions.Length > 0 && optionPositions[0] != null)
            defaultOptionPositionScale = optionPositions[0].localScale;

        CreateOptions();
    }

    private void CreateOptions()
    {
        currentOptions = new BalanceObject[optionPositions.Length];

        for (int i = 0; i < optionPositions.Length; i++)
        {
            BalanceObject randomPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

            ApplyOptionPositionSize(optionPositions[i], randomPrefab);

            BalanceObject option = Instantiate(randomPrefab, optionPositions[i].position, optionPositions[i].rotation);
            option.transform.parent = optionPositions[i].transform;
            option.InitializeOption(this, randomPrefab);
            currentOptions[i] = option;
        }
    }

    private void ApplyOptionPositionSize(Transform optionPosition, BalanceObject prefab)
    {
        optionPosition.localScale = new Vector3(
            defaultOptionPositionScale.x * prefab.HeightX,
            defaultOptionPositionScale.y,
            defaultOptionPositionScale.z * prefab.HeightY);
    }
    public void OffsetOptionPositionsZ(float zOffset)
    {
        StartCoroutine(OffsetOptionPositionsZCoroutine(zOffset));
    }

    private IEnumerator OffsetOptionPositionsZCoroutine(float zOffset)
    {
        Vector3[] startPositions = new Vector3[optionPositions.Length];
        Vector3[] targetPositions = new Vector3[optionPositions.Length];

        for (int i = 0; i < optionPositions.Length; i++)
        {
            if (optionPositions[i] == null)
                continue;

            startPositions[i] = optionPositions[i].position;
            targetPositions[i] = startPositions[i] + new Vector3(0f, 0f, zOffset);
        }

        float elapsedTime = 0f;

        while (elapsedTime < optionMoveDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / optionMoveDuration);
            t = t * t * (3f - 2f * t);

            for (int i = 0; i < optionPositions.Length; i++)
            {
                if (optionPositions[i] == null)
                    continue;

                optionPositions[i].position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
            }

            yield return null;
        }

        for (int i = 0; i < optionPositions.Length; i++)
        {
            if (optionPositions[i] != null)
                optionPositions[i].position = targetPositions[i];
        }
    }
    public void SelectObject(BalanceObject selectedPrefab)
    {
        if (selectedPrefab == null)
            return;

        if (currentPreviewPrefab == selectedPrefab && currentPreviewObject != null)
        {
            ReturnCurrentPreviewToPool();
            return;
        }

        ReturnCurrentPreviewToPool();

        currentPreviewPrefab = selectedPrefab;
        currentPreviewObject = GetPreviewFromPool(selectedPrefab);

        currentPreviewObject.transform.SetParent(ghostObjectsParent, false);
        currentPreviewObject.gameObject.SetActive(true);
        currentPreviewObject.SetSelectable(false);
        currentPreviewObject.SetPreviewColliders(false);
        currentPreviewObject.InitializePreviewRotation(selectedPrefab.transform.localRotation);

        gridController.SetSelectedObject(currentPreviewObject.gameObject);
    }

    private BalanceObject GetPreviewFromPool(BalanceObject prefab)
    {
        if (!previewPool.TryGetValue(prefab, out List<BalanceObject> prefabPool))
        {
            prefabPool = new List<BalanceObject>();
            previewPool.Add(prefab, prefabPool);
        }

        foreach (BalanceObject pooledObject in prefabPool)
        {
            if (pooledObject != null && !pooledObject.gameObject.activeSelf)
                return pooledObject;
        }

        BalanceObject newPreview = ghostObjectsParent != null ? Instantiate(prefab, ghostObjectsParent) : Instantiate(prefab);

        newPreview.SetSelectable(false);
        newPreview.SetPreviewColliders(false);
        newPreview.gameObject.SetActive(false);

        prefabPool.Add(newPreview);

        return newPreview;
    }

    private void ReturnCurrentPreviewToPool()
    {
        if (currentPreviewObject == null)
            return;

        currentPreviewObject.RemoveGhostMaterial();
        currentPreviewObject.ResetRotationState();
        currentPreviewObject.SetPreviewColliders(false);

        if (ghostObjectsParent != null)
            currentPreviewObject.transform.SetParent(ghostObjectsParent, false);

        currentPreviewObject.transform.localPosition = Vector3.zero;
        currentPreviewObject.gameObject.SetActive(false);

        currentPreviewObject = null;
        currentPreviewPrefab = null;

        if (gridController != null)
            gridController.SetSelectedObject(null);
    }

    public BalanceObject ConfirmPlacement(Vector3 position)
    {
        if (currentPreviewPrefab == null || currentPreviewObject == null)
            return null;

        int previewRotationStep = currentPreviewObject.RotationStep;
        BalanceObject placedObject = placedObjectsParent != null ? Instantiate(currentPreviewPrefab, placedObjectsParent) : Instantiate(currentPreviewPrefab);

        placedObject.transform.position = position;
        placedObject.SetRotationStep(previewRotationStep);
        placedObject.SetSelectable(false);
        placedObject.SetPreviewColliders(true);
        placedObject.RemoveGhostMaterial();

        ReturnCurrentPreviewToPool();

        return placedObject;
    }

    public void CompleteTurn()
    {
        roundNumber++;

        if (roundNumber % 4 == 0)
            gridController.ScaleGrid();

        ClearOptions();
        CreateOptions();
    }

    private void ClearOptions()
    {
        if (currentOptions == null)
            return;

        foreach (BalanceObject option in currentOptions)
        {
            if (option != null)
                Destroy(option.gameObject);
        }
    }
}