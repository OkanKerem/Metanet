using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private int weightX = 5;
    [SerializeField] private int weightZ = 5;

    [Header("References")]
    [SerializeField] private PlacementVisualConfig placementVisualConfig;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private LayerMask gridLayerMask;
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private TableBalanceController tableBalanceController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Grid Expansion Animation")]
    [SerializeField] private float cellSpawnDuration = 0.2f;
    [SerializeField] private float cellSpawnDelay = 0.03f;

    public int WeightX
    {
        get => weightX;
        set => weightX = value;
    }

    public int WeightZ
    {
        get => weightZ;
        set => weightZ = value;
    }

    public GameObject[,] gridArray;
    public GridCell[,] gridCells;
    public GameObject selectedGameObject;

    private RaycastHit hit;
    private bool isExpanding;

    public bool IsPlacingObject => selectedGameObject != null;
    public bool IsExpanding => isExpanding;

    private void Start()
    {
        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();

        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

        if (gridParent == null || gridPrefab == null)
        {
            Debug.LogError("Grid Parent veya Grid Prefab atanmam??.", this);
            enabled = false;
            return;
        }

        CreateGrid();
    }

    private void Update()
    {
        if (isExpanding || selectedGameObject == null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
            return;

        GridCell hitCell = hit.collider.GetComponentInParent<GridCell>();
        BalanceObject balanceObject = selectedGameObject.GetComponent<BalanceObject>();

        if (hitCell == null || balanceObject == null)
            return;

        if (cameraController != null && cameraController.ShouldRotatePlacedObject)
            balanceObject.Rotate90Degrees();
        else if (cameraController == null && Input.GetMouseButtonDown(1))
            balanceObject.Rotate90Degrees();

        Vector2Int startCell = GetStartCell(hitCell.X, hitCell.Z);
        bool canPlace = CanPlaceObject(balanceObject, startCell.x, startCell.y);

        selectedGameObject.transform.position = GetPlacementPosition(balanceObject, startCell.x, startCell.y);

        Material ghostMaterial = canPlace
            ? placementVisualConfig.PlaceableGhostMaterial
            : placementVisualConfig.NotPlaceableGhostMaterial;

        balanceObject.SetGhostMaterial(ghostMaterial);

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            Vector3 placementPosition = selectedGameObject.transform.position;
            BalanceObject placedObject = turnManager.ConfirmPlacement(placementPosition);

            if (placedObject == null)
                return;

            PlaceObject(placedObject, startCell.x, startCell.y);
            selectedGameObject = null;

            if (scoreManager != null)
                scoreManager.RegisterPlacement(placedObject);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayPlaceObjectSound();

            if (tableBalanceController != null)
                tableBalanceController.RecalculateBalance();

            turnManager.CompleteTurn();
        }
    }

    public void SetSelectedObject(GameObject selectedObject)
    {
        selectedGameObject = selectedObject;
    }

    private void CreateGrid()
    {
        gridArray = new GameObject[weightX, weightZ];
        gridCells = new GridCell[weightX, weightZ];

        float startX = -(weightX - 1) * 0.5f;
        float startZ = -(weightZ - 1) * 0.5f;

        for (int x = 0; x < weightX; x++)
        {
            for (int z = 0; z < weightZ; z++)
                CreateCell(x, z, startX + x, startZ + z);
        }
    }

    public void ScaleGrid()
    {
        if (isExpanding)
            return;

        int oldWeightX = weightX;
        int oldWeightZ = weightZ;

        GameObject[,] oldGridArray = gridArray;
        GridCell[,] oldGridCells = gridCells;

        weightX += 2;
        weightZ += 2;

        gridArray = new GameObject[weightX, weightZ];
        gridCells = new GridCell[weightX, weightZ];

        HashSet<BalanceObject> shiftedObjects = new HashSet<BalanceObject>();

        for (int x = 0; x < oldWeightX; x++)
        {
            for (int z = 0; z < oldWeightZ; z++)
            {
                int newX = x + 1;
                int newZ = z + 1;

                gridArray[newX, newZ] = oldGridArray[x, z];
                gridCells[newX, newZ] = oldGridCells[x, z];

                if (gridCells[newX, newZ] == null)
                    continue;

                gridCells[newX, newZ].Initialize(newX, newZ);

                BalanceObject occupyingObject = gridCells[newX, newZ].OccupyingObject;

                if (occupyingObject != null && shiftedObjects.Add(occupyingObject))
                    occupyingObject.OffsetGridCenter(1f, 1f);
            }
        }

        StartCoroutine(CreateNewBorderAnimated());

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayScaleGridSound();

        if (turnManager != null)
            turnManager.OffsetOptionPositionsZ(-1f);

        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();

        if (cameraController != null)
            cameraController.IncreaseSizeSlowly();

        if (tableBalanceController != null)
            tableBalanceController.RecalculateBalance();
    }

    private IEnumerator CreateNewBorderAnimated()
    {
        isExpanding = true;

        float localStartX = -(weightX - 1) * 0.5f;
        float localStartZ = -(weightZ - 1) * 0.5f;

        for (int x = 0; x < weightX; x++)
        {
            CreateCellAnimated(x, 0, localStartX + x, localStartZ);
            yield return new WaitForSeconds(cellSpawnDelay);
        }

        for (int z = 1; z < weightZ; z++)
        {
            CreateCellAnimated(weightX - 1, z, localStartX + weightX - 1, localStartZ + z);
            yield return new WaitForSeconds(cellSpawnDelay);
        }

        for (int x = weightX - 2; x >= 0; x--)
        {
            CreateCellAnimated(x, weightZ - 1, localStartX + x, localStartZ + weightZ - 1);
            yield return new WaitForSeconds(cellSpawnDelay);
        }

        for (int z = weightZ - 2; z > 0; z--)
        {
            CreateCellAnimated(0, z, localStartX, localStartZ + z);
            yield return new WaitForSeconds(cellSpawnDelay);
        }

        yield return new WaitForSeconds(cellSpawnDuration);

        isExpanding = false;
    }

    private void CreateCell(int xIndex, int zIndex, float localX, float localZ)
    {
        if (gridArray[xIndex, zIndex] != null)
            return;

        GameObject gridCell = Instantiate(gridPrefab, gridParent);
        gridCell.transform.localPosition = new Vector3(localX, 0f, localZ);
        gridCell.transform.localRotation = Quaternion.identity;

        gridArray[xIndex, zIndex] = gridCell;

        GridCell cell = gridCell.GetComponent<GridCell>();
        gridCells[xIndex, zIndex] = cell;

        if (cell != null)
            cell.Initialize(xIndex, zIndex);
    }

    private void CreateCellAnimated(int xIndex, int zIndex, float localX, float localZ)
    {
        if (gridArray[xIndex, zIndex] != null)
            return;

        GameObject gridCell = Instantiate(gridPrefab, gridParent);
        gridCell.transform.localPosition = new Vector3(localX, 0f, localZ);
        gridCell.transform.localRotation = Quaternion.identity;

        Vector3 targetScale = gridCell.transform.localScale;
        gridCell.transform.localScale = Vector3.zero;

        gridArray[xIndex, zIndex] = gridCell;

        GridCell cell = gridCell.GetComponent<GridCell>();
        gridCells[xIndex, zIndex] = cell;

        if (cell != null)
            cell.Initialize(xIndex, zIndex);

        StartCoroutine(ScaleCell(gridCell.transform, targetScale));
    }

    private IEnumerator ScaleCell(Transform cellTransform, Vector3 targetScale)
    {
        float elapsedTime = 0f;

        while (elapsedTime < cellSpawnDuration)
        {
            if (cellTransform == null)
                yield break;

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / cellSpawnDuration);
            t = t * t * (3f - 2f * t);

            cellTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);

            yield return null;
        }

        if (cellTransform != null)
            cellTransform.localScale = targetScale;
    }

    public bool CanPlaceObject(BalanceObject balanceObject, int startX, int startZ)
    {
        if (isExpanding)
            return false;

        for (int x = 0; x < balanceObject.HeightX; x++)
        {
            for (int z = 0; z < balanceObject.HeightY; z++)
            {
                int checkX = startX + x;
                int checkZ = startZ + z;

                if (checkX < 0 || checkZ < 0 || checkX >= weightX || checkZ >= weightZ)
                    return false;

                GridCell cell = gridCells[checkX, checkZ];

                if (cell == null || cell.IsOccupied)
                    return false;
            }
        }

        return true;
    }

    private void PlaceObject(BalanceObject balanceObject, int startX, int startZ)
    {
        for (int x = 0; x < balanceObject.HeightX; x++)
        {
            for (int z = 0; z < balanceObject.HeightY; z++)
                gridCells[startX + x, startZ + z].Occupy(balanceObject);
        }

        float centerX = startX + (balanceObject.HeightX - 1) * 0.5f;
        float centerZ = startZ + (balanceObject.HeightY - 1) * 0.5f;

        balanceObject.SetGridCenter(centerX, centerZ);
    }

    private Vector2Int GetStartCell(int hoveredX, int hoveredZ)
    {
        return new Vector2Int(hoveredX, hoveredZ);
    }

    private Vector3 GetPlacementPosition(BalanceObject balanceObject, int startX, int startZ)
    {
        int endX = Mathf.Clamp(startX + balanceObject.HeightX - 1, 0, weightX - 1);
        int endZ = Mathf.Clamp(startZ + balanceObject.HeightY - 1, 0, weightZ - 1);

        Vector3 firstCellPosition = gridCells[startX, startZ].transform.position;
        Vector3 lastCellPosition = gridCells[endX, endZ].transform.position;

        return (firstCellPosition + lastCellPosition) * 0.5f;
    }
}