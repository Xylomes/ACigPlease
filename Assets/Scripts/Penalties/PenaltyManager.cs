using System.Collections.Generic;
using UnityEngine;

public class PenaltyManager : MonoBehaviour
{
    public static PenaltyManager Instance { get; private set; }

    [Header("Penalty Components")]
    [SerializeField] private List<MonoBehaviour> penaltyComponents = new List<MonoBehaviour>();

    private IPenalty activePenalty;

    /// <summary>Fired when a penalty is applied. Passes the penalty type.</summary>
    public static event System.Action<PenaltyType> OnPenaltyApplied;

    /// <summary>Fired when the current penalty is cleared.</summary>
    public static event System.Action OnPenaltyCleared;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>Clear the current penalty, then activate a new random one.</summary>
    public void TriggerRandomPenalty()
    {
        ClearCurrentPenalty();

        IPenalty newPenalty = GetRandomPenalty();
        if (newPenalty == null)
            return;

        activePenalty = newPenalty;
        activePenalty.Activate();
        OnPenaltyApplied?.Invoke(activePenalty.PenaltyType);
    }

    /// <summary>Deactivate the current penalty if one is active.</summary>
    public void ClearCurrentPenalty()
    {
        if (activePenalty != null)
        {
            activePenalty.Deactivate();
            activePenalty = null;
            OnPenaltyCleared?.Invoke();
        }
    }

    private IPenalty GetRandomPenalty()
    {
        List<IPenalty> available = new List<IPenalty>();
        foreach (MonoBehaviour mb in penaltyComponents)
        {
            if (mb is IPenalty penalty)
            {
                available.Add(penalty);
            }
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }
}
