using System.Collections.Generic;
using UnityEngine;

public class HidingSpotManager : MonoBehaviour
{
    public static HidingSpotManager Instance { get; private set; }

    [Header("Cigarette Phase")]
    [SerializeField] private List<HidingSpot> cigaretteHidingSpots = new List<HidingSpot>();
    [SerializeField] private GameObject cigarettePrefab;

    [Header("Lighter Phase")]
    [SerializeField] private List<HidingSpot> lighterHidingSpots = new List<HidingSpot>();
    [SerializeField] private GameObject lighterPrefab;

    private const int NUMBER_OF_LIGHTERS = 3;

    private int cigaretteSpotIndex = -1;
    private int workingLighterIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>Randomly place cigarettes in one of the 8 hiding spots.</summary>
    public void SetupCigarettePhase()
    {
        CloseAllSpots(cigaretteHidingSpots);

        cigaretteSpotIndex = Random.Range(0, cigaretteHidingSpots.Count);

        for (int i = 0; i < cigaretteHidingSpots.Count; i++)
        {
            HidingSpot spot = cigaretteHidingSpots[i];
            spot.Setup(
                containsTarget: i == cigaretteSpotIndex,
                targetFlag: GameFlags.CIGARETTES_FOUND,
                isFunctionalTarget: true
            );
        }
    }

    /// <summary>Close all cigarette spots and spawn 3 lighters (1 functional).</summary>
    public void SetupLighterPhase()
    {
        CloseAllSpots(cigaretteHidingSpots);

        for (int i = 0; i < lighterHidingSpots.Count && i < NUMBER_OF_LIGHTERS; i++)
        {
            lighterHidingSpots[i].gameObject.SetActive(true);
        }

        workingLighterIndex = Random.Range(0, Mathf.Min(NUMBER_OF_LIGHTERS, lighterHidingSpots.Count));

        for (int i = 0; i < lighterHidingSpots.Count && i < NUMBER_OF_LIGHTERS; i++)
        {
            HidingSpot spot = lighterHidingSpots[i];
            spot.Setup(
                containsTarget: true,
                targetFlag: i == workingLighterIndex ? GameFlags.WORKING_LIGHTER_FOUND : null,
                isFunctionalTarget: i == workingLighterIndex
            );
        }

        // Disable any extra lighter spots beyond the 3 we use
        for (int i = NUMBER_OF_LIGHTERS; i < lighterHidingSpots.Count; i++)
        {
            lighterHidingSpots[i].gameObject.SetActive(false);
        }
    }

    /// <summary>Visually close and deactivate every spot in the given list.</summary>
    private void CloseAllSpots(List<HidingSpot> spots)
    {
        foreach (HidingSpot spot in spots)
        {
            if (spot != null)
            {
                spot.Close();
            }
        }
    }
}
