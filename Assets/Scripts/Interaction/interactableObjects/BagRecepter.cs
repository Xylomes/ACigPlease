using UnityEngine;

public class BagRecepter : MonoBehaviour, IInteractable
{
    private const string BAG_TAG = "Bag";
    private const string INTERACT_PROMPT = "Verser le sac";

    [SerializeField] private StockageBarrel stockageBarrel;
    [SerializeField] private Transform pourPoint;

    private Bag bagInZone;

    public string InteractionPrompt => INTERACT_PROMPT;
    public bool IsInteractable => bagInZone != null && bagInZone.IsHeld && !bagInZone.IsPouring;
    public Transform PourPoint => pourPoint;

    public void Interact()
    {
        if (bagInZone != null && bagInZone.IsHeld && !bagInZone.IsPouring)
        {
            bagInZone.StartPouring(this);
        }
    }

    private void OnTriggerStay(Collider pBag)
    {
        if (!pBag.CompareTag(BAG_TAG))
        {
            return;
        }

        Bag bag = pBag.GetComponent<Bag>();

        if (bag.IsHeld && !bag.IsPouring)
        {
            bagInZone = bag;
        }
        else if (bag == bagInZone)
        {
            bagInZone = null;
        }
    }

    private void OnTriggerExit(Collider pBag)
    {
        if (bagInZone != null && pBag.transform == bagInZone.transform)
        {
            bagInZone = null;
        }
    }

    public void ReceiveContent(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (!stockageBarrel.TryAddProductIntoBarrel())
            {
                break;
            }
        }
    }
}
