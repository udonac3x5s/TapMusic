using UnityEngine;
using common;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    // 設定JSON保存パス
    private string filePath;
    public AllScoreData AllScoreData = new();

#region Singleton
    private static ScoreManager instance;

    /// <summary>
    /// ScoreManagerインスタンス取得
    /// </summary>
    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                CreateInstance();
            }
            return instance;
        }
    }
#endregion


    private static void CreateInstance()
    {
        GameObject obj = new("ScoreManager");
        instance = obj.AddComponent<ScoreManager>();
        DontDestroyOnLoad(obj);
    }



    private void Awake()
    {
        // 多重生成防止
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Initialize();
    }

    void Initialize()
    {
        filePath = Path.Combine(Application.persistentDataPath, Const.SCORE_DATA_FILE_NAME);
    }


    /// <summary>
    /// スコアデータリセット
    /// </summary>
    public void ResetScores()
    {
        // 新規作成
        AllScoreData = new();
        SaveScores();
    }

    /// <summary>
    /// スコアデータ保存
    /// </summary>
    void SaveScores()
    {
        // JSONに変換
        var json = JsonUtility.ToJson(AllScoreData);
        // ファイルに保存
        File.WriteAllText(filePath, json);
        Debug.Log("スコアデータを保存しました: " + filePath, this);
    }

    public void LoadScores()
    {
        if (File.Exists(filePath))
        {
            // ファイルから読み込み
            var json = File.ReadAllText(filePath);
            // JSONをオブジェクトに変換
            AllScoreData = JsonUtility.FromJson<AllScoreData>(json);

            // スコアデータがnullの場合、新規作成
            if (AllScoreData == null)
                AllScoreData = new AllScoreData();

            Debug.Log("設定データを読み込みました: " + filePath, this);

        }
        else
        {
            Debug.Log("スコアファイルが存在しません。新規作成します。");
            ResetScores();
        }

        AllScoreData.songScoreData ??= Array.Empty<SongScoreData>();

        // ★ここで1回だけ整形して保存（初回/旧形式→新形式への移行）
        NormalizeAllScoreData(true);

    }


    /// <summary>
    /// 曲のスコアデータを取得
    /// </summary>
    public int GetSongScoreData(string songName, Difficulty difficulty)
    {
        // スコアデータが存在しない場合は新規作成
        if (AllScoreData == null)
            AllScoreData = new AllScoreData();

        // Null合体演算子を使用して、songScoreDataがnullの場合に空の配列を割り当てる
        AllScoreData.songScoreData ??= Array.Empty<SongScoreData>();

        // 1曲=1要素で取得
        var song = AllScoreData.songScoreData.FirstOrDefault(s => s.songName == songName);
        if (song == null)
        {
            song = CreateDefaultSongScoreData(songName); // Easy/Normal/Hard全部作る
            AllScoreData.songScoreData = AllScoreData.songScoreData.Append(song).ToArray();
            SaveScores();
        }

    // 難易度が欠けているデータを補完
        song.difficultyScoreData ??= Array.Empty<DifficultyScoreData>();
        song.difficultyScoreData = EnsureAllDifficulties(song.difficultyScoreData);

        var diff = song.difficultyScoreData.First(d => d.difficulty == difficulty);
        return diff.score;

        // foreach (var songData in AllScoreData.songScoreData)
        // {
        //     if (songData.songName == songName)
        //     {
        //         foreach (var diffData in songData.difficultyScoreData)
        //         {
        //             if (diffData.difficulty == difficulty)
        //             {
        //                 return diffData.score;
        //             }
        //         }
        //     }
        // }

        // Debug.Log($"スコアデータが見つかりませんでした。新規作成します: {songName}");
        // // 見つからなかった場合、新規作成
        // var newSongData = new SongScoreData
        // {
        //     songName = songName,
        //     difficultyScoreData = new DifficultyScoreData[]
        //     {
        //         new DifficultyScoreData
        //         {
        //             difficulty = difficulty,
        //             score = 0,
        //             maxCombo = 0,
        //             goodCount = 0,
        //             okCount = 0,
        //             missCount = 0
        //         }
        //     }
        // };

        // Debug.Log($"新規スコアデータ作成: {songName}");
        // var songList = new List<SongScoreData>(AllScoreData.songScoreData)
        // {
        //     newSongData
        // };
        // AllScoreData.songScoreData = songList.ToArray();

        // SaveScores();
        // return 0;
    }




    private void NormalizeAllScoreData(bool saveAfter)
    {
        // songName単位で統合（同名が複数あっても1つにする）
        var map = new Dictionary<string, SongScoreData>();

        foreach (var s in AllScoreData.songScoreData)
        {
            if (s == null || string.IsNullOrEmpty(s.songName)) continue;

            if (!map.TryGetValue(s.songName, out var existing))
            {
                existing = new SongScoreData
                {
                    songName = s.songName,
                    difficultyScoreData = s.difficultyScoreData ?? Array.Empty<DifficultyScoreData>()
                };
                map[s.songName] = existing;
            }
            else
            {
                var combined = (existing.difficultyScoreData ?? Array.Empty<DifficultyScoreData>())
                    .Concat(s.difficultyScoreData ?? Array.Empty<DifficultyScoreData>())
                    .ToArray();

                existing.difficultyScoreData = EnsureAllDifficulties(combined);
            }
        }

        // 欠け補完
        foreach (var kv in map)
            kv.Value.difficultyScoreData = EnsureAllDifficulties(kv.Value.difficultyScoreData);

        AllScoreData.songScoreData = map.Values.ToArray();

        // 既存jsonが分裂していた場合、ここで一度保存して綺麗にする
        if(saveAfter) SaveScores();
    }

    private static SongScoreData CreateDefaultSongScoreData(string songName)
    {
        return new SongScoreData
        {
            songName = songName,
            difficultyScoreData = new[]
            {
                CreateDefaultDiff(Difficulty.Easy),
                CreateDefaultDiff(Difficulty.Normal),
                CreateDefaultDiff(Difficulty.Hard),
            }
        };
    }

    private static DifficultyScoreData CreateDefaultDiff(Difficulty d)
    {
        return new DifficultyScoreData
        {
            difficulty = d,
            score = 0,
            maxCombo = 0,
            goodCount = 0,
            okCount = 0,
            missCount = 0
        };
    }

    /// <summary>
    /// 難易度データ配列を補完・統合する
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    private static DifficultyScoreData[] EnsureAllDifficulties(DifficultyScoreData[] arr)
    {
        var map = new Dictionary<Difficulty, DifficultyScoreData>();

        foreach (var x in arr)
        {
            if (!map.TryGetValue(x.difficulty, out var cur))
            {
                map[x.difficulty] = x;
            }
            else
            {
                // 重複があった場合の統合ルール：スコアが高い方を採用（必要なら変更可）
                if (x.score > cur.score) map[x.difficulty] = x;
            }
        }

        if (!map.ContainsKey(Difficulty.Easy))      map[Difficulty.Easy]    = CreateDefaultDiff(Difficulty.Easy);
        if (!map.ContainsKey(Difficulty.Normal))    map[Difficulty.Normal]  = CreateDefaultDiff(Difficulty.Normal);
        if (!map.ContainsKey(Difficulty.Hard))      map[Difficulty.Hard]    = CreateDefaultDiff(Difficulty.Hard);

        // 並び順固定（見た目も安定）
        return new[] { map[Difficulty.Easy], map[Difficulty.Normal], map[Difficulty.Hard] };
    }





















    void SetScore(SongScoreData songScoreData)
    {
        // AllScoreDataに曲データを設定または更新
        var songList = new List<SongScoreData>(AllScoreData.songScoreData);
        bool found = false;
        for (int i = 0; i < songList.Count; i++)
        {
            if (songList[i].songName == songScoreData.songName)
            {
                songList[i] = songScoreData;
                found = true;
                break;
            }
        }
        if (!found)
        {
            songList.Add(songScoreData);
        }
        AllScoreData.songScoreData = songList.ToArray();
    }



    /// <summary>
    /// 指定した曲のスコアデータを更新
    /// </summary>
    public int UpdateSongScoreData(string songName, Difficulty difficulty, int score, int maxCombo, int goodCount, int okCount, int missCount)
    {
        // これを呼ぶことで、曲データが無ければ作られる＆初期化される
        var currentScore = GetSongScoreData(songName, difficulty);

        if (score <= currentScore)
        {
            Debug.Log($"スコア更新なし: {songName} - 現在:{currentScore}, 新:{score}");
            // スコアの差
            int scoreDifference = score - currentScore;
            return scoreDifference;
        }

        Debug.Log($"スコア更新: {songName} - 現在のスコア: {currentScore}, 新しいスコア: {score}");



        // 念のためnullガード
        if(AllScoreData == null) AllScoreData = new AllScoreData();
        AllScoreData.songScoreData ??= Array.Empty<SongScoreData>();

        // 対象曲を取得（GetSongScoreDataで作られているのでnullにならない想定）
        var song = AllScoreData.songScoreData.First(s => s.songName == songName);


        // 難易度配列を補完（欠け/分裂データ対策）
        song.difficultyScoreData ??= Array.Empty<DifficultyScoreData>();
        song.difficultyScoreData = EnsureAllDifficulties(song.difficultyScoreData);


        // 更新
        var diff = song.difficultyScoreData.First(d => d.difficulty == difficulty);
        diff.score = score;
        diff.maxCombo = maxCombo;
        diff.goodCount = goodCount;
        diff.okCount = okCount;
        diff.missCount = missCount;

        // foreach (var songData in AllScoreData.songScoreData)
        // {
        //     if (songData.songName == songName)
        //     {
        //         foreach (var diffData in songData.difficultyScoreData)
        //         {
        //             if (diffData.difficulty == difficulty)
        //             {
        //                 // 既存のデータを更新
        //                 diffData.score = score;
        //                 diffData.maxCombo = maxCombo;
        //                 diffData.goodCount = goodCount;
        //                 diffData.okCount = okCount;
        //                 diffData.missCount = missCount;
        //                 // AllScoreDataに反映
        //                 SetScore(songData);
        //                 break;
        //             }
        //         }
        //         break;
        //     }
        // }

        Debug.Log($"スコアデータを保存します: {songName} - スコア: {score}");
        SaveScores();
        return score - currentScore;
    }
}
