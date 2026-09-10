using UnityEngine;
using UnityEngine.Networking;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
public class DataCollector : MonoBehaviour
{
    public string Date;
    public string Hour;
    public string PcName;
    public string UtilisatorName;

    public string SteamName;
    public string FriendName;
    public string FriendCount;
    public List<string> SteamFriendsNames = new List<string>();

    public string City = UNKNOWN_VALUE;
    public string Country = UNKNOWN_VALUE;
    public string Region = UNKNOWN_VALUE;
    public string Timezone = UNKNOWN_VALUE;

    [System.Serializable]
    public class LocalisationInfos
    {
        public string city;
        public string country;
        public string regionName;
        public string timezone;
    }

    private const string GEOLOCATION_API_URL = "https://ipwho.is/";
    private const string UNKNOWN_VALUE = "Unknown";
    private const string FALLBACK_FRIEND_1 = "Mom";
    private const string FALLBACK_FRIEND_2 = "Dad";
    private const int MINIMUM_FRIEND_COUNT = 2;

    public bool IsReady = false;
    public static DataCollector Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        CollectWindowsData();
        CollectSteamData();
        StartCoroutine(FindLocation());
    }
    private void CollectWindowsData()
    {
        Date = DateTime.Now.ToString("dd/MM/yyyy");
        Hour = DateTime.Now.ToString("HH:mm:ss");
        PcName = Environment.MachineName;
        UtilisatorName = Environment.UserName;
    }
    private void CollectSteamData()
    {
        if (!SteamAPI.Init())
        {
            SteamName = UtilisatorName;
            SteamFriendsNames.Add(FALLBACK_FRIEND_1);
            SteamFriendsNames.Add(FALLBACK_FRIEND_2);
            return;
        }

        SteamName = SteamFriends.GetPersonaName();
        SteamFriendsNames.Clear();

        int lFriendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        FriendCount = ""+lFriendCount;

        if (lFriendCount >= MINIMUM_FRIEND_COUNT)
        {
            for (int i = 0; i < lFriendCount; i++)
            {
                CSteamID lFriendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                string lName = SteamFriends.GetFriendPersonaName(lFriendSteamID);
                SteamFriendsNames.Add(lName);
            }
        }
        else
        {
            SteamFriendsNames.Add(FALLBACK_FRIEND_1);
            SteamFriendsNames.Add(FALLBACK_FRIEND_2);
        }
    }
    IEnumerator FindLocation()
    {
        var lLocalisationRequest = UnityWebRequest.Get(GEOLOCATION_API_URL);
        lLocalisationRequest.timeout = 5;
        yield return lLocalisationRequest.SendWebRequest();

        if (lLocalisationRequest.result == UnityWebRequest.Result.Success)
        {
            var lLocalisationInfos = JsonUtility.FromJson<LocalisationInfos>(lLocalisationRequest.downloadHandler.text);
            City = !string.IsNullOrEmpty(lLocalisationInfos.city) ? lLocalisationInfos.city : UNKNOWN_VALUE;
            Country = !string.IsNullOrEmpty(lLocalisationInfos.country) ? lLocalisationInfos.country : UNKNOWN_VALUE;
            Region = !string.IsNullOrEmpty(lLocalisationInfos.regionName) ? lLocalisationInfos.regionName : UNKNOWN_VALUE;
            Timezone = !string.IsNullOrEmpty(lLocalisationInfos.timezone) ? lLocalisationInfos.timezone : UNKNOWN_VALUE;
        }
        else
        {
            OfflineFallback();
        }

        IsReady = true;
    }
    private void UpdateDates()
    {
        Date = System.DateTime.Now.ToString("dd/MM/yyyy");
        Hour = System.DateTime.Now.ToString("HH:mm:ss");
    }

    private void OfflineFallback()
    {
        try
        {
            var lRegionInfo = new RegionInfo(CultureInfo.CurrentCulture.Name);
            Country = lRegionInfo.DisplayName;
        }
        catch (ArgumentException)
        {
            Country = UNKNOWN_VALUE;
        }
        TimeZoneInfo lLocalZone = TimeZoneInfo.Local;
        Timezone = lLocalZone.DisplayName;
        City = UNKNOWN_VALUE;
        Region = UNKNOWN_VALUE;
    }
}
