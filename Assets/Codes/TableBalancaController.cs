using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TableBalanceController : MonoBehaviour
{
    [SerializeField] private GridController gridController;
    [SerializeField] private GameObject tableRoot;

    [Header("Balance")]
    [SerializeField] private float torqueToAngle = 1.5f;
    [SerializeField] private float maxAngle = 15f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Lose")]
    [SerializeField] private float loseAngleMultiplier = 3f;
    [SerializeField] private float loseRotationSpeed = 4f;
    [SerializeField] private float objectReleaseDelay = 0.35f;
    [SerializeField] private float gameOverDelay = 0.65f;
    [SerializeField] private UnityEvent onGameLost;

    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private bool gameLost;

    public bool GameLost => gameLost;


    private void Awake()
    {
        if (tableRoot == null)
        {
            Debug.LogError("Table Root is not assigned.", this);
            enabled = false;
            return;
        }

        initialRotation = tableRoot.transform.localRotation;
        targetRotation = initialRotation;
    }

    private void Update()
    {
        float currentRotationSpeed = gameLost ? loseRotationSpeed : rotationSpeed;

        tableRoot.transform.localRotation = Quaternion.Slerp(
            tableRoot.transform.localRotation,
            targetRotation,
            currentRotationSpeed * Time.deltaTime
        );
    }

    public void RecalculateBalance()
    {
        if (gameLost)
            return;

        if (gridController == null || gridController.gridCells == null || tableRoot == null)
            return;

        HashSet<BalanceObject> countedObjects = new HashSet<BalanceObject>();

        float xTorque = 0f;
        float zTorque = 0f;

        float gridCenterX = (gridController.WeightX - 1) * 0.5f;
        float gridCenterZ = (gridController.WeightZ - 1) * 0.5f;

        for (int x = 0; x < gridController.WeightX; x++)
        {
            for (int z = 0; z < gridController.WeightZ; z++)
            {
                GridCell cell = gridController.gridCells[x, z];

                if (cell == null || !cell.IsOccupied)
                    continue;

                BalanceObject balanceObject = cell.OccupyingObject;

                if (balanceObject == null || !countedObjects.Add(balanceObject))
                    continue;

                float objectWeight = (int)balanceObject.Weight;
                float distanceX = balanceObject.GridCenter.x - gridCenterX;
                float distanceZ = balanceObject.GridCenter.y - gridCenterZ;

                xTorque += (distanceZ*0.6f) * objectWeight;
                zTorque += (distanceX*0.6f) * objectWeight;
            }
        }

        float calculatedXAngle = xTorque * torqueToAngle;
        float calculatedZAngle = -zTorque * torqueToAngle;

        if (Mathf.Abs(calculatedXAngle) > maxAngle )
        {
            LoseGame(calculatedXAngle, 0);
            return;
        }
        if ( Mathf.Abs(calculatedZAngle) > maxAngle)
        {
            LoseGame(0, calculatedZAngle);
            return;
        }
        float targetXAngle = Mathf.Clamp(calculatedXAngle, -maxAngle, maxAngle);
        float targetZAngle = Mathf.Clamp(calculatedZAngle, -maxAngle, maxAngle);

        targetRotation = initialRotation * Quaternion.Euler(targetXAngle, 0f, targetZAngle);
    }

    private void LoseGame(float calculatedXAngle, float calculatedZAngle)
    {
        if (gameLost)
            return;

        gameLost = true;

        float loseXAngle = GetLoseAngle(calculatedXAngle);
        float loseZAngle = GetLoseAngle(calculatedZAngle);

        targetRotation = initialRotation * Quaternion.Euler(loseXAngle, 0f, loseZAngle);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBalanceGoneSound();

        StartCoroutine(LoseSequence());
    }
    private IEnumerator LoseSequence()
    {
        yield return new WaitForSeconds(objectReleaseDelay);

        ReleaseObjects();

        yield return new WaitForSeconds(gameOverDelay);

        onGameLost?.Invoke();
    }
    private void ReleaseObjects()
    {
        BalanceObject[] balanceObjects = tableRoot.GetComponentsInChildren<BalanceObject>(true);

        foreach (BalanceObject balanceObject in balanceObjects)
        {
            if (balanceObject == null)
                continue;

            balanceObject.SetPreviewColliders(true);

            Rigidbody[] rigidbodies = balanceObject.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody objectRigidbody in rigidbodies)
            {
                if (objectRigidbody == null)
                    continue;

                objectRigidbody.transform.SetParent(null, true);
                objectRigidbody.useGravity = true;
                objectRigidbody.isKinematic = false;
                objectRigidbody.WakeUp();
            }
        }
    }

    private float GetLoseAngle(float calculatedAngle)
    {
        if (Mathf.Approximately(calculatedAngle, 0f))
            return 0f;

        float minimumLoseAngle = maxAngle * loseAngleMultiplier;
        float angleMagnitude = Mathf.Max(Mathf.Abs(calculatedAngle), minimumLoseAngle);

        return Mathf.Sign(calculatedAngle) * angleMagnitude;
    }

    private IEnumerator ReleaseObjectsCoroutine()
    {
        yield return new WaitForSeconds(objectReleaseDelay);

        BalanceObject[] balanceObjects = tableRoot.GetComponentsInChildren<BalanceObject>(true);

        foreach (BalanceObject balanceObject in balanceObjects)
        {
            if (balanceObject == null)
                continue;

            balanceObject.SetPreviewColliders(true);

            Rigidbody[] rigidbodies = balanceObject.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody objectRigidbody in rigidbodies)
            {
                if (objectRigidbody == null)
                    continue;

                objectRigidbody.transform.SetParent(null, true);
                objectRigidbody.useGravity = true;
                objectRigidbody.isKinematic = false;
                objectRigidbody.WakeUp();
            }
        }
    }
}