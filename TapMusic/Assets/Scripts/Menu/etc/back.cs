// using UnityEngine;

// public class back : MonoBehaviour
// {
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
        
//     }using System;
// using System.IO;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using System.Linq; // ← 上部に追加！
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using System.Threading.Tasks;

// [Serializable]
// class SongData
// {
//     public string name;             // 曲名
//     public string composer;         // 作曲者
//     public string bpm;
//     public string audioName;
//     public Difficulty difficulty;   // 難易度
//     public string jacketName;   // ジャケット名
//     public Sprite jacketImage; // ジャケット
//     public AudioClip audioClip;

// }

// [Serializable]
// class Difficulty
// {
//     public float easy;
//     public float normal;
//     public float hard;

// }

// public class SelectSongManager : MonoBehaviour
// {
//     #region  プライベート変数
//     [SerializeField] GameObject prefabButtonTemplate;                                       // ボタン君
//     [SerializeField] GameObject imageScroll;                                                // スクロールするところ
//     [SerializeField] GameObject songInterface;                                              // 曲情報
//     private Dictionary<Button, bool> buttonClickedFlags = new();   // ボタンフラグ

//     [Header("Audio")]
//     [SerializeField] AudioSource audioSource;

//     [SerializeField] List<SongData> songList = new();                        // 曲データ君
//     //[SerializeField] List<TextAsset> jsonData = new List<TextAsset>();   // json
//     #endregion


//     //private const string AddressableLabel = "SongData";

//     // private async Task  SetList()
//     // {
//     //     // for (var i = 0; i < jsonData.Count; i++)
//     //     // {

//     //     //     // Assets/Resources配下のtest.txtの中身を文字列に変換する。
//     //     //     string inputString = jsonData[i].ToString();
//     //     //     // 読み取った文字列をオブジェクト型に変換
//     //     //     songList[i] = JsonUtility.FromJson<SongData>(inputString);

//     //     //     var temp = Instantiate(prefabButtonTemplate, imageScroll.transform);
//     //     //     temp.transform.Find("SongName").GetComponent<Text>().text = songList[i].name.ToString();
//     //     //     temp.transform.Find("SongLevel").GetComponent<Text>().text = songList[i].difficulty.hard.ToString();
//     //     // }

//     //     // パス読み込み
//     //     string basePath = Path.Combine(Application.dataPath, "AddressableAssets/SongData");
//     //     if (!Directory.Exists(basePath))
//     //     {
//     //         Debug.LogError("SongData フォルダが存在しません");
//     //         return;
//     //     }

//     //     // 各曲フォルダを検索
//     //     string[] subDirs = Directory.GetDirectories(basePath);

//     //     foreach (string dir in subDirs)
//     //     {
//     //         string jsonPath = Path.Combine(dir, "SongData.json");

//     //         if (File.Exists(jsonPath))
//     //         {
//     //             string jsonText = File.ReadAllText(jsonPath);
//     //             SongData song = JsonUtility.FromJson<SongData>(jsonText);

//     //             // jacketFileName が JSON に含まれている前提
//     //             string jacketPath = Path.Combine(dir, song.jacketName);

//     //             if (File.Exists(jacketPath))
//     //             {
//     //                 byte[] imageBytes = File.ReadAllBytes(jacketPath);
//     //                 Texture2D texture = new Texture2D(2, 2);
//     //                 texture.LoadImage(imageBytes);

//     //                 song.jacketImage = Sprite.Create(texture,
//     //                     new Rect(0, 0, texture.width, texture.height),
//     //                     new Vector2(0.5f, 0.5f));
//     //             }

//     //             // AudioClip読み込み
//     //             string audioPath = Path.Combine(dir, song.audioName);
//     //             if (File.Exists(audioPath))
//     //             {
//     //                 //var handle = Addressables.LoadAssetAsync<AudioResource>(audioPath);
//     //                 //song.audioResource = handle.Result;
//     //             }
//     //             else
//     //             {
//     //                 Debug.LogWarning($"Audioファイルが見つかりません: {audioPath}");
//     //             }
//     //             songList.Add(song);

//     //             CreateSongButton(song);
//     //         }
//     //         else
//     //         {
//     //             Debug.LogWarning($"data.json が存在しません: {dir}");
//     //         }
//     //     }

//     //     Debug.Log($"読み込んだ曲数: {songList.Count}");
//     // }


//     /// <summary>
//     /// 曲データを Addressables から読み込み
//     /// </summary>
//     private async Task SetList()
//     {
//         // "SongData" ラベルが付いた TextAsset を全部ロード
//         var handle = Addressables.LoadAssetsAsync<TextAsset>("SongData", null);
//         await handle.Task;

//         foreach (var json in handle.Result)
//         {
//             // JSON を SongData に変換
//             SongData song = JsonUtility.FromJson<SongData>(json.text);

//             string folderName = song.name; // フォルダ名は曲名に合わせている想定

//             // ジャケット画像のロード
//             string jacketKey = $"SongData/{folderName}/{song.jacketName}";
//             var jacketHandle = Addressables.LoadAssetAsync<Texture2D>(jacketKey);
//             await jacketHandle.Task;
//             if (jacketHandle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 Texture2D tex = jacketHandle.Result;
//                 song.jacketImage = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
//             }
//             else
//             {
//                 Debug.LogWarning($"ジャケット画像の読み込みに失敗: {jacketKey}");
//             }

//             // AudioClipのロード
//             string audioKey = $"SongData/{folderName}/{song.audioName}";
//             var audioHandle = Addressables.LoadAssetAsync<AudioClip>(audioKey);
//             await audioHandle.Task;
//             if (audioHandle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 song.audioClip = audioHandle.Result;
//             }
//             else
//             {
//                 Debug.LogWarning($"音声ファイルの読み込みに失敗: {audioKey}");
//             }

//             // // AudioClipロード
//             // if (!string.IsNullOrEmpty(song.audioName))
//             // {
//             //     var audioHandle = Addressables.LoadAssetAsync<AudioClip>(song.audioName);
//             //     await audioHandle.Task;
//             //     if (audioHandle.Status == AsyncOperationStatus.Succeeded)
//             //         song.audioResource = audioHandle.Result;
//             // }

//             songList.Add(song);
//             CreateSongButton(song);
//         }

//         Debug.Log($"読み込んだ曲数: {songList.Count}");
//     }



//     /// <summary>
//     /// ボタン生成
//     /// </summary>
//     /// <param name="song">曲データ</param>
//     private void CreateSongButton(SongData song)
//     {
//         var temp = Instantiate(prefabButtonTemplate, imageScroll.transform);
//         temp.transform.Find("SongName").GetComponent<Text>().text = song.name;
//         temp.transform.Find("SongLevel").GetComponent<Text>().text = song.difficulty.hard.ToString("0.0");

//         if (temp.TryGetComponent<Button>(out var button))
//         {
//             buttonClickedFlags[button] = false;

//             button.onClick.AddListener(() =>
//             {
//                 foreach (var key in buttonClickedFlags.Keys.ToList())
//                 {
//                     if (key != button)
//                         buttonClickedFlags[key] = false;
//                 }

//                 if (buttonClickedFlags[button] != true)
//                 {
//                     buttonClickedFlags[button] = true;

//                     if (song.jacketImage != null)
//                         songInterface.transform.Find("Jacket").GetComponent<Image>().sprite = song.jacketImage;

//                     songInterface.transform.Find("SongName").GetComponent<Text>().text = song.name;
//                     songInterface.transform.Find("ComposerName").GetComponent<Text>().text = song.composer;
//                     songInterface.transform.Find("BPM").GetComponent<Text>().text = "BPM : " + song.bpm;

//                     // 音追加
//                     if (song.audioClip != null)
//                     {
//                         Debug.Log("曲開始");
//                         audioSource.Stop();
//                         audioSource.clip = song.audioClip;
//                         audioSource.volume = 0.3f;
//                         audioSource.Play();
//                     }
//                 }
//                 else
//                 {
//                     Debug.Log("開始");
//                 }

//             });
//         }
//     }

//     private async void Start()
//     {
//         await SetList();
//     }


// }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }
