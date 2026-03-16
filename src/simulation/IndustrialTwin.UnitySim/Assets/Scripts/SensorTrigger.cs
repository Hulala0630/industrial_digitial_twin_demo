using UnityEngine;

public class SensorTrigger : MonoBehaviour
{
    public bool occupied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        occupied = true;
        Debug.Log("Sensor occupied");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        occupied = false;
        Debug.Log("Sensor free");
    }
}