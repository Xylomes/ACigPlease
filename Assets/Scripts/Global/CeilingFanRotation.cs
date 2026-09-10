using UnityEngine;

public class CeilingFanRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f;

    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
