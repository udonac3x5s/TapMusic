// using UnityEngine;
// using PlayFab;
// using PlayFab.ClientModels;

// public class EmailVerificationGate : MonoBehaviour
// {
//     #region Private Variables
//     [Header("References")]
//     [SerializeField] private EmailVerificationPanel verifyPanel; // 認証UIパネル
//     [SerializeField] private GameObject[] enableWhenVerified;    // 認証済みでONにしたいUI
//     [SerializeField] private GameObject[] disableWhenUnverified; // 未認証の間OFFにしたいUI（任意）

//     [Header("Options")]
//     [SerializeField] private bool checkOnStart = true; // Startでチェックするか
//     #endregion

//     #region Unity Lifecycle
//     private void Start()
//     {
//         if (checkOnStart)
//         {
//             CheckAndApply();
//         }
//     }
//     #endregion

//     /// <summary>
//     /// メール認証状態をチェックして、UIを切り替える
//     /// </summary>
//     public void CheckAndApply()
//     {
//         if (verifyPanel == null)
//         {
//             Debug.LogError("EmailVerificationGate: verifyPanel が未設定です");
//             return;
//         }

//         // PlayFabログイン前に呼ばれると困るのでガード
//         if (!PlayFabClientAPI.IsClientLoggedIn())
//         {
//             ApplyUnverified("(未ログイン) 先にログインしてください");
//             verifyPanel.SetInteractable(true);
//             return;
//         }

//         verifyPanel.Show();
//         verifyPanel.SetMessage("認証状態を確認中...");
//         verifyPanel.SetInteractable(false);

//         CheckEmailVerified(
//             isVerified =>
//             {
//                 if (isVerified)
//                 {
//                     ApplyVerified();
//                 }
//                 else
//                 {
//                     ApplyUnverified("メール認証が完了していません");
//                 }

//                 verifyPanel.SetInteractable(true);
//             },
//             errorMessage =>
//             {
//                 ApplyUnverified("認証状態の取得に失敗しました");
//                 verifyPanel.SetMessage(errorMessage);
//                 verifyPanel.SetInteractable(true);
//             }
//         );
//     }

//     /// <summary>
//     /// EmailVerified を取得する（SDK差分に強い：GetPlayerProfile を使用）
//     /// </summary>
//     private void CheckEmailVerified(System.Action<bool> onResult, System.Action<string> onError)
//     {
//         var request = new GetPlayerProfileRequest
//         {
//             ProfileConstraints = new PlayerProfileViewConstraints
//             {
//                 ShowPrivateData = true
//             }
//         };

//         PlayFabClientAPI.GetPlayerProfile(
//             request,
//             result =>
//             {
//                 bool verified = result.PlayerProfile?.EmailVerified ?? false;
//                 onResult?.Invoke(verified);
//             },
//             error =>
//             {
//                 onError?.Invoke(error.GenerateErrorReport());
//             }
//         );
//     }

//     /// <summary>
//     /// 認証済みの状態を反映
//     /// </summary>
//     private void ApplyVerified()
//     {
//         verifyPanel.Hide();

//         if (enableWhenVerified != null)
//         {
//             for (int i = 0; i < enableWhenVerified.Length; i++)
//             {
//                 if (enableWhenVerified[i] != null)
//                     enableWhenVerified[i].SetActive(true);
//             }
//         }

//         if (disableWhenUnverified != null)
//         {
//             for (int i = 0; i < disableWhenUnverified.Length; i++)
//             {
//                 if (disableWhenUnverified[i] != null)
//                     disableWhenUnverified[i].SetActive(true);
//             }
//         }
//     }

//     /// <summary>
//     /// 未認証の状態を反映
//     /// </summary>
//     private void ApplyUnverified(string message)
//     {
//         verifyPanel.Show();
//         verifyPanel.SetMessage(message);

//         if (enableWhenVerified != null)
//         {
//             for (int i = 0; i < enableWhenVerified.Length; i++)
//             {
//                 if (enableWhenVerified[i] != null)
//                     enableWhenVerified[i].SetActive(false);
//             }
//         }

//         if (disableWhenUnverified != null)
//         {
//             for (int i = 0; i < disableWhenUnverified.Length; i++)
//             {
//                 if (disableWhenUnverified[i] != null)
//                     disableWhenUnverified[i].SetActive(false);
//             }
//         }
//     }
// }
