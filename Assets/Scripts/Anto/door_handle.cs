using UnityEngine;

public class door_handle : MonoBehaviour
{
    [SerializeField] door_shelf door;

    private void OnMouseDown()
    {
        // If a HidingSpot is present, let the interaction system handle it
        if (TryGetComponent<HidingSpot>(out _))
            return;

        door.ToggleDoor();
    }
}
