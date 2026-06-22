using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class RankingLoader : MonoBehaviour
{
    public static RankingLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// ランキングを取得する（TopN）
    /// </summary>
    public void LoadRanking(string songId, string difficulty, int topN)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("PlayFab未ログイン");
            return;
        }

        string statName = $"SCORE_{songId}_{difficulty}";

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
                Debug.Log($"=== Ranking {statName} ===");
                foreach (var e in result.Leaderboard)
                {
                    Debug.Log(
                        $"{e.Position + 1}位 " +
                        $"{(string.IsNullOrEmpty(e.DisplayName) ? "NO NAME" : e.DisplayName)} : " +
                        $"{e.StatValue}"
                    );
                }
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }
}
