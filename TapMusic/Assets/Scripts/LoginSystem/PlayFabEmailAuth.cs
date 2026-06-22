using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabEmailAuth : MonoBehaviour
{
    #region Private Variables
    [SerializeField] private string titleIdDebug; // （任意）デバッグ用。基本はPlayFabSharedSettingsに入れる
    #endregion

    /// <summary>
    /// 新規登録（Email + Username + Password）
    /// </summary>
    public void Register(string email, string username, string password)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            result =>
            {
                Debug.Log($"登録成功. PlayFabId={result.PlayFabId}");
                // 必要なら表示名を username に揃える（任意）
                SetDisplayName(username);
            },
            error =>
            {
                Debug.LogError($"登録エラー: {error.GenerateErrorReport()}");
            }
        );
    }

    /// <summary>
    /// 表示名を設定する（任意）
    /// </summary>
    private void SetDisplayName(string displayName)
    {
        var req = new UpdateUserTitleDisplayNameRequest { DisplayName = displayName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(
            req,
            r => Debug.Log($"表示名設定成功: {r.DisplayName}"),
            e => Debug.LogWarning($"表示名設定警告: {e.GenerateErrorReport()}")
        );
    }

    #region Login Methods
    /// <summary>
    /// メールアドレスでログイン（Email + Password）
    /// </summary>
    public void LoginWithEmail(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(
            request,
            result => Debug.Log($"ログイン成功. PlayFabId={result.PlayFabId}"),
            error => Debug.LogError($"ログインエラー: {error.GenerateErrorReport()}")
        );
    }


    /// <summary>
    /// ユーザーネームでログイン（Username + Password）
    /// </summary>
    public void LoginWithUsername(string username, string password)
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(
            request,
            result => Debug.Log($"ログイン成功. PlayFabId={result.PlayFabId}"),
            error => Debug.LogError($"ログインエラー: {error.GenerateErrorReport()}")
        );
    }
    #endregion

}
