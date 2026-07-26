using UnityEngine;

public class FakeFogMover : MonoBehaviour
{
    [SerializeField] private Vector2 moveDirection = new Vector2(0.15f, 0.08f);
    [SerializeField] private float moveSpeed = 0.15f;
    [SerializeField] private float rotationSpeed = 2f;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        float x = Mathf.Sin(Time.time * moveSpeed) * moveDirection.x;
        float z = Mathf.Cos(Time.time * moveSpeed * 0.8f) * moveDirection.y;

        transform.localPosition = startPosition + new Vector3(x, 0f, z);
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}