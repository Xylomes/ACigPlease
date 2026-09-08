using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Ouvrir";
    [SerializeField] private Animator animator;

    [Header("Spawn Point")]
    [Tooltip("Transform enfant placé à l'intérieur du meuble. L'objet y apparaît quand on ouvre.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Door Animation (optional)")]
    [Tooltip("Si la cachette est une porte animée, glisser le door_shelf ici.")]
    [SerializeField] private door_shelf doorShelf;

    [Header("Inner Voice (optional)")]
    [Tooltip("Lignes de voix intérieure jouées quand la cachette est vide.")]
    [SerializeField] private InnerVoiceData emptySpotVoice;

    private const string OPEN_ANIM_PARAM = "IsOpen";

    private bool containsTarget;
    private string targetFlag;
    private bool isFunctionalTarget;
    private bool isOpen;
    private GameObject targetPrefab;
    private GameObject spawnedObject;

    public string InteractionPrompt => interactionPrompt;

    public bool IsInteractable
    {
        get
        {
            if (isOpen)
                return false;

            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.GameOver)
                return false;

            return true;
        }
    }

    /// <summary>Configure this spot for the current game phase. Does NOT animate doors.</summary>
    /// <param name="containsTarget">True if this spot holds the object the player is looking for.</param>
    /// <param name="targetFlag">The flag to set when the target is found (null for non-functional lighters).</param>
    /// <param name="isFunctionalTarget">True if this is the working lighter (no penalty on open).</param>
    /// <param name="prefabToSpawn">Prefab to instantiate at the spawn point when opened.</param>
    public void Setup(bool containsTarget, string targetFlag, bool isFunctionalTarget, GameObject prefabToSpawn)
    {
        this.containsTarget = containsTarget;
        this.targetFlag = targetFlag;
        this.isFunctionalTarget = isFunctionalTarget;
        this.targetPrefab = prefabToSpawn;
        isOpen = false;

        // Only destroy spawned object, do NOT toggle the door
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

        Open();

        if (containsTarget)
        {
            SpawnObject();

            if (!string.IsNullOrEmpty(targetFlag))
            {
                GameFlags.SetFlag(targetFlag);
            }
        }
        else
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

    /// <summary>Instantiate the target prefab at the spawn point.</summary>
    private void SpawnObject()
    {
        if (targetPrefab == null)
            return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        spawnedObject = Instantiate(targetPrefab, pos, rot);
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
    }

    /// <summary>Visually close and reset this hiding spot. Only toggles the door if it was actually open.</summary>
    public void Close()
    {
        // Only toggle the door if it was open — avoids opening closed doors during setup
        if (isOpen)
        {
            if (doorShelf != null)
            {
                doorShelf.ToggleDoor();
            }
            if (animator != null)
            {
                animator.SetBool(OPEN_ANIM_PARAM, false);
            }
        }

        isOpen = false;

        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }
}
