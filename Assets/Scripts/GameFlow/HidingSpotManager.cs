using System.Collections.Generic;
using UnityEngine;

public class HidingSpotManager : MonoBehaviour
{
    public static HidingSpotManager Instance { get; private set; }

    [SerializeField] private List<HidingSpot> hidingSpots = new List<HidingSpot>();

    [SerializeField] private GameObject cigarettePrefab;
    [SerializeField] private GameObject lighterPrefab;

    private const int NUMBER_OF_CIGARETTE_SPOTS = 8;
    private const int NUMBER_OF_LIGHTERS = 3;

    private int cigaretteSpotIndex = -1;
    private int workingLighterIndex = -1;
    private List<int> lighterSpotIndices = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HashSet<GameObject> lSeen = new HashSet<GameObject>();
        for (int i = hidingSpots.Count - 1; i >= 0; i--)
        {
            if (hidingSpots[i] == null || !lSeen.Add(hidingSpots[i].gameObject))
            {
                hidingSpots.RemoveAt(i);
            }
        }
    }

    public void SetupCigarettePhase()
    {
        EnableAllSpots();

        cigaretteSpotIndex = Random.Range(0, hidingSpots.Count);

        for (int i = 0; i < hidingSpots.Count; i++)
        {
            HidingSpot lSpot = hidingSpots[i];
            lSpot.Setup(
                pContainsTarget: i == cigaretteSpotIndex,
                pTargetFlag: GameFlags.CIGARETTES_FOUND,
                pIsFunctionalTarget: true,
                pPrefabToSpawn: i == cigaretteSpotIndex ? cigarettePrefab : null
            );
        }
    }

    public void SetupLighterPhase()
    {
        CloseAllSpots();

        lighterSpotIndices = PickRandomIndices(hidingSpots.Count, NUMBER_OF_LIGHTERS);

        workingLighterIndex = Random.Range(0, NUMBER_OF_LIGHTERS);

        for (int i = 0; i < hidingSpots.Count; i++)
        {
            int lLighterListPosition = lighterSpotIndices.IndexOf(i);
            bool lHasLighter = lLighterListPosition != -1;
            bool lIsFunctional = lHasLighter && lLighterListPosition == workingLighterIndex;

            hidingSpots[i].Setup(
                pContainsTarget: lHasLighter,
                pTargetFlag: lIsFunctional ? GameFlags.WORKING_LIGHTER_FOUND : null,
                pIsFunctionalTarget: lIsFunctional,
                pPrefabToSpawn: lHasLighter ? lighterPrefab : null
            );
        }
    }

    public void CloseAllSpots()
    {
        foreach (HidingSpot lSpot in hidingSpots)
        {
            if (lSpot != null)
            {
                lSpot.Close();
            }
        }
    }

    public void ResetAllSpots()
    {
        foreach (HidingSpot lSpot in hidingSpots)
        {
            if (lSpot != null)
            {
                lSpot.ForceClose();
                lSpot.Setup(
                    pContainsTarget: false,
                    pTargetFlag: null,
                    pIsFunctionalTarget: false,
                    pPrefabToSpawn: null
                );
                lSpot.gameObject.SetActive(true);
            }
        }
    }

    private void EnableAllSpots()
    {
        foreach (HidingSpot lSpot in hidingSpots)
        {
            if (lSpot != null)
            {
                lSpot.gameObject.SetActive(true);
            }
        }
    }

    private List<int> PickRandomIndices(int pMax, int pCount)
    {
        List<int> lPool = new List<int>();
        for (int i = 0; i < pMax; i++) lPool.Add(i);

        List<int> lResult = new List<int>();
        for (int i = 0; i < pCount && lPool.Count > 0; i++)
        {
            int lPickIndex = Random.Range(0, lPool.Count);
            lResult.Add(lPool[lPickIndex]);
            lPool.RemoveAt(lPickIndex);
        }
        return lResult;
    }
}
