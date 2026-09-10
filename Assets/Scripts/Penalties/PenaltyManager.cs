using System.Collections.Generic;
using UnityEngine;

public class PenaltyManager : MonoBehaviour
{
    public static PenaltyManager Instance { get; private set; }

    [SerializeField] private List<MonoBehaviour> penaltyComponents = new List<MonoBehaviour>();

    private IPenalty activePenalty;

    public static event System.Action<PenaltyType> OnPenaltyApplied;

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

    public void TriggerRandomPenalty()
    {
        ClearCurrentPenalty();

        IPenalty lNewPenalty = GetRandomPenalty();
        if (lNewPenalty == null)
            return;

        activePenalty = lNewPenalty;
        activePenalty.Activate();
        OnPenaltyApplied?.Invoke(activePenalty.PenaltyType);
    }

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
        List<IPenalty> lAvailable = new List<IPenalty>();
        foreach (MonoBehaviour lMb in penaltyComponents)
        {
            if (lMb is IPenalty lPenalty)
            {
                lAvailable.Add(lPenalty);
            }
        }

        if (lAvailable.Count == 0)
            return null;

        return lAvailable[Random.Range(0, lAvailable.Count)];
    }
}
