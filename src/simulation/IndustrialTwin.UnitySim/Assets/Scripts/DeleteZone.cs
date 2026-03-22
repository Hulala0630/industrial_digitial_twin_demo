using UnityEngine;

public class GateSortDeleteZone : MonoBehaviour
{
    [Header("References")]
    public GateController gateController;

    [Header("Options")]
    public bool deleteOnlyWhenGateOpen = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        if (gateController == null) return;

        if (!deleteOnlyWhenGateOpen || gateController.isOpen)
        {
            Rigidbody rb = other.attachedRigidbody;
            GameObject target = rb != null ? rb.gameObject : other.gameObject;

            Destroy(target);
        }
    }
}