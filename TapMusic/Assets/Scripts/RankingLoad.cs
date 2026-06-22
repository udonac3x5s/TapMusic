using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using common;

public class RankingLoad : MonoBehaviour
{
    public static RankingLoad Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 曲ごとのランキング取得（上位）
    /// </summary>
    public void GetSongLeaderboard(string songId, Difficulty difficulty, int start = 0, int maxResults = 50,
        Action<List<PlayerLeaderboardEntry>> onSuccess = null,
        Action<PlayFabError> onError = null)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            onError?.Invoke(new PlayFabError { ErrorMessage = "PlayFab未ログイン" });
            return;
        }

        string statName = BuildStatName(songId, difficulty);

        var req = new GetLeaderboardRequest
        {
            StatisticName = statName,
            StartPosition = start,
            MaxResultsCount = maxResults,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        };

        PlayFabClientAPI.GetLeaderboard(req,
            res => onSuccess?.Invoke(res.Leaderboard),
            err => onError?.Invoke(err)
        );
    }

    /// <summary>
    /// 曲ごとのランキング取得（自分周辺）
    /// </summary>
    public void GetSongLeaderboardAroundMe(string songId, Difficulty difficulty, int maxResults = 20,
        Action<List<PlayerLeaderboardEntry>> onSuccess = null,
        Action<PlayFabError> onError = null)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            onError?.Invoke(new PlayFabError { ErrorMessage = "PlayFab未ログイン" });
            return;
        }

        string statName = BuildStatName(songId, difficulty);

        var req = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statName,
            MaxResultsCount = maxResults,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(req,
            res => onSuccess?.Invoke(res.Leaderboard),
            err => onError?.Invoke(err)
        );
    }

    private static string BuildStatName(string songId, Difficulty difficulty)
        => $"SCORE_{songId}_{difficulty}";


    /// <summary>
    /// ランキング読み込み（UI用）
    /// </summary>
    public void LoadRanking()
    {
        FindFirstObjectByType<RankingUI>()?.LoadRankingTop50();
    }


}
