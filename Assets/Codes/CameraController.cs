using UnityEngine;

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-100)]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 40f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float smoothZoomSpeed = 2f;
    [SerializeField] private float gridScaleDistanceIncrease = 2.5f;
    [SerializeField] private float dragThreshold = 5f;
    [SerializeField] private float minPitch = 10f;
    [SerializeField] private float maxPitch = 80f;

    private float distance;
    private float targetDistance;
    private float yaw;
    private float pitch;
    private Vector3 targetPoint;

    private Vector2 rightMouseDownPosition;
    private bool pendingObjectRotateClick;

    public bool ShouldRotatePlacedObject { get; private set; }

    private void Awake()
    {
        targetPoint = target != null ? target.position : Vector3.zero;

        Vector3 toTarget = targetPoint - transform.position;
        distance = toTarget.magnitude;
        targetDistance = distance;

        if (distance <= 0.001f)
            return;

        Quaternion lookRotation = Quaternion.LookRotation(toTarget);
        pitch = lookRotation.eulerAngles.x;
        yaw = lookRotation.eulerAngles.y;

        if (pitch > 180f)
            pitch -= 360f;
    }

    private void Update()
    {
        ShouldRotatePlacedObject = false;

        HandleZoomInput();
        HandleRotationInput();
        distance = Mathf.Lerp(distance, targetDistance, smoothZoomSpeed * Time.deltaTime);

        ApplyCameraTransform();
    }

    public void IncreaseSizeSlowly()
    {
        targetDistance = Mathf.Clamp(targetDistance + gridScaleDistanceIncrease, minDistance, maxDistance);
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) < 0.001f)
            return;

        targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed * 10f, minDistance, maxDistance);
    }

    private void HandleRotationInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            rightMouseDownPosition = Input.mousePosition;
            pendingObjectRotateClick = true;
        }

        if (Input.GetMouseButton(1))
        {
            Vector2 dragDelta = (Vector2)Input.mousePosition - rightMouseDownPosition;

            if (dragDelta.magnitude >= dragThreshold)
                pendingObjectRotateClick = false;

            if (!pendingObjectRotateClick)
            {
                yaw += Input.GetAxis("Mouse X") * rotationSpeed;
                pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
        }

        if (Input.GetMouseButtonUp(1) && pendingObjectRotateClick)
            ShouldRotatePlacedObject = true;
    }

    private void ApplyCameraTransform()
    {
        if (target != null)
            targetPoint = target.position;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = rotation * Vector3.back;
        transform.position = targetPoint + direction * distance;
        transform.rotation = rotation;
    }
}
