using UnityEngine;

/// <summary>
/// Rotates the attached GameObject continuously around a specified axis at a given speed.
/// Designed for ceiling fans and similar constantly-rotating props.
/// </summary>
public class CeilingFanRotation : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second.")]
    [SerializeField] private float rotationSpeed = 360f;

    [Tooltip("Axis around which the object rotates.")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
