using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

/// <summary>
/// PlayFabのログイン処理を行うクラス
/// </summary>
public class PlayFabLogin : MonoBehaviour
{
    private bool _shouldCreateAccount;      //アカウントを作成するか
    private string _customID;               //ログイン時に使うID
    [SerializeField] string customName = "Player";// 表示名

    //=================================================================================
    //ログイン処理
    //=================================================================================
    public void Start()
    {
        Login();
    }


    //ログイン実行
    private void Login()
    {
        // カスタムIDの取得
        _customID = LoadCustomID();
        //既にアカウントが作成されており、CreateAccountがtrueになっていてもエラーにはならない
        var request = new LoginWithCustomIDRequest { CustomId = _customID, CreateAccount = _shouldCreateAccount };
        // ログイン実行
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    //表示名の設定
    private void SetDisplayName()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = customName
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdateSuccess, OnDisplayNameUpdateFailure);
    }

    //表示名設定成功
    private void OnDisplayNameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("表示名の設定に成功: " + result.DisplayName);
    }

    //表示名設定失敗
    private void OnDisplayNameUpdateFailure(PlayFabError error)
    {
        Debug.LogError("表示名の設定に失敗: " + error.GenerateErrorReport());
    }

    //ログイン成功
    private void OnLoginSuccess(LoginResult result)
    {
        //アカウントを作成しようとしたのに、IDが既に使われていて、出来なかった場合
        if (_shouldCreateAccount == true && result.NewlyCreated == false)
        {
            Debug.LogWarning("CustomId :" + _customID + "は既に使われています。");
            Login();//ログインしなおし
            return;
        }

        //アカウント新規作成できたらIDを保存
        if (result.NewlyCreated == true)
        {
            SaveCustomID();
            Debug.Log("新規作成成功");
        }

        Debug.Log("ログイン成功!!");
        SetDisplayName();//表示名の設定
    }

    //ログイン失敗
    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFabのログインに失敗\n" + error.GenerateErrorReport());
    }




    //=================================================================================
    //カスタムIDの取得
    //=================================================================================

    //IDを保存する時のKEY
    private static readonly string CUSTOM_ID_SAVE_KEY = "CUSTOM_ID_SAVE_KEY";

    //IDを取得
    private string LoadCustomID()
    {
        //IDをセーブデータから取得
        string id = PlayerPrefs.GetString(CUSTOM_ID_SAVE_KEY);

        //idの中身がnullもしくは空の文字列("")の場合は_shouldCreateAccountはtrueになる。
        _shouldCreateAccount = string.IsNullOrEmpty(id);

        //idの中身がない場合、文字列を新規作成
        if (_shouldCreateAccount == true)
        {
            return GenerateCustomID();//文字列を新規作成
        }
        else
        {
            return id;//セーブされた文字列を返す
        }
    }

    //IDの保存
    private void SaveCustomID()
    {
        PlayerPrefs.SetString(CUSTOM_ID_SAVE_KEY, _customID);
    }




    //=================================================================================
    //カスタムIDの生成
    //=================================================================================

    //IDを生成する
    //ユニークな文字列をGuidを使用し生成
    //https://docs.microsoft.com/ja-jp/dotnet/api/system.guid.tostring?redirectedfrom=MSDN&view=netframework-4.8#System_Guid_ToString_System_String_
    private string GenerateCustomID()
    {
        //Guidの構造体生成
        Guid guid = Guid.NewGuid();

        return guid.ToString("N");//書式指定子はNを指定 詳細は「Guid.ToString メソッド」のドキュメント参照
    }
}
