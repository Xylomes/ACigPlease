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
    }

    /// <summary>Randomly place cigarettes in one of the hiding spots.</summary>
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

    /// <summary>Close all spots, pick 3 random ones for lighters (1 functional).</summary>
    public void SetupLighterPhase()
    {
        CloseAllSpots();

        // Pick 3 random spot indices from the 8
        lighterSpotIndices = PickRandomIndices(hidingSpots.Count, NUMBER_OF_LIGHTERS);

        // Enable only the 3 chosen spots, disable the rest
        for (int i = 0; i < hidingSpots.Count; i++)
        {
            hidingSpots[i].gameObject.SetActive(lighterSpotIndices.Contains(i));
        }

        workingLighterIndex = Random.Range(0, NUMBER_OF_LIGHTERS);

        for (int i = 0; i < lighterSpotIndices.Count; i++)
        {
            HidingSpot spot = hidingSpots[lighterSpotIndices[i]];
            bool isFunctional = i == workingLighterIndex;
            spot.Setup(
                containsTarget: true,
                targetFlag: isFunctional ? GameFlags.WORKING_LIGHTER_FOUND : null,
                isFunctionalTarget: isFunctional,
                prefabToSpawn: lighterPrefab
            );
        }
    }

    /// <summary>Close and reset every hiding spot.</summary>
    private void CloseAllSpots()
    {
        foreach (HidingSpot spot in hidingSpots)
        {
            if (spot != null)
            {
                spot.Close();
            }
        }
    }

    /// <summary>Make sure all spots are active (for cigarette phase).</summary>
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

    /// <summary>Pick count unique random indices from [0, max).</summary>
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
