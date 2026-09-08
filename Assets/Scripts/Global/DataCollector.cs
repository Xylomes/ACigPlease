using UnityEngine;
using UnityEngine.Networking;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
public class DataCollector : MonoBehaviour
{
    //Windows infos 
    public string Date;
    public string Hour;
    public string PcName;
    public string UtilisatorName;

    //steam infos 
    public string SteamName;
    public string FriendName;
    public string FriendCount;
    public List<string> SteamFriendsNames = new List<string>();

    //Localisation
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

        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        FriendCount = ""+friendCount;

        if (friendCount >= MINIMUM_FRIEND_COUNT)
        {
            for (int i = 0; i < friendCount; i++)
            {
                CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                string name = SteamFriends.GetFriendPersonaName(friendSteamID);
                SteamFriendsNames.Add(name);
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
        var localisationRequest = UnityWebRequest.Get(GEOLOCATION_API_URL);
        localisationRequest.timeout = 5;
        yield return localisationRequest.SendWebRequest();

        if (localisationRequest.result == UnityWebRequest.Result.Success)
        {
            var localisationInfos = JsonUtility.FromJson<LocalisationInfos>(localisationRequest.downloadHandler.text);
            City = !string.IsNullOrEmpty(localisationInfos.city) ? localisationInfos.city : UNKNOWN_VALUE;
            Country = !string.IsNullOrEmpty(localisationInfos.country) ? localisationInfos.country : UNKNOWN_VALUE;
            Region = !string.IsNullOrEmpty(localisationInfos.regionName) ? localisationInfos.regionName : UNKNOWN_VALUE;
            Timezone = !string.IsNullOrEmpty(localisationInfos.timezone) ? localisationInfos.timezone : UNKNOWN_VALUE;
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
            var regionInfo = new RegionInfo(CultureInfo.CurrentCulture.Name);
            Country = regionInfo.DisplayName;
        }
        catch (ArgumentException)
        {
            Country = UNKNOWN_VALUE;
        }
        TimeZoneInfo localZone = TimeZoneInfo.Local;
        Timezone = localZone.DisplayName;
        City = UNKNOWN_VALUE;
        Region = UNKNOWN_VALUE;
    }
}
