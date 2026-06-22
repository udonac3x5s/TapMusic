using UnityEngine;
using common;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System;
using TMPro;
using Cysharp.Threading.Tasks;

public class GameSettings : MonoBehaviour
{
    #region Variables
    [Header("パネル設定")]
    [SerializeField] float alphaSet;
    [SerializeField] float alphaSpeed;

    [Header("設定項目リスト")]
    [SerializeField] public List<SettingButton> settingButton;

    [SerializeField] AnimationClip openWindow;
    [SerializeField] AnimationClip closeWindow;

    string filePath;

    [SerializeField] GameObject settingPanelAll;
    [SerializeField] GameObject settingMovePanel;

    // ノーツスピード調整値
    float notesSpeedMini = 0.1f;
    float notesSpeedBig = 1f;
    // ノーツオフセット調整値
    float notesOffsetMini = 1f;
    float notesOffsetBig = 10f;
    // 音楽オフセット調整値
    float musicOffsetMini = 1f;
    float musicOffsetBig = 10f;


    public bool IsSettingPanelOpen { get; private set; } = false;

    [SerializeField] private SettingNotesManager settingNotesManager; // 設定画面のプレビュー用

    bool isTap = false;
    #endregion

    #region Unity Methods
    async void Start()
    {
        await SettingLoad();
        SetButtonAddListener();
    }
    #endregion

    #region ボタンリスナー登録
    void SetButtonAddListener()
    {
        foreach (var button in settingButton)
        {
            switch (button.settingType)
            {
                case SettingType.NotesSpeed:
                    // settingTmpObjの子オブジェクトのボタンが押されたら
                    // 00 +0.1
                    button.settingTmpObj.transform.GetChild(0).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesSpeed(button.settingTmpObj, notesSpeedMini);
                    });
                    // 01 +1.0
                    button.settingTmpObj.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesSpeed(button.settingTmpObj, notesSpeedBig);
                    });
                    // 02 -0.1
                    button.settingTmpObj.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesSpeed(button.settingTmpObj, -notesSpeedMini);
                    });
                    // 03 -1.0
                    button.settingTmpObj.transform.GetChild(3).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesSpeed(button.settingTmpObj, -notesSpeedBig);
                    });
                    break;

                case SettingType.NotesOffset:
                    // 00 +0.1
                    button.settingTmpObj.transform.GetChild(0).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesOffset(button.settingTmpObj, notesOffsetMini);
                    });
                    // 01 +1.0
                    button.settingTmpObj.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesOffset(button.settingTmpObj, notesOffsetBig);
                    });
                    // 02 -0.1
                    button.settingTmpObj.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesOffset(button.settingTmpObj, -notesOffsetMini);
                        NotesOffsetSettingUIUpdate(Handover.Instance, button.settingTmpObj);
                    });
                    // 03 -1.0
                    button.settingTmpObj.transform.GetChild(3).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetNotesOffset(button.settingTmpObj, -notesOffsetBig);
                    });
                    break;

                case SettingType.MusicOffset:
                    // 00 +0.1
                    button.settingTmpObj.transform.GetChild(0).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetMusicOffset(button.settingTmpObj, musicOffsetMini);
                    });
                    // 01 +1.0
                    button.settingTmpObj.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetMusicOffset(button.settingTmpObj, musicOffsetBig);
                    });
                    // 02 -0.1
                    button.settingTmpObj.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetMusicOffset(button.settingTmpObj, -musicOffsetMini);
                    });
                    // 03 -1.0
                    button.settingTmpObj.transform.GetChild(3).gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetMusicOffset(button.settingTmpObj, -musicOffsetBig);
                    });
                    break;

                case SettingType.Reset:
                    button.settingTmpObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log("設定リセット");
                        Handover.Instance.gameSetting = new GameSetting
                        {
                            NotesSpeed = Const.RESET_NOTES_SPEED,
                            NotesOffset = Const.RESET_NOTES_OFFSET,
                            MusicOffset = Const.RESET_MUSIC_OFFSET
                        };
                        SettingWrite();
                        // UI更新
                        foreach (var btn in settingButton)
                        {
                            switch (btn.settingType)
                            {
                                case SettingType.NotesSpeed:
                                    btn.settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = Handover.Instance.gameSetting.NotesSpeed.ToString("0.0");
                                    break;
                                case SettingType.NotesOffset:
                                    btn.settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = Handover.Instance.gameSetting.NotesOffset.ToString("0");
                                    break;
                                case SettingType.MusicOffset:
                                    btn.settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = Handover.Instance.gameSetting.MusicOffset.ToString("0");
                                    break;
                                default:
                                    break;
                            }
                        }
                    });
                    break;

                case SettingType.SettingSwitch:
                    Debug.Log("設定パネルの表示切替リスナー登録");
                    button.settingTmpObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (button.settingTmpObj == null) return;
                        Debug.Log("設定パネルの表示切替");

                        // if (button.settingOn) settingPanel.SetActive(true);                 // 強制ON
                        // else if (button.settingOff) settingPanel.SetActive(false);          // 強制OFF
                        // else settingPanel.SetActive(!settingPanel.activeSelf);              // トグル
                        if (settingPanelAll.activeSelf) CloseSettingPanel().Forget();
                        else OpenSettingPanel().Forget();
                    });
                    break;

                default:
                    break;
            }
        }
    }
    #endregion

    #region ボタン処理
    /// <summary>
    /// ノーツスピード設定
    /// </summary>
    void SetNotesSpeed(GameObject settingTmpObj, float speed)
    {
        Handover.Instance.gameSetting.NotesSpeed += speed;
        NotesSpSettingUIUpdate(Handover.Instance, settingTmpObj);
    }

    /// <summary>
    /// ノーツオフセット設定
    /// </summary>
    void SetNotesOffset(GameObject settingTmpObj, float offset)
    {
        Handover.Instance.gameSetting.NotesOffset += offset;
        NotesOffsetSettingUIUpdate(Handover.Instance, settingTmpObj);
    }

    /// <summary>
    /// 音楽オフセット設定
    /// </summary>
    void SetMusicOffset(GameObject settingTmpObj, float offset)
    {
        Handover.Instance.gameSetting.MusicOffset += offset;
        MusicOffsetSettingUIUpdate(Handover.Instance, settingTmpObj);
    }
    #endregion

    #region 長押し





    #endregion

    #region ノーツ設定UI更新
    void NotesSpSettingUIUpdate(Handover handover, GameObject settingTmpObj)
    {
        // // 四捨五入
        // handover.gameSetting.NotesSpeed = Mathf.Round(handover.gameSetting.NotesSpeed * 10f) / 10f;
        // if (handover.gameSetting.NotesSpeed >= Const.MAX_NOTES_SPEED) handover.gameSetting.NotesSpeed = Const.MAX_NOTES_SPEED;
        // if (handover.gameSetting.NotesSpeed <= Const.MIN_NOTES_SPEED) handover.gameSetting.NotesSpeed = Const.MIN_NOTES_SPEED;
        // Debug.Log("ノーツスピード:" + handover.gameSetting.NotesSpeed);

        // // 04 Text
        // settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = handover.gameSetting.NotesSpeed.ToString("0.0");

        handover.gameSetting.NotesSpeed = Mathf.Round(handover.gameSetting.NotesSpeed * 10f) / 10f;
        if (handover.gameSetting.NotesSpeed >= Const.MAX_NOTES_SPEED) handover.gameSetting.NotesSpeed = Const.MAX_NOTES_SPEED;
        if (handover.gameSetting.NotesSpeed <= Const.MIN_NOTES_SPEED) handover.gameSetting.NotesSpeed = Const.MIN_NOTES_SPEED;

        settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text =
            handover.gameSetting.NotesSpeed.ToString("0.0");

        // プレビュー反映（パネル開いてる時だけ）
        if (IsSettingPanelOpen && settingNotesManager != null)
        {
            settingNotesManager.SetNotesSpeed(handover.gameSetting.NotesSpeed);
        }


    }
    #endregion

    #region ノーツオフセット設定UI更新
    void NotesOffsetSettingUIUpdate(Handover handover, GameObject settingTmpObj)
    {
        // 四捨五入
        handover.gameSetting.NotesOffset = Mathf.Round(handover.gameSetting.NotesOffset * 10f) / 10f;
        if (handover.gameSetting.NotesOffset >= Const.MAX_NOTES_OFFSET) handover.gameSetting.NotesOffset = Const.MAX_NOTES_OFFSET;
        if (handover.gameSetting.NotesOffset <= Const.MIN_NOTES_OFFSET) handover.gameSetting.NotesOffset = Const.MIN_NOTES_OFFSET;
        Debug.Log("ノーツオフセット:" + handover.gameSetting.NotesOffset);

        // 04 Text
        settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = handover.gameSetting.NotesOffset.ToString("0");
    }
    #endregion

    #region 音楽オフセット設定UI更新
    void MusicOffsetSettingUIUpdate(Handover handover, GameObject settingTmpObj)
    {
        // 四捨五入
        handover.gameSetting.MusicOffset = Mathf.Round(handover.gameSetting.MusicOffset * 1f) / 1f;
        if (handover.gameSetting.MusicOffset >= Const.MAX_MUSIC_OFFSET) handover.gameSetting.MusicOffset = Const.MAX_MUSIC_OFFSET;
        if (handover.gameSetting.MusicOffset <= Const.MIN_MUSIC_OFFSET) handover.gameSetting.MusicOffset = Const.MIN_MUSIC_OFFSET;
        Debug.Log("音楽オフセット:" + handover.gameSetting.MusicOffset);

        // 04 Text
        settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = handover.gameSetting.MusicOffset.ToString("0");
    }
    #endregion

    #region 設定読み込み
    /// <summary>
    /// 設定読み込み
    /// </summary>
    /// <returns></returns>
    private async Task SettingLoad()
    {
        filePath = Path.Combine(Application.persistentDataPath, Const.SETTING_FILE_NAME);
        Debug.Log("設定ファイルパス: " + filePath);

        GameSetting gameSetting;
        if (File.Exists(filePath))
        {
            var jsonText = File.ReadAllText(filePath);
            gameSetting = JsonUtility.FromJson<GameSetting>(jsonText);
        }
        else
        {
            // ファイルがなければデフォルト値で初期化
            gameSetting = new GameSetting();
            File.WriteAllText(filePath, JsonUtility.ToJson(gameSetting, true));
        }

        Handover.Instance.gameSetting = gameSetting;

        // UIの初期化
        foreach (var button in settingButton)
        {
            switch (button.settingType)
            {
                case SettingType.NotesSpeed:
                    button.settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = Handover.Instance.gameSetting.NotesSpeed.ToString("0.0");
                    break;
                case SettingType.NotesOffset:
                    button.settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = Handover.Instance.gameSetting.NotesOffset.ToString("0");
                    break;
                case SettingType.MusicOffset:
                    button.settingTmpObj.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = Handover.Instance.gameSetting.MusicOffset.ToString("0");
                    break;
                default:
                    break;
            }
        }
        await Task.CompletedTask;
    }

    void SettingWrite()
    {
        string json = JsonUtility.ToJson(Handover.Instance.gameSetting, true);
        File.WriteAllText(filePath, json);
        Debug.Log("設定データを書き込みました: " + filePath);
    }
    #endregion

    #region パネルオープン
    public async UniTaskVoid OpenSettingPanel()
    {
        if (isTap) return;
        isTap = true;

        settingPanelAll.SetActive(true);
        settingMovePanel.SetActive(true);

        EnableBGShadowUpdateAsync(settingPanelAll).Forget();

        var anim = settingMovePanel.GetComponent<Animator>();
        anim.Play(openWindow.name);
        // アニメーション終了を待つ（1フレーム待ってから）
        await UniTask.Yield();
        float clipLength = anim.GetCurrentAnimatorStateInfo(0).length;

        await UniTask.Delay(TimeSpan.FromSeconds(clipLength));
        isTap = false;
        IsSettingPanelOpen = true;

        // 開いた瞬間に今の設定でプレビューを作り直す
        if (settingNotesManager != null)
        {
            settingNotesManager.InitPreview(Handover.Instance.gameSetting.NotesSpeed);
        }
    }
    #endregion

    #region パネルクローズ
    public async UniTaskVoid CloseSettingPanel()
    {
        if (isTap) return;
        isTap = true;

        // 書き込み
        SettingWrite();


        Animator anim = settingMovePanel.GetComponent<Animator>();
        anim.Play(closeWindow.name);

        DisableBGShadowUpdateAsync(settingPanelAll).Forget();

        // アニメーション終了を待つ（1フレーム待ってから）
        await UniTask.Yield();
        float clipLength = anim.GetCurrentAnimatorStateInfo(0).length;

        await UniTask.Delay(TimeSpan.FromSeconds(clipLength));

        settingPanelAll.SetActive(false);
        settingMovePanel.SetActive(false);
        isTap = false;
        IsSettingPanelOpen = false;
    }
    #endregion

    #region 背景シャドウエフェクト
    /// <summary>
    /// 背景シャドウエフェクト無効化
    /// </summary>
    async UniTask DisableBGShadowUpdateAsync(GameObject bg)
    {
        Image image = bg.transform.GetChild(0).GetComponent<Image>();
        image.color = new Vector4(0, 0, 0, 150f / 255f);
        float nowAlpha = alphaSet / 255f;

        for (; ; )
        {
            nowAlpha -= alphaSpeed / 255f;
            image.color = new Vector4(0, 0, 0, nowAlpha);

            if (nowAlpha <= 20f / 255f)
                break;

            await Task.Delay(TimeSpan.FromSeconds(1f / 60f));
        }

        image.color = new Vector4(0, 0, 0, 0);
    }

    /// <summary>
    /// 背景シャドウエフェクト有効化
    /// </summary>
    async UniTask EnableBGShadowUpdateAsync(GameObject bg)
    {
        Image image = bg.transform.GetChild(0).GetComponent<Image>();
        image.color = new Vector4(0, 0, 0, 0);
        float nowAlpha = 0;

        for (; ; )
        {
            nowAlpha += alphaSpeed / 255f;
            image.color = new Vector4(0, 0, 0, nowAlpha);

            if (nowAlpha >= alphaSet / 255f)
                break;

            await Task.Delay(TimeSpan.FromSeconds(1f / 60f));
        }

        image.color = new Vector4(0, 0, 0, alphaSet / 255f);
    }
    #endregion






}
