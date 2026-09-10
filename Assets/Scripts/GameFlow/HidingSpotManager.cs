using System.Collections.Generic;
using UnityEngine;

public class HidingSpotManager : MonoBehaviour
{
    public static HidingSpotManager Instance { get; private set; }

    [Header("Hiding Spots (8)")]
    [Tooltip("Les 8 cachettes de la scène. Le paquet de clopes et les briquets y spawnent.")]
    [SerializeField] private List<HidingSpot> hidingSpots = new List<HidingSpot>();

    [Header("Prefabs")]
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

        HashSet<GameObject> seen = new HashSet<GameObject>();
        for (int i = hidingSpots.Count - 1; i >= 0; i--)
        {
            if (hidingSpots[i] == null || !seen.Add(hidingSpots[i].gameObject))
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
            HidingSpot spot = hidingSpots[i];
            spot.Setup(
                containsTarget: i == cigaretteSpotIndex,
                targetFlag: GameFlags.CIGARETTES_FOUND,
                isFunctionalTarget: true,
                prefabToSpawn: i == cigaretteSpotIndex ? cigarettePrefab : null
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
            int lighterListPosition = lighterSpotIndices.IndexOf(i);
            bool hasLighter = lighterListPosition != -1;
            bool isFunctional = hasLighter && lighterListPosition == workingLighterIndex;

            hidingSpots[i].Setup(
                containsTarget: hasLighter,
                targetFlag: isFunctional ? GameFlags.WORKING_LIGHTER_FOUND : null,
                isFunctionalTarget: isFunctional,
                prefabToSpawn: hasLighter ? lighterPrefab : null
            );
        }
    }

    public void CloseAllSpots()
    {
        foreach (HidingSpot spot in hidingSpots)
        {
            if (spot != null)
            {
                spot.Close();
            }
        }
    }

    public void ResetAllSpots()
    {
        foreach (HidingSpot spot in hidingSpots)
        {
            if (spot != null)
            {
                spot.ForceClose();
                spot.Setup(
                    containsTarget: false,
                    targetFlag: null,
                    isFunctionalTarget: false,
                    prefabToSpawn: null
                );
                spot.gameObject.SetActive(true);
            }
        }
    }

    private void EnableAllSpots()
    {
        foreach (HidingSpot spot in hidingSpots)
        {
            if (spot != null)
            {
                spot.gameObject.SetActive(true);
            }
        }
    }

    private List<int> PickRandomIndices(int max, int count)
    {
        List<int> pool = new List<int>();
        for (int i = 0; i < max; i++) pool.Add(i);

        List<int> result = new List<int>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int pickIndex = Random.Range(0, pool.Count);
            result.Add(pool[pickIndex]);
            pool.RemoveAt(pickIndex);
        }
        return result;
    }
}