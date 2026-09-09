using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private string openPrompt = "Ouvrir";
    [SerializeField] private string closePrompt = "Fermer";
    [SerializeField] private Animator animator;

    [Header("Spawn Point")]
    [Tooltip("Transform enfant placé à l'intérieur du meuble. L'objet y apparaît quand on ouvre.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Door Animation (optional)")]
    [Tooltip("Si la cachette est une porte animée en rotation, glisser le door_shelf ici.")]
    [SerializeField] private door_shelf doorShelf;

    [Header("Drawer Slide (optional)")]
    [Tooltip("Si la cachette est un tiroir qui coulisse, glisser le drawer_slide ici.")]
    [SerializeField] private drawer_slide drawerSlide;

    [Header("Inner Voice (optional)")]
    [Tooltip("Lignes de voix intérieure jouées quand la cachette est vide.")]
    [SerializeField] private InnerVoiceData emptySpotVoice;

    private const string OPEN_ANIM_PARAM = "IsOpen";

    private bool containsTarget;
    private string targetFlag;
    private bool isFunctionalTarget;
    private bool isOpen;
    public bool IsOpen => isOpen;
    private GameObject targetPrefab;
    private GameObject spawnedObject;
    private bool itemWasTaken;

    public string InteractionPrompt => isOpen ? closePrompt : openPrompt;

    public bool IsInteractable
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.GameOver)
                return false;

            return true;
        }
    }

    /// <summary>Configure this spot for the current game phase. Closes the door if it was open.</summary>
    /// <param name="containsTarget">True if this spot holds the object the player is looking for.</param>
    /// <param name="targetFlag">The flag to set when the target is found (null for non-functional lighters).</param>
    /// <param name="isFunctionalTarget">True if this is the working lighter (no penalty on open).</param>
    /// <param name="prefabToSpawn">Prefab to instantiate at the spawn point when opened.</param>
    public void Setup(bool containsTarget, string targetFlag, bool isFunctionalTarget, GameObject prefabToSpawn)
    {
        // Close the door/drawer visually if it was open before reconfiguring
        if (isOpen)
        {
            if (doorShelf != null)
            {
                doorShelf.ToggleDoor();
            }
            if (drawerSlide != null)
            {
                drawerSlide.ToggleDoor();
            }
            if (animator != null)
            {
                animator.SetBool(OPEN_ANIM_PARAM, false);
            }
        }

        this.containsTarget = containsTarget;
        this.targetFlag = targetFlag;
        this.isFunctionalTarget = isFunctionalTarget;
        this.targetPrefab = prefabToSpawn;
        isOpen = false;
        itemWasTaken = false;

        // Destroy any previously spawned object (still in the spot or picked up)
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }

    public void Interact()
    {
        if (!IsInteractable)
            return;

        // If already open, close it (player-initiated close preserves spawned items)
        if (isOpen)
        {
            Close(destroySpawned: false);
            return;
        }

        Open();

        if (containsTarget && !itemWasTaken)
        {
            SpawnObject();
        }
        else if (!containsTarget && !itemWasTaken)
        {
            // Empty hiding spot: trigger a random penalty
            PenaltyManager.Instance?.TriggerRandomPenalty();

            // Show inner voice line for empty spot
            if (InnerVoiceManager.Instance != null)
            {
                if (emptySpotVoice != null)
                {
                    InnerVoiceManager.Instance.Show(emptySpotVoice);
                }
                else
                {
                    string[] defaultLines = {
                        "<shake>Rien ici...</shake>",
                        "<shake>Merde. Pas là non plus.</shake>",
                        "<shake>Putain, où sont mes clopes ?</shake>",
                        "<shake>Vide. Encore.</shake>",
                        "<shake>Je sens que je perds la tête.</shake>"
                    };
                    InnerVoiceManager.Instance.Show(defaultLines[Random.Range(0, defaultLines.Length)]);
                }
            }
        }
    }

    /// <summary>Instantiate the target prefab at the spawn point if it hasn't been taken yet.</summary>
    private void SpawnObject()
    {
        if (targetPrefab == null || itemWasTaken)
            return;

        // Don't spawn a duplicate if the item is still at the spawn point
        if (spawnedObject != null)
            return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        spawnedObject = Instantiate(targetPrefab, pos, rot);

        // Only parent to the spawn point for drawers so the item slides along with it.
        // Doors and shelves don't move the spawn point, so leave the item in world space.
        if (drawerSlide != null)
        {
            Transform parent = spawnPoint != null ? spawnPoint : transform;
            spawnedObject.transform.SetParent(parent, true);
        }

        TargetItem targetItem = spawnedObject.GetComponent<TargetItem>();
        if (targetItem != null)
        {
            targetItem.Init(targetFlag);
            targetItem.OnPickedUp += HandleItemPickedUp;
        }
    }

    private void HandleItemPickedUp()
    {
        spawnedObject = null;
        itemWasTaken = true;
    }

    /// <summary>Visually open this hiding spot. Triggers door animation if present.</summary>
    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;
        if (animator != null)
        {
            animator.SetBool(OPEN_ANIM_PARAM, true);
        }
        if (doorShelf != null)
        {
            doorShelf.ToggleDoor();
        }
        if (drawerSlide != null)
        {
            drawerSlide.ToggleDoor();
        }
    }

    /// <summary>Visually close and reset this hiding spot. Only toggles the door if it was actually open.</summary>
    /// <param name="destroySpawned">If true, destroys the spawned object (used during phase transitions). Player-initiated close preserves items.</param>
    public void Close(bool destroySpawned = true)
    {
        // Only toggle the door if it was open — avoids opening closed doors during setup
        if (isOpen)
        {
            if (doorShelf != null)
            {
                doorShelf.ToggleDoor();
            }
            if (drawerSlide != null)
            {
                drawerSlide.ToggleDoor();
            }
            if (animator != null)
            {
                animator.SetBool(OPEN_ANIM_PARAM, false);
            }
        }

        isOpen = false;

        if (destroySpawned && spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }

    /// <summary>Force-close the door/drawer to its closed state regardless of internal sync issues. Used during full game reset.</summary>
    public void ForceClose()
    {
        if (doorShelf != null)
        {
            doorShelf.ForceClose();
        }
        if (drawerSlide != null)
        {
            drawerSlide.ForceClose();
        }
        if (animator != null)
        {
            animator.SetBool(OPEN_ANIM_PARAM, false);
        }

        isOpen = false;

        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }
}
