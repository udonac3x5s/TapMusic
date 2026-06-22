using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using common;

public class SongSortPanel : MonoBehaviour
{
    #region Inspector設定
    [SerializeField] SelectSongManager selectSongManager;
    [SerializeField] GameObject sortPanelObject;
    [SerializeField] Button sortByDifficultyButton;

    [SerializeField] Button ascendingOrderButton;
    [SerializeField] Button descendingOrderButton;


    bool isPanelVisible = false;
    #endregion

    void Start()
    {
        // 初期状態では非表示
        sortPanelObject.SetActive(false);

        // ボタンにクリックイベントを追加
        sortByDifficultyButton.onClick.AddListener(ToggleSortPanel);

        ascendingOrderButton.onClick.AddListener(() => selectSongManager.OnOrderAscending());
        descendingOrderButton.onClick.AddListener(() => selectSongManager.OnOrderDescending());
    }

    #region SortPanel表示・非表示
    void ToggleSortPanel()
    {
        if (isPanelVisible)
        {
            PanelFalse();
        }
        else
        {
            PanelTrue();
        }
        isPanelVisible = !isPanelVisible;
    }

    void PanelTrue()
    {
        sortPanelObject.SetActive(true);
    }

    void PanelFalse()
    {
        sortPanelObject.SetActive(false);
    }
    #endregion


    // 難易度でソートボタンが押されたとき
    public void OnSortByDifficulty()
    {
        selectSongManager.OnSortTypeDifficulty();
    }








}
