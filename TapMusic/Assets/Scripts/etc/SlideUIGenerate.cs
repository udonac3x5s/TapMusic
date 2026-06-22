using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SlideUIGenerate : MonoBehaviour
{
    #region Variables
    private GameObject slideUI;
    [SerializeField] private float slideOpenSpeed = 1.0f;
    [SerializeField] private float slideCloseSpeed = 0.7f;

    private Transform slideLeft;
    private Transform slideRight;

    private AudioClip closeSE;
    private AudioClip openSE;

    public bool IsClose { get; private set; } = false;
    #endregion

    #region Singleton
    private static SlideUIGenerate instance;

    public static SlideUIGenerate Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("Error");
            }
            return instance;
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SoundManager.Instance.ResetSettings();
        Initialize();
        InitializeAsync().Forget();
    }

    private void Initialize()
    {
        var canvas = GetComponentInChildren<Canvas>().gameObject;
        if (canvas == null)
        {
            Debug.LogError("Canvasが見つかりません。");
            return;
        }
        slideUI = canvas;
        if (slideUI.transform.childCount < 2)
        {
            Debug.LogError("slideUI の子要素が2つ未満です。");
            return;
        }

        slideLeft = slideUI.transform.GetChild(0);
        slideRight = slideUI.transform.GetChild(1);
    }
    #endregion

    #region Methods
    /// <summary>
    /// スライドUIClose
    /// </summary>
    public async UniTask SceneMove(string sceneName, bool openAfterLoad = false)
    {
        SlideClose();

        // シーン移動
        await UniTask.Delay((int)(slideCloseSpeed * 1100));
        SoundManager.Instance.StopBGM();
        await SceneManager.LoadSceneAsync(sceneName);

        // シーン移動後にスライドUIを開く
        if (openAfterLoad)
        {
            SlideOpen();
        }
    }

    public void SlideClose()
    {
        IsClose = true;
        PlayCloseAnimation();
    }

    /// <summary>
    /// スライドUIOpen
    /// </summary>
    public void SlideOpen()
    {
        IsClose = false;
        PlayOpenAnimation();
    }
    #endregion


    #region Slide Methods
    private void PlayOpenAnimation()
    {
        // UniTask.Delay(500).Forget();

        SoundManager.Instance.PlaySE(openSE);

        // DoTweenを使って移動
        slideLeft.DOLocalMoveX(-2500f, slideOpenSpeed).SetEase(Ease.InOutSine);
        slideRight.DOLocalMoveX(1500f, slideOpenSpeed).SetEase(Ease.InOutSine);

        // // SEのリリース Addressable
        // UniTask.Delay(400).ContinueWith(() =>
        // {
        //     Addressables.Release(open);
        // });
    }

    private void PlayCloseAnimation()
    {
        SoundManager.Instance.PlaySE(closeSE);

        // DoTweenを使って移動
        slideLeft.DOLocalMoveX(0f, slideCloseSpeed).SetEase(Ease.InOutSine);
        slideRight.DOLocalMoveX(0f, slideCloseSpeed).SetEase(Ease.InOutSine);

        // // SEのリリース Addressable
        // UniTask.Delay(400).ContinueWith(() =>
        // {
        //     Addressables.Release(close);
        // });

    }
    #endregion

    #region  UniTasks Methods
    private async UniTask InitializeAsync()
    {
        // Addressables 初期化
        await Addressables.InitializeAsync();

        // SE 読み込み
        await LoadSEAsync();
    }

    /// <summary>
    /// SEの読み込み
    /// </summary>
    private async UniTask LoadSEAsync()
    {
        closeSE = await LoadClipAsync(
            "Loading/SE/Close");

        openSE = await LoadClipAsync(
            "Loading/SE/Open");
    }

    /// <summary>
    /// AddressablesからAudioClipを非同期で読み込む
    /// </summary>
    private async UniTask<AudioClip> LoadClipAsync(string key)
    {
        var handle = Addressables.LoadAssetAsync<AudioClip>(key);
        await handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"SE Load Failed : {key}");
            return null;
        }

        return handle.Result;
    }
    #endregion

}
