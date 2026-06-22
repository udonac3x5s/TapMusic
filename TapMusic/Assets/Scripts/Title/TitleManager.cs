using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    #region Variables
    [SerializeField] Button startButton;
    [SerializeField, Tooltip("スタートボタン")] GameObject lockObject;
    bool isTap = false;

    #endregion

    #region Unity Methods
    async void Start()
    {
        Init();
    }
    #endregion

    #region Methods
    void Init()
    {
        startButton.onClick.AddListener(() =>
        {
            if (isTap) return;
            isTap = true;
            SlideUIGenerate.Instance.SceneMove("Menu").Forget();
        });

        // フレームレート設定
        Application.targetFrameRate = 120;

        // VSync無効化
        QualitySettings.vSyncCount = 0;

        UniTask.Delay(1000).ContinueWith(() =>
        {
            lockObject.SetActive(false);
        });

    }

    void CloseInit()
    {
        SlideUIGenerate.Instance.SlideOpen();
    }
    #endregion
}
