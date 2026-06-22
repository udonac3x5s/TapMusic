using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabRanking : MonoBehaviour
{
    #region Private Variables
    [Header("Debug (任意)")]
    [SerializeField] private string debugSongId = "song001";  // デバッグ用曲ID
    [SerializeField] private string debugDifficulty = "Hard"; // デバッグ用難易度
    [SerializeField] private int debugScore = 123456;         // デバッグ用スコア
    #endregion

    #region Public API
    /// <summary>
    /// スコアを送信する（曲別・難易度別）
    /// </summary>
    public void PostScore(string songId, string difficulty, int score, Action onSuccess = null, Action<string> onError = null)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            onError?.Invoke("PlayFab未ログインです");
            return;
        }

        string statName = BuildStatName(songId, difficulty);

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = statName,
                    Value = score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(
            request,
            _ =>
            {
                Debug.Log($"スコア送信成功: {statName} = {score}");
                onSuccess?.Invoke();
            },
            error =>
            {
                string msg = error.GenerateErrorReport();
                Debug.LogError(msg);
                onError?.Invoke(msg);
            }
        );
    }

    /// <summary>
    /// ランキングを取得する（TopN）
    /// </summary>
    public void GetTopLeaderboard(string songId, string difficulty, int topN,
        Action<List<PlayerLeaderboardEntry>> onSuccess, Action<string> onError = null)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            onError?.Invoke("PlayFab未ログインです");
            return;
        }

        string statName = BuildStatName(songId, difficulty);

        var request = new GetLeaderboardRequest
        {
            StatisticName = statName,
            StartPosition = 0,
            MaxResultsCount = Mathf.Clamp(topN, 1, 100)
        };

        PlayFabClientAPI.GetLeaderboard(
            request,
            result =>
            {
                Debug.Log($"ランキング取得成功: {statName} entries={result.Leaderboard?.Count ?? 0}");
                onSuccess?.Invoke(result.Leaderboard); // ← これが List<PlayerLeaderboardEntry>
            },
            error =>
            {
                string msg = error.GenerateErrorReport();
                Debug.LogError(msg);
                onError?.Invoke(msg);
            }
        );
    }

    /// <summary>
    /// 自分の順位周辺を取得する
    /// </summary>
    public void GetAroundMe(string songId, string difficulty, int maxResults,
        Action<List<PlayerLeaderboardEntry>> onSuccess, Action<string> onError = null)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            onError?.Invoke("PlayFab未ログインです");
            return;
        }

        string statName = BuildStatName(songId, difficulty);

        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statName,
            MaxResultsCount = Mathf.Clamp(maxResults, 1, 100)
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            request,
            result =>
            {
                Debug.Log($"自分の順位周辺取得成功: {statName} entries={result.Leaderboard?.Count ?? 0}");
                onSuccess?.Invoke(result.Leaderboard); // ← List<PlayerLeaderboardEntry>
            },
            error =>
            {
                string msg = error.GenerateErrorReport();
                Debug.LogError(msg);
                onError?.Invoke(msg);
            }
        );
    }
    #endregion

    #region Debug Helpers
    /// <summary>
    /// デバッグ：スコア送信→Top10取得してConsoleに出す
    /// </summary>
    [ContextMenu("DEBUG: スコア送信→Top10取得")]
    private void DebugPostThenGetTop10()
    {
        PostScore(
            debugSongId,
            debugDifficulty,
            debugScore,
            onSuccess: () =>
            {
                GetTopLeaderboard(
                    debugSongId,
                    debugDifficulty,
                    10,
                    entries =>
                    {
                        for (int i = 0; i < entries.Count; i++)
                        {
                            var e = entries[i];
                            Debug.Log($"{e.Position + 1}位 {e.DisplayName}({e.PlayFabId}) : {e.StatValue}");
                        }
                    }
                );
            }
        );
    }
    #endregion

    #region Internal
    /// <summary>
    /// 統計名を作る（曲別・難易度別）
    /// </summary>
    private string BuildStatName(string songId, string difficulty)
    {
        return $"SCORE_{songId}_{difficulty}";
    }
    #endregion
}
