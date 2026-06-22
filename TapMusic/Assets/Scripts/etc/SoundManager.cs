using UnityEngine;
using System.IO;
using common;

public class SoundManager : MonoBehaviour
{
    #region Singleton
    private static SoundManager instance;

    /// <summary>
    /// SoundManagerインスタンス取得
    /// </summary>
    public static SoundManager Instance
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

    #region Variables
    private AudioSource bgmSource;
    private AudioSource seSource;
    private AudioClip tapSE;
    private SoundSettings settings;


    // private を上に（好みの並び）
    private AudioSource[] sources;
    private int index;
    private int poolSize = 16;

    void Test()
    {
        sources = new AudioSource[poolSize];
        tapSE = Resources.Load<AudioClip>("Sounds/SE/Tes");

        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            src.clip = tapSE;

            sources[i] = src;
        }
    }

    /// <summary>
    /// タップSEを即時再生する
    /// </summary>
    public void PlayTap()
    {
        if (tapSE == null) return;

        var src = sources[index];

        // 連打で同じソースが被っても確実に鳴らす
        src.Stop();
        src.time = 0f;
        src.Play();

        index++;
        if (index >= sources.Length) index = 0;
    }

    /// <summary>
    /// タップSEの音量を設定する（0〜1）
    /// </summary>
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].volume = volume;
        }
    }

    // 設定JSON保存パス
    private string SavePath;
    #endregion

    #region Initialization
    /// <summary>
    /// SoundManagerを生成する
    /// </summary>
    private static void CreateInstance()
    {
        GameObject obj = new("SoundManager");
        instance = obj.AddComponent<SoundManager>();
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

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Initialize()
    {
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        seSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        seSource.playOnAwake = false;

        SavePath = Path.Combine(Application.persistentDataPath, Const.SOUND_SETTINGS_FILE_NAME);

        LoadSoundSettings();
        ApplyVolume();
        // Test();
    }
    #endregion

    #region Public Methods

    #region BGM
    /// <summary>
    /// BGMを再生する
    /// </summary>
    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmClip == null)
        {
            Debug.LogWarning("BGMクリップがnullです");
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    /// <summary>
    /// BGMを停止する
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// BGMを事前ロードする
    /// </summary>
    public void PreloadBGM(AudioClip bgmClip)
    {
        if (bgmClip == null)
        {
            Debug.LogWarning("BGMクリップがnullです");
            return;
        }

        bgmSource.clip = bgmClip;
    }

    /// <summary>
    /// 事前ロードしたBGMを再生する
    /// </summary>
    public void PlayPreloadBGM()
    {
        bgmSource.Play();
    }


    /// <summary>
    /// 指定したDSP時間にBGMを再生する
    /// </summary>
    public void PlayScheduledBGM(double dspTime)
    {
        if (bgmSource.clip == null)
        {
            Debug.LogError("BGMクリップがnullです");
            return;
        }
        bgmSource.PlayScheduled(dspTime);
    }


    /// <summary>
    /// BGMを一時停止する
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource.isPlaying) bgmSource.Pause();
    }

    /// <summary>
    /// BGMを再開する
    /// </summary>
    public void ResumeBGM()
    {
        if (!bgmSource.isPlaying) bgmSource.UnPause();
    }
    #endregion

    #region SE
    /// <summary>
    /// SEを再生する
    /// </summary>
    public void PlaySE(AudioClip seClip)
    {
        if (seClip == null)
        {
            Debug.LogWarning("SEクリップがnullです");
            return;
        }

        seSource.PlayOneShot(seClip);
    }

    /// <summary>
    /// SEを停止する
    /// </summary>
    public void StopSE()
    {
        seSource.Stop();
    }



    #endregion






    /// <summary>
    /// 現在の設定を取得する（UI用）
    /// </summary>
    public SoundSettings GetSettings()
    {
        return settings;
    }

    /// <summary>
    /// 音量を設定して保存する
    /// </summary>
    public void SetVolume(float master, float bgm, float se)
    {
        settings.masterVolume = master;
        settings.bgmVolume = bgm;
        settings.seVolume = se;

        ApplyVolume();
        SaveSoundSettings();
    }

    /// <summary>
    /// 設定を初期値に戻す
    /// </summary>
    public void ResetSettings()
    {
        settings = new();
        ApplyVolume();
        SaveSoundSettings();
    }

    /// <summary>
    /// BGMのループ設定を行う
    /// </summary>
    public void LoopSettings(bool loop)
    {
        bgmSource.loop = loop;
    }
    #endregion

    #region ロード/セーブ関連
    /// <summary>
    /// サウンド設定をロードする
    /// 優先度：persistentDataPath → Resources
    /// </summary>
    private void LoadSoundSettings()
    {
        // ユーザー保存データ優先
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            settings = JsonUtility.FromJson<SoundSettings>(json);
            return;
        }
        else
        {
            // 新しくファイルを作成
            settings = new();
            SaveSoundSettings();
            return;
        }
    }

    /// <summary>
    /// サウンド設定を保存する
    /// </summary>
    private void SaveSoundSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(SavePath, json);
    }

    /// <summary>
    /// AudioSourceに音量を反映する
    /// </summary>
    private void ApplyVolume()
    {
        bgmSource.volume = settings.masterVolume * settings.bgmVolume;
        seSource.volume = settings.masterVolume * settings.seVolume;
    }
    #endregion
}
