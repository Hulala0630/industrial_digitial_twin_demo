using System.Collections.Generic;
using UnityEngine;

public class ConveyorZone : MonoBehaviour
{
    public bool running = true;
    public Vector3 moveDirection = Vector3.left;
    public float speed = 1.5f;

    private readonly List<Rigidbody> itemsOnConveyor = new();

    private void Update()
    {
        for (int i = itemsOnConveyor.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = itemsOnConveyor[i];

            if (rb == null)
            {
                itemsOnConveyor.RemoveAt(i);
                continue;
            }

            if (running)
            {
                Vector3 targetVelocity = moveDirection.normalized * speed;
                rb.linearVelocity = new Vector3(
                    targetVelocity.x,
                    rb.linearVelocity.y,
                    targetVelocity.z
                );
            }
            else
            {
                rb.linearVelocity = new Vector3(
                    0f,
                    rb.linearVelocity.y,
                    0f
                );
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !itemsOnConveyor.Contains(rb))
        {
            itemsOnConveyor.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            itemsOnConveyor.Remove(rb);
        }
    }
}