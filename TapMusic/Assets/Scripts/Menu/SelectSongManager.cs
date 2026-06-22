using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using common;
using TMPro;

public class SelectSongManager : MonoBehaviour
{
    #region SerializeField
    [SerializeField] GameObject prefabButtonTemplate;
    [SerializeField] GameObject imageScroll;
    [SerializeField] GameObject songInterface;
    [SerializeField] public GameObject DifficultyButtons;
    [SerializeField] GameObject textStart;
    SongSelectInfo songSelectInfo;
    SongSortType lastSortType = SongSortType.ByName;
    SortOrder lastSortOrder = SortOrder.Ascending;
    #endregion

    #region Data
    public Difficulty nowDifficulty = Difficulty.Hard;

    private readonly List<SongData> songList = new();
    private readonly List<Text> difficultLevelText = new();
    private readonly Dictionary<Button, bool> buttonClickedFlags = new();

    // Addressables 同時ロード制限
    private readonly SemaphoreSlim assetSemaphore = new(4);


    private bool isTap = false;

    #endregion

    #region Start or Initialization
    void Start()
    {
        StartAsync().Forget();
    }

    private async UniTask StartAsync()
    {
        // SoundManager.Instance.ResetSettings();              // サウンドマネージャー設定リセット
        SoundManager.Instance.LoopSettings(true);           // BGMループ設定
        ScoreManager.Instance.LoadScores();                 // スコアデータ読み込み
        await InitializeAsync();
        await SetList();
    }

    async UniTask InitializeAsync()
    {
        // Addressables 初期化3
        await Addressables.InitializeAsync();
    }
    #endregion

    #region Song List Load

    private async UniTask SetList()
    {
        // TextAsset(JSON)を全部ロード
        var handle = Addressables.LoadAssetsAsync<TextAsset>("SongData", null);
        await handle.ToUniTask();

        // 曲データ解析＆UI生成
        try
        {
            // JSON解析は軽いので並列
            var parseTasks = new List<UniTask<SongData>>();

            foreach (var json in handle.Result)
            {
                if (json.name is "Easy" or "Normal" or "Hard")
                    continue;

                parseTasks.Add(ParseSongDataAsync(json));
            }

            // JSONだけ先に全部揃える
            var songs = await UniTask.WhenAll(parseTasks);

            foreach (var song in songs)
            {
                songList.Add(song);

                // UIは即生成（体感速度最優先）
                CreateSongButton(song);

                // 重いアセットは裏でロード
                LoadSongAssetsAsync(song).Forget();
            }

            Debug.Log($"読み込んだ曲数: {songList.Count}");

            // 最初の曲を選択状態にする
            if (songList.Count > 0)
            {
                // 以前の選択情報を読み込み
                SongData lastSong = songList[0];
                Difficulty lastDifficulty = Difficulty.Hard;
                // 選曲情報読み込み
                SetSongSelectInfoLoadJson(ref lastSong, ref lastDifficulty);
                if (lastSong != null)
                {
                    // 難易度設定
                    nowDifficulty = lastDifficulty;
                    LevelTextUpdate();

                    // 曲選択
                    for (int i = 0; i < songList.Count; i++)
                    {
                        if (songList[i].name == lastSong.name)
                        {
                            var button = imageScroll.transform.GetChild(i).GetComponent<Button>();

                            // 画像情報などがロードされるのを待つ
                            await LoadSongAssetsAsync(songList[i]);

                            await OnSongButtonClicked(button, songList[i]).ContinueWith(() =>
                            {
                                // 現在の難易度に合わせてボタンの色を変更
                                foreach (Transform child in DifficultyButtons.transform)
                                {
                                    var button = child.GetComponent<Image>();
                                    var colors = button.color;
                                    if (button.GetComponent<Button>().name == nowDifficulty.ToString())
                                    {
                                        colors = new Color32(164, 220, 246, 255); // 選択されたボタンを水色に
                                    }
                                    else
                                    {
                                        colors = Color.white; // 他のボタンを白に
                                    }
                                    button.color = colors;
                                }
                                // フェードイン
                                if (SlideUIGenerate.Instance != null)
                                    SlideUIGenerate.Instance.SlideOpen();
                            });
                            break;
                        }
                    }
                }
                else
                {
                    var firstButton = imageScroll.transform.GetChild(0).GetComponent<Button>();

                    // ロード完了を待つ
                    await LoadSongAssetsAsync(songList[0]);

                    // ロード後に選択処理
                    await OnSongButtonClicked(firstButton, songList[0]).ContinueWith(() =>
                    {
                        // 現在の難易度に合わせてボタンの色を変更
                        foreach (Transform child in DifficultyButtons.transform)
                        {
                            var button = child.GetComponent<Image>();
                            var colors = button.color;
                            if (button.GetComponent<Button>().name == nowDifficulty.ToString())
                            {
                                colors = new Color32(164, 220, 246, 255); // 選択されたボタンを水色に
                            }
                            else
                            {
                                colors = Color.white; // 他のボタンを白に
                            }
                            button.color = colors;
                        }

                        // フェードイン
                        // if (SlideUIGenerate.Instance != null)
                        //     SlideUIGenerate.Instance.SlideOpen();
                    });
                }
            }

            // ソート初期化（最後のソート方法を適用）
            ApplySort(false);
        }
        finally
        {
            // 解放
            Addressables.Release(handle);
        }
    }

    /// <summary>
    /// JSON解析
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private UniTask<SongData> ParseSongDataAsync(TextAsset json)
    {
        SongData song = JsonUtility.FromJson<SongData>(json.text);
        return UniTask.FromResult(song);
    }
    #endregion

    #region 曲データのアセットロード
    private async UniTask LoadSongAssetsAsync(SongData song)
    {
        // セマフォを取得
        // セマフォとは、同時にアクセスできるスレッド数を制限する仕組み
        await assetSemaphore.WaitAsync();

        try
        {
            string folder = song.name;

            var jacketTask = Addressables
                .LoadAssetAsync<Texture2D>($"SongData/{folder}/{song.jacketName}")
                .ToUniTask();

            var audioTask = Addressables
                .LoadAssetAsync<AudioClip>($"SongData/{folder}/{song.audioName}")
                .ToUniTask();

            (Texture2D tex, AudioClip clip) =
                await UniTask.WhenAll(jacketTask, audioTask);

            if (tex != null)
            {
                song.jacketImage = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f)
                );
            }

            song.audioClip = clip;
        }
        finally
        {
            // セマフォを解放
            assetSemaphore.Release();
        }
    }
    #endregion

    #region ボタン生成 クリック処理
    private void CreateSongButton(SongData song)
    {
        var obj = Instantiate(prefabButtonTemplate, imageScroll.transform);

        obj.transform.Find("SongName").GetComponent<Text>().text = song.name;
        var levelText = obj.transform.Find("SongLevel").GetComponent<Text>();
        levelText.text = song.difficulty.hard.ToString("0.0");
        difficultLevelText.Add(levelText);

        // ボタンクリックイベント登録
        if (!obj.TryGetComponent<Button>(out var button))
            return;

        buttonClickedFlags[button] = false;

        button.onClick.AddListener(() =>
        {
            OnSongButtonClicked(button, song).Forget();
        });
    }

    /// <summary>
    /// 曲ボタンクリック
    /// </summary>
    /// <param name="button">クリックされたボタン</param>
    /// <param name="song">曲データ</param>
    /// <returns></returns>
    private async UniTask OnSongButtonClicked(Button button, SongData song)
    {
        if (isTap) return;

        foreach (var key in buttonClickedFlags.Keys.ToList())
        {
            // 他のボタンはクリックフラグをリセット
            if (key != button)
                buttonClickedFlags[key] = false;
        }

        // 初回クリック（選択）
        if (!buttonClickedFlags[button])
        {
            buttonClickedFlags[button] = true;

            if (song.jacketImage != null)
                songInterface.transform.Find("Jacket")
                    .GetComponent<Image>().sprite = song.jacketImage;

            songInterface.transform.Find("SongName").GetComponent<TextMeshProUGUI>().text = song.name;
            songInterface.transform.Find("ComposerName").GetComponent<TextMeshProUGUI>().text = song.composer;
            songInterface.transform.Find("BPM").GetComponent<TextMeshProUGUI>().text = $"BPM : {song.bpm}";
            songInterface.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = $"Best Score : {ScoreManager.Instance.GetSongScoreData(song.name, nowDifficulty).ToString("D7")}";

            DifficultyButtons.transform.Find("Easy")
                .GetComponentInChildren<Text>().text = song.difficulty.easy.ToString("0.0");
            DifficultyButtons.transform.Find("Normal")
                .GetComponentInChildren<Text>().text = song.difficulty.normal.ToString("0.0");
            DifficultyButtons.transform.Find("Hard")
                .GetComponentInChildren<Text>().text = song.difficulty.hard.ToString("0.0");

            // 選曲した曲のボタンの色を変更(Imageコンポーネント)
            foreach (Transform child in imageScroll.transform)
            {
                var img = child.GetComponent<Image>();
                Color colors;
                if (child.GetComponent<Button>() == button)
                {
                    colors = new Color32(164, 220, 246, 255); // 選択されたボタンを水色に
                }
                else
                {
                    colors = Color.white; // 他のボタンを白に
                }
                img.color = colors;
            }

            // startテキストボタンの子オブジェクトにセット
            textStart.transform.SetParent(button.transform);
            // アンカーを中央にセット
            RectTransform textRect = textStart.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);

            // 位置移動
            // textRect.localPosition = new Vector3(0, 0, 0);
            textRect.anchoredPosition = new Vector2(0, 0);


            if (song.audioClip != null)
            {
                SoundManager.Instance.StopBGM();
                SoundManager.Instance.PlayBGM(song.audioClip);
            }

            Handover.Instance.handoverSongData = song;
            Handover.Instance.nowDifficulty = nowDifficulty;
            Handover.Instance.dataSheet = null;

            RankingLoad.Instance.LoadRanking(); // ランキング読み込み

        }
        // 2回目クリック（ゲーム開始）
        else
        {
            string diffName = nowDifficulty switch {
                Difficulty.Easy => "Easy",
                Difficulty.Normal => "Normal",
                Difficulty.Hard => "Hard",
                _ => "Hard"
            };
            string scoreKey = $"SongData/{song.name}/{diffName}.json";

            // string scoreKey = $"SongData/{song.name}/{nowDifficulty}.json";

            var handle = Addressables.LoadAssetAsync<TextAsset>(scoreKey);
            var textAsset = await handle.ToUniTask();

            try
            {
                if (textAsset == null)
                {
                    Debug.LogWarning($"譜面データが見つかりません: {scoreKey}");
                    return;
                }
                isTap = true;


                Handover.Instance.dataSheet = textAsset != null
                    ? JsonUtility.FromJson<DataSheet>(textAsset.text)
                    : null;

                Handover.Instance.handoverSongData = song;
                Handover.Instance.nowDifficulty = nowDifficulty;

                // 選曲情報保存
                SongSelectInfoSetJson(song);



                SoundManager.Instance.StopBGM();
                SlideUIGenerate.Instance.SceneMove("Game").Forget();
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
    }

    /// <summary>
    /// 難易度ボタンクリック
    /// </summary>
    public void OnDifficultyButtonClicked(int difficultyIndex)
    {
        nowDifficulty = difficultyIndex switch
        {
            0 => Difficulty.Easy,
            1 => Difficulty.Normal,
            2 => Difficulty.Hard,
            _ => Difficulty.Hard
        };

        // レベルテキスト更新
        LevelTextUpdate();

        // 選択中の曲があればスコア更新
        var selectedSong = Handover.Instance.handoverSongData;
        if (selectedSong != null)
        {
            songInterface.transform.Find("Score").GetComponent<TextMeshProUGUI>().text =
                $"Best Score : {ScoreManager.Instance.GetSongScoreData(selectedSong.name, nowDifficulty).ToString("D7")}";
        }
    }
    #endregion

    #region レベルテキストの更新
    /// <summary>
    /// 難易度変更時にレベルテキストを更新
    /// </summary>
    public void LevelTextUpdate()
    {
        // 各曲のレベルテキストを現在の難易度に合わせて更新
        for (int i = 0; i < imageScroll.transform.childCount; i++)
        {
            var btnTf = imageScroll.transform.GetChild(i);
            var nameText = btnTf.Find("SongName")?.GetComponent<Text>();
            var levelText = btnTf.Find("SongLevel")?.GetComponent<Text>();
            if (nameText == null || levelText == null) continue;

            // songListから曲データを探す
            var song = songList.FirstOrDefault(s => s.name == nameText.text);
            if (song == null) continue;

            // 難易度に応じてレベルテキストを更新
            levelText.text = song.difficulty.GetValue(nowDifficulty).ToString("0.0");
        }
    }
    #endregion

    #region Sort Function
    /// <summary>
    /// ソート順変更（昇順）
    /// </summary>
    public void OnOrderAscending()
    {
        lastSortOrder = SortOrder.Ascending;
        ApplySort(true);
    }

    /// <summary>
    /// ソート順変更（降順）
    /// </summary>
    public void OnOrderDescending()
    {
        lastSortOrder = SortOrder.Descending;
        ApplySort(true);
    }

    /// <summary>
    /// ソートタイプ変更（曲名順）
    /// </summary>
    public void OnSortTypeName()
    {
        lastSortType = SongSortType.ByName;
        ApplySort(true);
    }

    /// <summary>
    /// ソートタイプ変更（難易度順）
    /// </summary>
    public void OnSortTypeDifficulty()
    {
        lastSortType = SongSortType.ByDifficulty;
        ApplySort(true);
    }


    // これだけが並び替えを実行する（保存するかどうかもここで制御）
    public void ApplySort(bool save)
    {
        switch (lastSortType)
        {
            case SongSortType.ByName:
                songList.Sort((a, b) => lastSortOrder == SortOrder.Ascending
                    ? string.Compare(a.name, b.name, StringComparison.Ordinal)
                    : string.Compare(b.name, a.name, StringComparison.Ordinal));
                break;

            case SongSortType.ByDifficulty:
                songList.Sort((a, b) => lastSortOrder == SortOrder.Ascending
                    ? a.difficulty.GetValue(nowDifficulty).CompareTo(b.difficulty.GetValue(nowDifficulty))
                    : b.difficulty.GetValue(nowDifficulty).CompareTo(a.difficulty.GetValue(nowDifficulty)));
                break;
        }

        RefreshSongButtons();
        LevelTextUpdate();   // 表示の難易度ズレ防止（重要）

        if (save) SetSortTypeJson();
    }










    /// <summary>
    /// 曲ボタンの順番を更新
    /// </summary>
    private void RefreshSongButtons()
    {
        // 曲ボタンの順番をsongListに合わせて更新・子オブジェクトとして順番を入れ替え
        for (int i = 0; i < songList.Count; i++)
        {
            var song = songList[i];

            // songListの曲名と一致するボタンを探す
            for (int j = 0; j < imageScroll.transform.childCount; j++)
            {
                var btn = imageScroll.transform.GetChild(j).GetComponent<Button>();
                var songName = btn.transform.Find("SongName").GetComponent<Text>().text;

                if (songName == song.name)
                {
                    // 見つかったら順番を入れ替え
                    btn.transform.SetSiblingIndex(i);
                    break;
                }
            }
        }
    }
    #endregion

    #region Set User Info

    /// <summary>
    /// 選曲情報をJSONから読み込む
    /// </summary>
    void SetSongSelectInfoLoadJson(ref SongData lastSong, ref Difficulty lastDifficulty)
    {
        // Jsonとして読み込み
        string path = Application.persistentDataPath + "/" + Const.SONG_SELECT_INFO_FILE_NAME;
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            songSelectInfo = JsonUtility.FromJson<SongSelectInfo>(json);
            lastSong = songSelectInfo.lastSelectedSong;
            lastDifficulty = songSelectInfo.lastSelectedDifficulty;
            lastSortType = songSelectInfo.lastSortType;
            lastSortOrder = songSelectInfo.lastSortOrder;
        }
        else
        {
            lastSong = null;
            lastDifficulty = Difficulty.Hard;
            lastSortType = SongSortType.ByName;
            lastSortOrder = SortOrder.Ascending;
        }
    }

    void SetSortTypeJson()
    {
        // 既存データが無いなら作る
        songSelectInfo ??= new SongSelectInfo();

        // 選曲情報は「取れたときだけ更新」(取れないときは保持)
        if (Handover.Instance != null && Handover.Instance.handoverSongData != null)
        {
            songSelectInfo.lastSelectedSong = Handover.Instance.handoverSongData;
            songSelectInfo.lastSelectedDifficulty = nowDifficulty;
        }

        // ソート情報は常に更新
        songSelectInfo.lastSortType = lastSortType;
        songSelectInfo.lastSortOrder = lastSortOrder;

        string json = JsonUtility.ToJson(songSelectInfo);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + Const.SONG_SELECT_INFO_FILE_NAME,json);
    }


    void SongSelectInfoSetJson(SongData song)
    {
        // Jsonとして出力
        songSelectInfo = new()
        {
            lastSelectedSong = song,
            lastSelectedDifficulty = nowDifficulty,
            lastSortType = lastSortType,
            lastSortOrder = lastSortOrder
        };

        string json = JsonUtility.ToJson(songSelectInfo);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + Const.SONG_SELECT_INFO_FILE_NAME, json);
    }

    #endregion

}
