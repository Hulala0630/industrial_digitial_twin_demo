using UnityEngine;

public class GateGuideZone : MonoBehaviour
{
    public GateController gateController;

    public Vector3 closedDirection = new Vector3(1f, 0f, 0f);
    public Vector3 openDirection = new Vector3(1f, 0f, 1f);

    public float guideSpeed = 1.5f;
    public float blend = 8f;
    public bool keepYVelocity = true;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Vector3 dir = (gateController != null && gateController.isOpen)
            ? openDirection.normalized
            : closedDirection.normalized;

        Vector3 targetVelocity = dir * guideSpeed;

        if (keepYVelocity)
            targetVelocity.y = rb.linearVelocity.y;
        else
            targetVelocity.y = 0f;

        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            targetVelocity,
            blend * Time.deltaTime
        );
    }
}