using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Ouvrir";
    [SerializeField] private Animator animator;

    private const string OPEN_ANIM_PARAM = "IsOpen";

    private bool containsTarget;
    private string targetFlag;
    private bool isFunctionalTarget;
    private bool isOpen;

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

    /// <summary>Configure this spot for the current game phase.</summary>
    /// <param name="containsTarget">True if this spot holds the object the player is looking for.</param>
    /// <param name="targetFlag">The flag to set when the target is found (null if this is a non-functional lighter).</param>
    /// <param name="isFunctionalTarget">True if this is the working lighter (no penalty on open).</param>
    public void Setup(bool containsTarget, string targetFlag, bool isFunctionalTarget)
    {
        this.containsTarget = containsTarget;
        this.targetFlag = targetFlag;
        this.isFunctionalTarget = isFunctionalTarget;
        isOpen = false;
        Close();
    }

    public void Interact()
    {
        if (!IsInteractable)
            return;

        Open();

        if (containsTarget && !string.IsNullOrEmpty(targetFlag))
        {
            GameFlags.SetFlag(targetFlag);
        }
        else if (!containsTarget || !isFunctionalTarget)
        {
            // Empty hiding spot or non-functional lighter: trigger a penalty
            // Non-functional lighters do NOT trigger penalties, so only penalize empty spots
            if (!containsTarget)
            {
                PenaltyManager.Instance?.TriggerRandomPenalty();
            }
        }
    }

    /// <summary>Visually open this hiding spot.</summary>
    public void Open()
    {
        isOpen = true;
        if (animator != null)
        {
            animator.SetBool(OPEN_ANIM_PARAM, true);
        }
    }

    /// <summary>Visually close and reset this hiding spot.</summary>
    public void Close()
    {
        isOpen = false;
        if (animator != null)
        {
            animator.SetBool(OPEN_ANIM_PARAM, false);
        }
    }
}
