using UnityEngine;

public class GateController : MonoBehaviour
{
    public bool isOpen = false;

    public Vector3 closedEuler = Vector3.zero;
    public Vector3 openEuler = new Vector3(0, 45, 0);
    public float rotateSpeed = 8f;

    private void Update()
    {
        Quaternion targetRotation = Quaternion.Euler(isOpen ? openEuler : closedEuler);

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
    }
}
