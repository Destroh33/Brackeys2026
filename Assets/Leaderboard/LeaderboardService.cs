using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class LeaderboardService
{
    public const string Table = "runs";

    const string ListColumns = "id,username,time_ms,seed,seeded,deaths,created_at";

    public static string Reference { get; private set; }
    public static string PublishableKey { get; private set; }

    public const string ConfigPath = "LeaderboardConfig";

    static bool loadedConfig;

    public static bool Configured
    {
        get
        {
            if (string.IsNullOrEmpty(Reference) && !loadedConfig) LoadConfig();
            return !string.IsNullOrEmpty(Reference) && !string.IsNullOrEmpty(PublishableKey);
        }
    }

    static void LoadConfig()
    {
        loadedConfig = true;

        var config = Resources.Load<LeaderboardConfig>(ConfigPath);
        if (config) Configure(config.Reference, config.PublishableKey);
        else Debug.LogWarning($"[Leaderboard] No Resources/{ConfigPath} asset, and nothing called Configure()");
    }
    public static string BaseUrl => $"https://{Reference}.supabase.co/rest/v1/{Table}";

    public static void Configure(string reference, string publishableKey)
    {
        if (string.IsNullOrEmpty(reference) || string.IsNullOrEmpty(publishableKey)) return;

        Reference = reference;
        PublishableKey = publishableKey;
    }

    [Serializable]
    public class Entry
    {
        public long id;
        public string username;
        public int time_ms;
        public string seed;
        public bool seeded;
        public int deaths;
        public string created_at;

        public float Seconds => time_ms / 1000f;
    }

    [Serializable] class EntryList { public Entry[] items; }

    public static IEnumerator FetchTop(int limit, Action<List<Entry>> done)
    {
        yield return Fetch($"{BaseUrl}?select={ListColumns}&order=time_ms.asc&limit={limit}", done);
    }

    public static IEnumerator FetchTopForSeed(string seed, int limit, Action<List<Entry>> done)
    {
        string escaped = UnityWebRequest.EscapeURL(seed);
        yield return Fetch($"{BaseUrl}?select={ListColumns}&seed=eq.{escaped}&order=time_ms.asc&limit={limit}", done);
    }

    static IEnumerator Fetch(string url, Action<List<Entry>> done)
    {
        if (!Configured) { done?.Invoke(null); yield break; }

        using var request = UnityWebRequest.Get(url);
        SetHeaders(request);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[Leaderboard] Fetch failed: {request.error}");
            done?.Invoke(null);
            yield break;
        }

        done?.Invoke(ParseEntries(request.downloadHandler.text));
    }

    public static IEnumerator FetchRank(int timeMs, Action<int> done)
    {
        if (!Configured) { done?.Invoke(-1); yield break; }

        using var request = UnityWebRequest.Get($"{BaseUrl}?select=id&time_ms=lt.{timeMs}");
        SetHeaders(request);
        request.SetRequestHeader("Prefer", "count=exact");
        request.SetRequestHeader("Range", "0-0");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            done?.Invoke(-1);
            yield break;
        }

        string range = request.GetResponseHeader("Content-Range");
        int slash = range == null ? -1 : range.LastIndexOf('/');

        if (slash < 0 || !int.TryParse(range[(slash + 1)..], out int faster))
        {
            done?.Invoke(-1);
            yield break;
        }

        done?.Invoke(faster + 1);
    }

    public static IEnumerator Submit(string playerId, string username, int timeMs, string seed, bool seeded, int deaths, Action<bool> done)
    {
        if (!Configured) { done?.Invoke(false); yield break; }

        var body = new StringBuilder();
        body.Append('{')
            .Append("\"player_id\":\"").Append(Escape(playerId)).Append("\",")
            .Append("\"username\":\"").Append(Escape(username)).Append("\",")
            .Append("\"time_ms\":").Append(timeMs).Append(',')
            .Append("\"seed\":\"").Append(Escape(seed)).Append("\",")
            .Append("\"seeded\":").Append(seeded ? "true" : "false").Append(',')
            .Append("\"deaths\":").Append(deaths)
            .Append('}');

        using var request = new UnityWebRequest(BaseUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString())),
            downloadHandler = new DownloadHandlerBuffer(),
        };
        SetHeaders(request);
        yield return request.SendWebRequest();

        bool ok = request.result == UnityWebRequest.Result.Success;
        if (!ok) Debug.LogWarning($"[Leaderboard] Submit failed: {request.error} | {request.downloadHandler.text}");

        done?.Invoke(ok);
    }

    static void SetHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("apikey", PublishableKey);
        if (PublishableKey.StartsWith("eyJ")) request.SetRequestHeader("Authorization", "Bearer " + PublishableKey);
        request.SetRequestHeader("Content-Type", "application/json");
    }

    public static List<Entry> ParseEntries(string json)
    {
        var list = new List<Entry>();
        if (string.IsNullOrEmpty(json)) return list;

        try
        {
            var wrapper = JsonUtility.FromJson<EntryList>("{\"items\":" + json + "}");
            if (wrapper?.items != null) list.AddRange(wrapper.items);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Leaderboard] Parse failed: " + e.Message);
        }

        return list;
    }

    static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
