using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabAutoLogin : MonoBehaviour
{
    private const string CUSTOM_ID_KEY = "PLAYFAB_CUSTOM_ID";
    private static PlayFabAutoLogin instance;

    private bool isLoggingIn = false;

    private void Awake()
    {
        // 多重生成防止（DontDestroyOnLoad とセットで必須）
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        TryAutoLogin();
    }

    /// <summary>
    /// 保存済みCustomIdがある場合のみ自動ログインする
    /// </summary>
    public void TryAutoLogin()
    {
        if (isLoggingIn) return;
        if (PlayFabClientAPI.IsClientLoggedIn()) return;

        if (!PlayerPrefs.HasKey(CUSTOM_ID_KEY))
        {
            Debug.Log("オートログインのCustomIdが保存されていません。");
            return;
        }

        string customId = PlayerPrefs.GetString(CUSTOM_ID_KEY);
        if (string.IsNullOrWhiteSpace(customId))
        {
            Debug.Log("オートログインのCustomIdが空です。");
            return;
        }

        Debug.Log($"オートログイン CustomId = {customId}");

        isLoggingIn = true;

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = false // ← 重要：無いIDで新規作成しない
        };

        PlayFabClientAPI.LoginWithCustomID(
            request,
            result =>
            {
                isLoggingIn = false;
                Debug.Log($"オートログイン成功. PlayFabId = {result.PlayFabId}");
            },
            error =>
            {
                isLoggingIn = false;
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    /// <summary>
    /// 手動ログイン/初回作成が成功したタイミングでCustomIdを保存する
    /// （※あなたの「初回ログイン処理」の成功コールバックで呼ぶ）
    /// </summary>
    public void SaveCustomId(string customId)
    {
        if (string.IsNullOrWhiteSpace(customId)) return;
        PlayerPrefs.SetString(CUSTOM_ID_KEY, customId);
        PlayerPrefs.Save();
        Debug.Log($"CustomId 保存成功: {customId}");
    }

    /// <summary>
    /// デバッグ用：自動ログイン情報を消す
    /// </summary>
    [ContextMenu("DEBUG: カスタムID削除")]
    public void DebugClearSavedCustomId()
    {
        PlayerPrefs.DeleteKey(CUSTOM_ID_KEY);
        PlayerPrefs.Save();
        Debug.Log("CustomId 削除成功");
    }
}
