using System.Collections;
using UnityEngine;

public class DestructionGround : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0.15f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Object"))
            return;

        StartCoroutine(DisableObject(other.gameObject));
    }

    private IEnumerator DisableObject(GameObject objectToDisable)
    {
        if (objectToDisable == null)
            yield break;

        Collider objectCollider = objectToDisable.GetComponent<Collider>();

        if (objectCollider != null)
            objectCollider.enabled = false;

        Rigidbody objectRigidbody = objectToDisable.GetComponent<Rigidbody>();

        if (objectRigidbody != null)
        {
            objectRigidbody.linearVelocity = Vector3.zero;
            objectRigidbody.angularVelocity = Vector3.zero;
            objectRigidbody.isKinematic = true;
        }

        yield return new WaitForSeconds(destroyDelay);

        if (EffectManager.Instance != null)
            EffectManager.Instance.PlayDisappearEffect(objectToDisable.transform.position);

        objectToDisable.SetActive(false);
    }
}