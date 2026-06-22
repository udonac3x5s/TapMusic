using UnityEngine;
using PlayFab.ClientModels;
using System.Collections.Generic;
using common;
using TMPro;

public class RankingUI : MonoBehaviour
{
    [SerializeField] private GameObject rankingBoxPrefab;
    [SerializeField] private Transform rankingContentParent;


    void Start()
    {
        // LoadRankingTop50();
    }

    [ContextMenu("Test Load Ranking (Top50)")]
    public void LoadRankingTop50()
    {
        if (Handover.Instance == null || Handover.Instance.handoverSongData == null)
        {
            Debug.LogWarning("Handover or SongData が null。曲選択後に開いてる？");
            return;
        }

        var songId = Handover.Instance.handoverSongData.name; // できれば固定ID推奨
        var diff = Handover.Instance.nowDifficulty;

        RankingLoad.Instance.GetSongLeaderboard(
            songId, diff, 0, 50,
            onSuccess: OnLoaded,
            onError: err => Debug.LogWarning(err.GenerateErrorReport())
        );
    }

    private void OnLoaded(List<PlayerLeaderboardEntry> list)
    {
        Debug.Log($"ランキング件数: {list.Count}");
        rankingContentParent.DetachChildren(); // 一旦クリア

        foreach (var e in list)
        {
            var name = string.IsNullOrEmpty(e.DisplayName) ? "(NoName)" : e.DisplayName;
            Debug.LogWarning($"{e.Position + 1}位 {name} : {e.StatValue}");

            var box = Instantiate(rankingBoxPrefab, rankingContentParent);
            box.transform.GetChild(0).GetComponent<TMP_Text>().text = e.DisplayName.ToString();
            box.transform.GetChild(1).GetComponent<TMP_Text>().text = e.StatValue.ToString();
            box.transform.GetChild(2).GetComponent<TMP_Text>().text = (e.Position + 1).ToString() + "位";
        }
    }
}
