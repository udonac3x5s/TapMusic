// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using PlayFab;
// using PlayFab.ClientModels;

// public class EmailVerificationPanel : MonoBehaviour
// {
//     #region Private Variables
//     [Header("UI")]
//     [SerializeField] private GameObject root;     // Panelのルート（このオブジェクトでも可）
//     [SerializeField] private TMP_Text titleText;  // タイトル
//     [SerializeField] private TMP_Text bodyText;   // 説明文
//     [SerializeField] private TMP_Text emailText;  // メール表示（任意）
//     [SerializeField] private TMP_Text msgText;    // 状態メッセージ

//     [SerializeField] private Button btnResend;    // 再送
//     [SerializeField] private Button btnRefresh;   // 再チェック
//     [SerializeField] private Button btnLogout;    // ログアウト（任意）

//     [Header("External")]
//     [SerializeField] private EmailVerificationGate gate; // 再チェック呼び出し先

//     [Header("Options")]
//     [SerializeField] private string cachedEmailForResend = ""; // 登録メール（登録画面から SetEmail で渡す）
//     #endregion

//     #region Unity Lifecycle
//     private void Awake()
//     {
//         if (btnResend != null) btnResend.onClick.AddListener(OnClickResend);
//         if (btnRefresh != null) btnRefresh.onClick.AddListener(OnClickRefresh);
//         if (btnLogout != null) btnLogout.onClick.AddListener(OnClickLogout);

//         if (titleText != null) titleText.text = "メール認証が必要です";

//         if (bodyText != null)
//         {
//             bodyText.text =
//                 "登録したメールアドレスに確認メールを送信しました。\n" +
//                 "メール内のリンクを開いたあと、この画面で「再チェック」を押してください。\n\n" +
//                 "※ 迷惑メールに入ることがあります";
//         }

//         SetMessage("");
//         UpdateEmailLabel();
//     }
//     #endregion

//     /// <summary>
//     /// パネル表示
//     /// </summary>
//     public void Show()
//     {
//         if (root != null) root.SetActive(true);
//         else gameObject.SetActive(true);

//         UpdateEmailLabel();
//     }

//     /// <summary>
//     /// パネル非表示
//     /// </summary>
//     public void Hide()
//     {
//         if (root != null) root.SetActive(false);
//         else gameObject.SetActive(false);
//     }

//     /// <summary>
//     /// 状態メッセージ更新
//     /// </summary>
//     public void SetMessage(string msg)
//     {
//         if (msgText != null) msgText.text = msg;
//     }

//     /// <summary>
//     /// ボタンを有効/無効化
//     /// </summary>
//     public void SetInteractable(bool enable)
//     {
//         if (btnResend != null) btnResend.interactable = enable;
//         if (btnRefresh != null) btnRefresh.interactable = enable;
//         if (btnLogout != null) btnLogout.interactable = enable;
//     }

//     /// <summary>
//     /// 再送に使うメールをセット（登録画面から渡す用）
//     /// </summary>
//     public void SetEmail(string email)
//     {
//         cachedEmailForResend = email?.Trim() ?? "";
//         UpdateEmailLabel();
//     }

//     private void UpdateEmailLabel()
//     {
//         if (emailText == null) return;

//         if (string.IsNullOrEmpty(cachedEmailForResend))
//         {
//             emailText.text = "メール: （未設定）";
//         }
//         else
//         {
//             emailText.text = $"メール: {cachedEmailForResend}";
//         }
//     }

//     /// <summary>
//     /// 「確認メール再送」ボタン
//     /// </summary>
//     private void OnClickResend()
//     {
//         if (!PlayFabClientAPI.IsClientLoggedIn())
//         {
//             SetMessage("未ログインです。先にログインしてください。");
//             return;
//         }

//         if (string.IsNullOrEmpty(cachedEmailForResend))
//         {
//             SetMessage("再送先メールが未設定です（登録時のメールを SetEmail してください）");
//             return;
//         }

//         SetInteractable(false);
//         SetMessage("メールを送信中...");

//         // 再送：アカウント回復メール（環境によっては確認メールとして扱う/テンプレ利用）
//         PlayFabClientAPI.SendAccountRecoveryEmail(
//             new SendAccountRecoveryEmailRequest
//             {
//                 Email = cachedEmailForResend,
//                 TitleId = PlayFabSettings.TitleId
//             },
//             _ =>
//             {
//                 SetMessage("送信しました。メールを確認してください。");
//                 SetInteractable(true);
//             },
//             error =>
//             {
//                 SetMessage($"送信に失敗: {error.ErrorMessage}");
//                 SetInteractable(true);
//             }
//         );
//     }

//     /// <summary>
//     /// 「再チェック」ボタン
//     /// </summary>
//     private void OnClickRefresh()
//     {
//         if (gate == null)
//         {
//             SetMessage("Gate参照が未設定です（Inspectorで gate を設定してください）");
//             return;
//         }

//         SetMessage("再チェック中...");
//         gate.CheckAndApply();
//     }

//     /// <summary>
//     /// 「ログアウト」ボタン（任意）
//     /// </summary>
//     private void OnClickLogout()
//     {
//         PlayFabClientAPI.ForgetAllCredentials();
//         SetMessage("ログアウトしました。");
//         // ここでログイン画面へ戻すなど（あなたのUI構成に合わせて）
//     }
// }
