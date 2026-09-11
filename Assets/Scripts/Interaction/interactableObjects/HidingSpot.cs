using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private string openPrompt = "Ouvrir";
    [SerializeField] private string closePrompt = "Fermer";
    [SerializeField] private Animator animator;

    [SerializeField] private Transform spawnPoint;

    [SerializeField] private door_shelf doorShelf;

    [SerializeField] private drawer_slide drawerSlide;

    [SerializeField] private InnerVoiceData emptySpotVoice;
    [SerializeField] private AudioClip[] emptySpotSounds;
    [Range(0f, 1f)]
    [SerializeField] private float emptySpotSoundChance = 0.3f;

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

    public void Setup(bool pContainsTarget, string pTargetFlag, bool pIsFunctionalTarget, GameObject pPrefabToSpawn)
    {
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

        this.containsTarget = pContainsTarget;
        this.targetFlag = pTargetFlag;
        this.isFunctionalTarget = pIsFunctionalTarget;
        this.targetPrefab = pPrefabToSpawn;
        isOpen = false;
        itemWasTaken = false;

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

        if (isOpen)
        {
            Close(pDestroySpawned: false);
            return;
        }

        Open();

        if (containsTarget && !itemWasTaken)
        {
            SpawnObject();
        }
        else if (!containsTarget && !itemWasTaken)
        {
            PenaltyManager.Instance?.TriggerRandomPenalty();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayRandomSFXWithChance(emptySpotSounds, emptySpotSoundChance);
            }

            if (InnerVoiceManager.Instance != null)
            {
                if (emptySpotVoice != null)
                {
                    InnerVoiceManager.Instance.Show(emptySpotVoice);
                }
                else
                {
                    string[] lDefaultLines = {
                        "<shake>Rien ici...</shake>",
                        "<shake>Merde. Pas là non plus.</shake>",
                        "<shake>Putain, où sont mes clopes ?</shake>",
                        "<shake>Vide. Encore.</shake>",
                        "<shake>Je sens que je perds la tête.</shake>"
                    };
                    InnerVoiceManager.Instance.Show(lDefaultLines[Random.Range(0, lDefaultLines.Length)]);
                }
            }
        }
    }

    private void SpawnObject()
    {
        if (targetPrefab == null || itemWasTaken)
            return;

        if (spawnedObject != null)
            return;

        Vector3 lPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion lRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        spawnedObject = Instantiate(targetPrefab, lPos, lRot);

        if (drawerSlide != null)
        {
            Transform lParent = spawnPoint != null ? spawnPoint : transform;
            spawnedObject.transform.SetParent(lParent, true);
        }

        TargetItem lTargetItem = spawnedObject.GetComponent<TargetItem>();
        if (lTargetItem != null)
        {
            lTargetItem.Init(targetFlag);
            lTargetItem.OnPickedUp += HandleItemPickedUp;
        }
    }

    private void HandleItemPickedUp()
    {
        spawnedObject = null;
        itemWasTaken = true;
    }

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

    public void Close(bool pDestroySpawned = true)
    {
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

        if (pDestroySpawned && spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }

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
