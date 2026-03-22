using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("State")]
    public bool isOpen = false;

    [Header("Rotation")]
    public Vector3 closedEuler = Vector3.zero;
    public Vector3 openEuler = new Vector3(0f, 90f, 0f);
    public float rotateSpeed = 360f; // degrees per second

    private Quaternion _targetRotation;

    private void Start()
    {
        _targetRotation = Quaternion.Euler(isOpen ? openEuler : closedEuler);
        transform.localRotation = _targetRotation;
    }

    private void Update()
    {
        _targetRotation = Quaternion.Euler(isOpen ? openEuler : closedEuler);

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            _targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
    }
}