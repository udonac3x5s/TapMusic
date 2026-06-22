using System;
using System.Collections.Generic;
using common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections; // 追加
using Cysharp.Threading.Tasks;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;
using PlayFab;

public class NotesManager : MonoBehaviour
{
    #region ノーツプール
    [Header("ノーツプール")]
    [SerializeField] private NotesPool notesPool;
    #endregion



    #region パーティクル
    [Header("パーティクル")]
    [SerializeField] private ParticleSystem hitEffectLane0;
    [SerializeField] private ParticleSystem hitEffectLane1;
    [SerializeField] private ParticleSystem hitEffectLane2;
    [SerializeField] private ParticleSystem hitEffectLane3;
    #endregion



    #region ノーツ情報
    [Header("ノーツ情報")]
    public int noteNum;

    public List<int> LaneNum = new();
    public List<int> NoteType = new();
    public List<float> NotesTime = new();     // 譜面時間（秒）
    public List<GameObject> NotesObj = new();

    public bool isPlaying = false;
    public bool isSongPlaying {get ; private set; } = false;

    const string judgeText0 = "Great";
    const string judgeText1 = "Good";
    const string judgeText2 = "Bad";

    int allCombo = 0;


    #endregion

    #region 開始設定
    [Header("開始猶予(ms)")]
    [SerializeField] private float startDelay = 1000f;
    #endregion

    #region 判定設定
    [Header("判定設定")]
    private List<bool> judged = new();
    [SerializeField] private float goodThresholdMs = 50f;
    [SerializeField] private float okThresholdMs = 100f;
    [SerializeField] private float maxJudgeMs = 200f;
    private float awaitTimeMs = 10f;
    private float currentSongTime = 0f;
    private float visualSongTime = 0f; // ← ノーツ移動用
    public float VisualSongTime => visualSongTime;
    #endregion

    #region 参照
    [Header("参照")]
    [SerializeField] private GameObject JudgeText;
    [SerializeField] private Transform judgeTextParent;

    [SerializeField] private Transform judgeTextParent0;
    [SerializeField] private Transform judgeTextParent1;
    [SerializeField] private Transform judgeTextParent2;
    [SerializeField] private Transform judgeTextParent3;

    [SerializeField] private GameObject noteObj;
    // [SerializeField] private AudioSource audioSource;
    #region 一時中断
    private bool isPaused = false;
    private double pauseDspTime;      // 一時停止時のDSP時間
    private float pauseSongTime;      // 一時停止時のcurrentSongTime
    #endregion

    private bool notesLoaded = false;


    public GameSetting gameSetting;
    #endregion

    #region オートMISS
    [Header("オートMISS設定")]
    [SerializeField] private float despawnZ = -2f;
    [SerializeField] private bool autoMissByTime = true;
    [SerializeField] private bool autoMissByPosition = true;
    #endregion

    #region 終了
    [Header("終了設定")]
    private float endTimeSec = float.PositiveInfinity;              // 終了時間（秒）
    private bool ended = false;
    public bool Ended => ended;
    bool isTap = false;
    [SerializeField] private UnityEvent onGameEnd;
    [SerializeField] private GameObject resultPanel;
    #endregion

    #region スコア
    [Header("スコア設定")]
    [SerializeField] private float score;
    [SerializeField] private int combo;
    [SerializeField] private int maxCombo;
    [SerializeField] private float goodScore = 1000;
    [SerializeField] private float noScore = 500;
    [SerializeField] private float missScore = 0;
    [SerializeField] private float maxScore = 1000000;
    [SerializeField] private float goodScoreNow = 1000;
    [SerializeField] private float okScoreNow = 500;
    private int goodCount;
    private int okCount;
    private int missCount;
    #endregion

    #region UI
    [Header("UI参照")]
    [SerializeField] private TMP_Text uploadText;
    [SerializeField] private TMP_Text scoreTextTMP;
    [SerializeField] private TMP_Text comboTextTMP;

    [SerializeField] private TMP_Text resultScoreTMP;
    [SerializeField] private TMP_Text resultScoreDiffTMP;
    [SerializeField] private TMP_Text resultMaxComboTMP;
    [SerializeField] private TMP_Text resultDetailTMP;

    [SerializeField] private TMP_Text resultGoodTMP;
    [SerializeField] private TMP_Text resultOKTMP;
    [SerializeField] private TMP_Text resultMissTMP;
    #endregion

    #region Audio Sync
    private double dspStartTime;
    #endregion

    #region Unity Methods
    void OnEnable()
    {
        gameSetting = Handover.Instance.gameSetting;

        SoundManager.Instance.ResetSettings();
        SoundManager.Instance.LoopSettings(false);

        SoundManager.Instance.PreloadBGM(Handover.Instance.handoverSongData.audioClip);

        ResetScore();
        WaitSongStartAsync(); // ← 先に再生準備

        // UI初期化
        if (scoreTextTMP) scoreTextTMP.text = "0000000";
        if (comboTextTMP) comboTextTMP.text = "";
        resultPanel.SetActive(false);

        // デバッグログ
        Debug.Log($"DSP now: {AudioSettings.dspTime}");
        Debug.Log($"DSP start: {dspStartTime}");
        Debug.Log($"SongTime: {currentSongTime}");
        Debug.Log($"First NoteTime: {NotesTime[0]}");
    }

    void Update()
    {
        if (!isPlaying|| isPaused) return;

        // 表示用時間は常に進める
        visualSongTime = (float)(AudioSettings.dspTime - dspStartTime);

        // 判定用は 0 clamp
        currentSongTime = Mathf.Max(0f, visualSongTime);

        AutoMissSweep();

        // ノーツの表示/非表示切り替え
        UpdateNotesVisibility();

        if (!ended && currentSongTime >= endTimeSec)
            EndGame();
    }
    #endregion

    #region ノーツ表示管理
    /// <summary>
    /// ノーツの表示/非表示切り替え
    /// </summary>
    private void UpdateNotesVisibility()
    {
        if (NotesObj == null) return;

        for (int i = 0; i < NotesObj.Count; i++)
        {
            var note = NotesObj[i];
            if (note == null) continue;

            // Z座標で判定
            float z = note.transform.position.z;
            bool shouldBeActive = z <= 40f;

            // 現在の状態と違う場合だけ切り替える
            if (note.GetComponent<MeshRenderer>().enabled != shouldBeActive)
                note.GetComponent<MeshRenderer>().enabled = shouldBeActive;
        }
    }
    #endregion

    #region 譜面ロード
    private void LoadNotes()
    {
        if (notesLoaded) return;
        notesLoaded = true;

        DataSheet sheet = Handover.Instance.dataSheet;

        noteNum = sheet.notes.Length;               // ノーツ数
        judged = new List<bool>(noteNum);

        float beatSec = 60f / sheet.BPM;            // 1拍秒数

        for (int i = 0; i < sheet.notes.Length; i++)
        {
            var n = sheet.notes[i];

            int sampleRate = AudioSettings.outputSampleRate;        // サンプルレート取得(offset計算用)

            float offsetSec = sheet.offset / (float)sampleRate;     // 譜面オフセット秒

            // ノーツ時間計算
            float time = n.num / // 拍数 +
            (float)n.LPB * // 小節内位置 *
            beatSec + // 拍秒数 +
            offsetSec + // 譜面オフセット秒 +
            gameSetting.NotesOffset * 0.001f + // グローバルオフセット秒(修正予定)
            awaitTimeMs * 0.001f + maxJudgeMs * 0.001f; // 待機時間＋最大判定時間

            // ノーツ情報登録
            LaneNum.Add(n.block);
            NoteType.Add(n.type);
            NotesTime.Add(time);

            // 終了ノーツの設定
            if (n.type == 0)
            {
                endTimeSec = time;      // 終了時間更新
                judged.Add(true);       // 判定済みに設定
                NotesObj.Add(null);     // ダミー登録
                continue;
            }

            judged.Add(false);          // 未判定に設定

            float now = visualSongTime; // 生成時の時間
            float remain = time - now;  // 残り時間(秒)
            float z = remain * gameSetting.NotesSpeed;  // Z座標計算

            // ノーツ生成
            var note = Instantiate(noteObj, new Vector3(n.block - 1.5f, 0.5f, z), Quaternion.identity);

            note.GetComponent<Notes>().Init(time, gameSetting.NotesSpeed, this);
            NotesObj.Add(note);
        }

        // コンボ数計算
        allCombo = NotesObj.Count -1; // 終了ノーツ分引く
        goodScoreNow = maxScore / allCombo;
        okScoreNow = goodScoreNow / 2f;

        goodScore = goodScoreNow;
        noScore = okScoreNow;

    }
    #endregion

    #region 再生開始
    private async void WaitSongStartAsync()
    {
        double delaySec = startDelay * 0.001;                                   // 開始猶予秒
        double musicOffsetSec = gameSetting.MusicOffset * 0.001;                // 音楽オフセット秒

        while (isPaused)
            await UniTask.Yield();                                              // 一時停止中は待機

        // ★ ここで全オフセットを合算して1回だけ決める
        dspStartTime = AudioSettings.dspTime + delaySec + musicOffsetSec +
            awaitTimeMs * 0.001f + maxJudgeMs * 0.001f; // 待機時間＋最大判定時間

        SoundManager.Instance.PlayScheduledBGM(dspStartTime);

        LoadNotes();                                                            // 譜面ロード
        isPlaying = true;

        // slideUI開始
        SlideUIGenerate.Instance.SlideOpen();

        // 曲再生開始待機
        await UniTask.WaitUntil(() => AudioSettings.dspTime >= dspStartTime);
        isSongPlaying = true;
        Debug.Log("Song Started");
    }
    #endregion

    #region 判定
    public void JudgeLane(int lane)
    {
        if (!isPlaying) return;

        float now = currentSongTime;            // 現在の曲時間
        int bestIdx = -1;                       // ベストノーツインデックス
        float bestAbs = float.MaxValue;         // ベスト絶対値差

        float judgeWindowSec = maxJudgeMs * 0.001f;     // 判定ウィンドウ秒
        float earlyWindowSec = maxJudgeMs * 0.001f;                  // 早押し許容

        for (int i = 0; i < noteNum; i++)
        {
            if (judged[i]) continue;
            if (LaneNum[i] != lane) continue;

            float diff = now - NotesTime[i];
            float abs = Mathf.Abs(diff);

            if (diff < -earlyWindowSec) continue; // 未来ノーツ早すぎは無視
            if (diff > judgeWindowSec) continue;  // 遅すぎも無視

            // ベスト更新
            if (abs < bestAbs)
            {
                bestAbs = abs;
                bestIdx = i;
            }
        }

        if (bestIdx < 0) return;

        float diffMs = (now - NotesTime[bestIdx]) * 1000f;
        float absMs = Mathf.Abs(diffMs);

        if (absMs <= goodThresholdMs)
        {
            RegisterHit(bestIdx, judgeText0, goodScore, true, lane);
            PlayHitEffect(lane);
        }
        else if (absMs <= okThresholdMs)
        {
            RegisterHit(bestIdx, judgeText1, noScore, true, lane);
            PlayHitEffect(lane);
        }
        else
            RegisterMiss(bestIdx, lane);
    }
    #endregion

    #region パーティクル
    private void PlayHitEffect(int lane)
    {
        switch (lane)
        {
            case 0:
                if (hitEffectLane0 != null)
                    hitEffectLane0.Play();
                break;
            case 1:
                if (hitEffectLane1 != null)
                    hitEffectLane1.Play();
                break;
            case 2:
                if (hitEffectLane2 != null)
                    hitEffectLane2.Play();
                break;
            case 3:
                if (hitEffectLane3 != null)
                    hitEffectLane3.Play();
                break;
            default:
                Debug.LogWarning("Invalid lane for hit effect: " + lane);
                break;
        }
    }
    #endregion

    #region ヒット処理
    private void RegisterHit(int idx, string label, float addScore, bool comboAdd, int lane)
    {
        judged[idx] = true;
        // Destroy(NotesObj[idx]);
        NotesObj[idx].SetActive(false);
        NotesObj[idx] = null;

        score += addScore;
        combo = comboAdd ? combo + 1 : combo;           // コンボ加算
        maxCombo = Mathf.Max(combo, maxCombo);

        if (label == judgeText0) goodCount++;
        else okCount++;

        SpawnJudgeText(label, LaneNum[idx]);
        UpdateScoreUI();
    }

    /// <summary>
    /// ミス処理
    /// </summary>
    /// <param name="idx"></param>
    private void RegisterMiss(int idx, int lane)
    {
        judged[idx] = true;
        Destroy(NotesObj[idx]);
        NotesObj[idx] = null;

        combo = 0;
        missCount++;

        SpawnJudgeText(judgeText2, lane);
        UpdateScoreUI();
    }

    #region オートMISS
    /// <summary>
    /// オートMISS処理のスイープ
    /// </summary>
    private void AutoMissSweep()
    {
        float now = currentSongTime;
        float missWindow = maxJudgeMs / 1000f;

        for (int i = 0; i < noteNum; i++)
        {
            if (judged[i] || NoteType[i] == 0) continue;

            if (autoMissByTime && now - NotesTime[i] > missWindow)
            {
                RegisterMiss(i, LaneNum[i]);
                //RegisterHit(i, judgeText0, goodScore, true, LaneNum[i]);
            }
        }
    }
    #endregion

    #endregion

    #region 中断機能
    /// <summary>
    /// ゲームを一時停止する
    /// </summary>
    public void PauseGame()
    {
        if (!isPlaying || isPaused) return;

        isPaused = true;
        pauseDspTime = AudioSettings.dspTime;
        pauseSongTime = currentSongTime;

        SoundManager.Instance.PauseBGM(); // AudioSource 側でPause
    }

    /// <summary>
    /// ゲームを再開する
    /// </summary>
    public void ResumeGame()
    {
        if (!isPlaying || !isPaused) return;

        isPaused = false;
        // DSP開始時間を現在時間にずらして補正
        dspStartTime += AudioSettings.dspTime - pauseDspTime;

        SoundManager.Instance.ResumeBGM(); // AudioSource 側でResume
    }
    #endregion

    #region UI
    /// <summary>
    /// 判定テキスト生成
    /// </summary>
    private void SpawnJudgeText(string text, int lane = -1)
    {
        if (JudgeText == null) return;

        GameObject judgeTextPool = JudgeTextPool.Instance.pool.Get();
        switch (lane)
        {
            case 0:
                judgeTextPool.transform.SetParent(judgeTextParent0, false);
                break;
            case 1:
                judgeTextPool.transform.SetParent(judgeTextParent1, false);
                break;
            case 2:
                judgeTextPool.transform.SetParent(judgeTextParent2, false);
                break;
            case 3:
                judgeTextPool.transform.SetParent(judgeTextParent3, false);
                break;
            default:
                judgeTextPool.transform.SetParent(judgeTextParent, false);
                break;
        }


        // var go = Instantiate(JudgeText, judgeTextParent);
        judgeTextPool.GetComponent<TMP_Text>().text = text;
        judgeTextPool.GetComponent<TMP_Text>().color = text switch
        {
            judgeText0 => Color.yellow,
            judgeText1 => Color.green,
            judgeText2 => Color.red,
            _ => Color.white
        };

        // 一定時間後にプールに返却
        UniTask.Delay(200).ContinueWith(() =>
        {
            JudgeTextPool.Instance.pool.Release(judgeTextPool);
        });
        judgeTextPool.transform.DOLocalMoveY(judgeTextPool.transform.localPosition.y + 50f, 0.1f).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// スコアUI更新
    /// </summary>
    private void UpdateScoreUI()
    {
        // スコア上限処理
        if (score > 999900.0f) score = 1000000.0f;


        // Mathf.CeilToIntとは, 小数点以下を切り上げて整数に変換する関数
        if (scoreTextTMP) scoreTextTMP.text =  Mathf.CeilToInt(score).ToString("D7");           // スコア表示更新

        // コンボ表示更新
        if (comboTextTMP)
        {
            if(combo > 1)
                comboTextTMP.text = combo.ToString();
            else
                comboTextTMP.text = "";
        }
    }

    /// <summary>
    /// スコアリセット
    /// </summary>
    private void ResetScore()
    {
        score = combo = maxCombo = 0;
        goodCount = okCount = missCount = 0;
    }
    #endregion

    #region 終了
    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    private async void EndGame()
    {
        // 終了処理
        ended = true;
        isPlaying = false;
        SoundManager.Instance.StopBGM();

        // 結果表示
        SlideUIGenerate.Instance.SlideClose();

        await UniTask.Delay(1500);

        resultPanel.SetActive(true);

        ScoreUpdate();
        resultScoreTMP.text = Mathf.CeilToInt(score).ToString("D7");
        resultMaxComboTMP.text = maxCombo.ToString();
        // resultDetailTMP.text =
        //     $"GOOD {goodCount}\nOK {noCount}\nMISS {missCount}";


        resultGoodTMP.text = goodCount.ToString();
        resultOKTMP.text = okCount.ToString();
        resultMissTMP.text = missCount.ToString();

        SlideUIGenerate.Instance.SlideOpen();

        // スコアアップロード
        if(PlayFabClientAPI.IsClientLoggedIn()) // ← ログイン済みなら
        {
            ScoreUploader.Instance.UploadScore(Handover.Instance.handoverSongData.name,Handover.Instance.nowDifficulty.ToString(),Mathf.CeilToInt(score));

            // すぐランキング見たいなら
            RankingLoader.Instance.LoadRanking(Handover.Instance.handoverSongData.name, Handover.Instance.nowDifficulty.ToString(), 10);
            uploadText.text = "アップロードしました";
        }
        else
        {
            Debug.Log("ログインしていないためスコアをアップロードできません");
            uploadText.text = "ログインしていないためアップロードできません";
        }



        // イベント発火
        onGameEnd?.Invoke();
    }

    void ScoreUpdate()
    {
        Debug.Log("Score Update Called");
        float scoreDifference = ScoreManager.Instance.UpdateSongScoreData(
            Handover.Instance.handoverSongData.name,
            Handover.Instance.nowDifficulty,
            Mathf.CeilToInt(score),
            maxCombo,
            goodCount,
            okCount,
            missCount
        );
        resultScoreDiffTMP.text = (scoreDifference >= 0 ? "+" : "") +  Mathf.Ceil(scoreDifference).ToString();
    }
    #endregion



    public float CurrentSongTime => currentSongTime;

    public void SceneMove()
    {
        if (isTap) return;
        isTap = true;
        SlideUIGenerate.Instance.SceneMove("Menu").Forget();
    }

    public void RetryResult()
    {
        if (isTap) return;
        isTap = true;
        SlideUIGenerate.Instance.SceneMove("Game").Forget();
    }

}
