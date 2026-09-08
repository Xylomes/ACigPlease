using UnityEngine;

public class TargetItem : MonoBehaviour
{
    private string targetFlag;
    private bool hasFlagToSet;
    private Rigidbody rb;
    private bool isGrabbed;

    public void Init(string flagToSetOnPickup)
    {
        targetFlag = flagToSetOnPickup;
        hasFlagToSet = !string.IsNullOrEmpty(flagToSetOnPickup);
        rb = GetComponent<Rigidbody>();
    }

    public void OnGrabbed(Transform holdPoint)
    {
        if (isGrabbed)
            return;

        isGrabbed = true;

        if (hasFlagToSet)
        {
            GameFlags.SetFlag(targetFlag);
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
}
