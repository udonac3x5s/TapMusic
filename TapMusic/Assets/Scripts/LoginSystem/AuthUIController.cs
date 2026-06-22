using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class AuthUIController : MonoBehaviour
{
    #region Private Variables
    [Header("Tabs")]
    [SerializeField] private Button btnLoginTab;                 // ログインタブボタン
    [SerializeField] private Button btnRegisterTab;              // 登録タブボタン

    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;              // ログインパネル
    [SerializeField] private GameObject registerPanel;           // 登録パネル

    [Header("Login UI (TMP)")]
    [SerializeField] private TMP_InputField loginEmailOrUser;    // メール or ユーザーネーム
    [SerializeField] private TMP_InputField loginPassword;       // パスワード
    [SerializeField] private Button btnLoginSubmit;              // ログイン実行
    [SerializeField] private TMP_Text txtLoginMsg;               // メッセージ表示

    [Header("Register UI (TMP)")]
    [SerializeField] private TMP_InputField regEmail;            // メール
    [SerializeField] private TMP_InputField regUsername;         // ユーザーネーム
    [SerializeField] private TMP_InputField regPassword;         // パスワード
    [SerializeField] private TMP_InputField regPasswordConfirm;  // パスワード確認
    [SerializeField] private Button btnRegisterSubmit;           // 登録実行
    [SerializeField] private TMP_Text txtRegisterMsg;            // メッセージ表示

    [Header("Options")]
    [SerializeField] private bool loginWithEmailPreferred = true; // 入力がメールっぽい時はEmailログイン優先
    [SerializeField] private int minPasswordLength = 8;           // 最低パスワード長
    [SerializeField] private int maxPlayFabUsernameLength = 15;    // PlayFabのユーザーネーム最大長


    bool loginPanelActive = true;
    bool registerPanelActive = false;

    private bool isBusy = false; // 多重送信防止
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        btnLoginTab.onClick.AddListener(ShowLogin);
        btnRegisterTab.onClick.AddListener(ShowRegister);

        btnLoginSubmit.onClick.AddListener(OnClickLogin);
        btnRegisterSubmit.onClick.AddListener(OnClickRegister);

        ShowLogin();
    }
    #endregion

    #region UI Control
    /// <summary>
    /// ログイン画面を表示
    /// </summary>
    private void ShowLogin()
    {
        if (loginPanelActive)
        {
            loginPanel.SetActive(false);
            registerPanel.SetActive(false);
            loginPanelActive = false;
            registerPanelActive = false;
            return;
        }
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        loginPanelActive = true;
        registerPanelActive = false;

        SetLoginMsg("");
        SetRegisterMsg("");
    }

    /// <summary>
    /// 登録画面を表示
    /// </summary>
    private void ShowRegister()
    {
        if (registerPanelActive)
        {
            loginPanel.SetActive(false);
            registerPanel.SetActive(false);
            loginPanelActive = false;
            registerPanelActive = false;
            return;
        }
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        loginPanelActive = false;
        registerPanelActive = true;


        SetLoginMsg("");
        SetRegisterMsg("");
    }

    /// <summary>
    /// 多重送信を防ぐためにUIをロックする
    /// </summary>
    private void SetBusy(bool busy)
    {
        isBusy = busy;

        btnLoginTab.interactable = !busy;
        btnRegisterTab.interactable = !busy;
        btnLoginSubmit.interactable = !busy;
        btnRegisterSubmit.interactable = !busy;
    }

    private void SetLoginMsg(string msg) => txtLoginMsg.text = msg;
    private void SetRegisterMsg(string msg) => txtRegisterMsg.text = msg;
    #endregion

    #region Button Handlers
    /// <summary>
    /// ログインボタン押下
    /// </summary>
    private void OnClickLogin()
    {
        if (isBusy) return;

        string id = loginEmailOrUser.text.Trim();
        string pass = loginPassword.text;

        if (string.IsNullOrEmpty(id))
        {
            SetLoginMsg("メールアドレス or ユーザーネームを入力してください");
            return;
        }
        if (string.IsNullOrEmpty(pass))
        {
            SetLoginMsg("パスワードを入力してください");
            return;
        }

        SetBusy(true);
        SetLoginMsg("ログイン中...");

        bool looksEmail = IsEmailLike(id);

        // メールっぽいならメールログイン、それ以外はUsernameログイン
        if (loginWithEmailPreferred && looksEmail)
        {
            LoginWithEmail(id, pass);
        }
        else
        {
            // どっちも試したい場合は、まずメール→失敗したらUsername にする実装も可能
            LoginWithUsername(id, pass);
        }
    }

    /// <summary>
    /// 登録ボタン押下
    /// </summary>
    private void OnClickRegister()
    {
        if (isBusy) return;

        string email = regEmail.text.Trim();
        string user = regUsername.text.Trim();
        string pass = regPassword.text;
        string pass2 = regPasswordConfirm.text;

        if (!IsEmailLike(email))
        {
            SetRegisterMsg("メールアドレスの形式が正しくありません");
            return;
        }
        if (string.IsNullOrEmpty(user) || user.Length > maxPlayFabUsernameLength)
        {
            SetRegisterMsg($"ユーザーネームは1文字以上{maxPlayFabUsernameLength}文字以下にしてください");
            return;
        }
        if (string.IsNullOrEmpty(pass) || pass.Length < minPasswordLength)
        {
            SetRegisterMsg($"パスワードは {minPasswordLength} 文字以上にしてください");
            return;
        }
        if (pass != pass2)
        {
            SetRegisterMsg("パスワード（確認）が一致しません");
            return;
        }

        SetBusy(true);
        SetRegisterMsg("登録中...");

        Register(email, user, pass);
    }
    #endregion

    #region PlayFab Calls
    /// <summary>
    /// 新規登録（Email + Username + Password）
    /// </summary>
    private void Register(string email, string username, string password)
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
                SetRegisterMsg("登録OK!ログインしてください。");
                SetBusy(false);

                // 任意：表示名をユーザーネームに揃える
                UpdateDisplayName(username);

                // 登録後にログイン画面へ移動するなら:
                // ShowLogin();
            },
            error =>
            {
                SetRegisterMsg(ToUserFriendlyError(error));
                SetBusy(false);
            }
        );
    }

    /// <summary>
    /// メールアドレスでログイン
    /// </summary>
    private void LoginWithEmail(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(
            request,
            result =>
            {
                SetLoginMsg("ログインOK!");
                SetBusy(false);

                Debug.Log($"ログイン成功. PlayFabId={result.PlayFabId}");
                // TODO: 次のシーンへ遷移など
            },
            error =>
            {
                SetLoginMsg(ToUserFriendlyError(error));
                SetBusy(false);
            }
        );
    }

    /// <summary>
    /// ユーザーネームでログイン
    /// </summary>
    private void LoginWithUsername(string username, string password)
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(
            request,
            result =>
            {
                SetLoginMsg("ログインOK!");
                SetBusy(false);

                Debug.Log($"ログイン成功. PlayFabId={result.PlayFabId}");
                // TODO: 次のシーンへ遷移など
            },
            error =>
            {
                SetLoginMsg(ToUserFriendlyError(error));
                SetBusy(false);
            }
        );
    }

    /// <summary>
    /// 表示名更新（任意）
    /// </summary>
    private void UpdateDisplayName(string displayName)
    {
        var req = new UpdateUserTitleDisplayNameRequest { DisplayName = displayName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(
            req,
            _ => { },
            _ => { } // 失敗しても致命ではない
        );
    }
    #endregion

    #region Helpers
    /// <summary>
    /// メール形式っぽいかをざっくり判定
    /// </summary>
    private bool IsEmailLike(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return Regex.IsMatch(text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    /// <summary>
    /// エラーをユーザー向けに丸める
    /// </summary>
    private string ToUserFriendlyError(PlayFabError error)
    {
        // 代表的なものだけ分岐。足りなければ増やせる。
        return error.Error switch
        {
            PlayFabErrorCode.InvalidEmailAddress => "メールアドレスが正しくありません",
            PlayFabErrorCode.EmailAddressNotAvailable => "このメールアドレスは既に使われています",
            PlayFabErrorCode.UsernameNotAvailable => "このユーザーネームは既に使われています",
            PlayFabErrorCode.InvalidPassword => "パスワードが無効です（短すぎる等）",
            PlayFabErrorCode.InvalidUsername => "ユーザーネームが無効です",
            PlayFabErrorCode.InvalidEmailOrPassword => "メール/パスワードが違います",
            PlayFabErrorCode.AccountNotFound => "アカウントが見つかりません",
            _ => $"エラー: {error.ErrorMessage}"
        };
    }
    #endregion
}
