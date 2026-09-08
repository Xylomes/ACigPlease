using UnityEngine;

public class door_handle : MonoBehaviour
{
    [SerializeField] door_shelf door;

    private void OnMouseDown()
    {
        door.ToggleDoor();
    }
}
