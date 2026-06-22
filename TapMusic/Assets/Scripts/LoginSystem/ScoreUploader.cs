using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class ScoreUploader : MonoBehaviour
{
    public static ScoreUploader Instance { get; private set; }

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
    /// リザルト専用：サーバー値より高ければ「今回スコア」をアップロードする（ローカル自己ベストは見ない）
    /// </summary>
    public void UploadScore(string songId, string difficulty, int score)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("PlayFab未ログイン");
            return;
        }

        string statName = $"SCORE_{songId}_{difficulty}";

        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest { StatisticNames = new List<string> { statName } },
            getResult =>
            {
                bool found = false;
                int serverScore = int.MinValue;

                if (getResult.Statistics != null)
                {
                    foreach (var s in getResult.Statistics)
                    {
                        if (s.StatisticName == statName)
                        {
                            found = true;
                            serverScore = s.Value;
                            break;
                        }
                    }
                }

                Debug.Log($"[RESULT CHECK] {statName} server={(found ? serverScore.ToString() : "none")} result={score}");

                // サーバー未登録（初回） or 今回がサーバーより高いなら送る
                if (!found || score > serverScore)
                {
                    Upload(statName, score);
                }
                else
                {
                    Debug.Log($"[RESULT SKIP] サーバーの方が高い/同じ: server={serverScore} result={score}");
                }
            },
            error =>
            {
                Debug.LogError($"[RESULT GET NG] {error.GenerateErrorReport()}");
                // 取得失敗時に送るかは好み：
                // ・安全 → 送らない（データ汚し防止）
                // ・確実 → 送る（下の行を有効化）
                // Upload(statName, score);
            }
        );
    }

    private void Upload(string statName, int score)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(
            new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate { StatisticName = statName, Value = score }
                }
            },
            _ => Debug.Log($"[RESULT UPLOAD OK] {statName} = {score}"),
            e => Debug.LogError($"[RESULT UPLOAD NG] {e.GenerateErrorReport()}")
        );
    }
}
