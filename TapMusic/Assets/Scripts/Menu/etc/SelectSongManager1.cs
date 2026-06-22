// using System;
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
//     private Dictionary<Button, bool> buttonClickedFlags = new Dictionary<Button, bool>();   // ボタンフラグ

//     [Header("Audio")]
//     [SerializeField] AudioSource audioSource;

//     [SerializeField] List<SongData> songList = new List<SongData>();                        // 曲データ君
//     //[SerializeField] List<TextAsset> jsonData = new List<TextAsset>();   // json
//     #endregion


//     private const string AddressableLabel = "SongData";

//     void SetList()
//     {
//         // for (var i = 0; i < jsonData.Count; i++)
//         // {

//         //     // Assets/Resources配下のtest.txtの中身を文字列に変換する。
//         //     string inputString = jsonData[i].ToString();
//         //     // 読み取った文字列をオブジェクト型に変換
//         //     songList[i] = JsonUtility.FromJson<SongData>(inputString);

//         //     var temp = Instantiate(prefabButtonTemplate, imageScroll.transform);
//         //     temp.transform.Find("SongName").GetComponent<Text>().text = songList[i].name.ToString();
//         //     temp.transform.Find("SongLevel").GetComponent<Text>().text = songList[i].difficulty.hard.ToString();
//         // }


//         string basePath = Path.Combine(Application.dataPath, "AddressableAssets/SongData");
//         if (!Directory.Exists(basePath))
//         {
//             Debug.LogError("SongData フォルダが存在しません");
//             return;
//         }

//         // 各曲フォルダを検索
//         string[] subDirs = Directory.GetDirectories(basePath);

//         foreach (string dir in subDirs)
//         {
//             string jsonPath = Path.Combine(dir, "SongData.json");

//             if (File.Exists(jsonPath))
//             {
//                 string jsonText = File.ReadAllText(jsonPath);
//                 SongData song = JsonUtility.FromJson<SongData>(jsonText);

//                 // jacketFileName が JSON に含まれている前提
//                 string jacketPath = Path.Combine(dir, song.jacketName);

//                 if (File.Exists(jacketPath))
//                 {
//                     byte[] imageBytes = File.ReadAllBytes(jacketPath);
//                     Texture2D texture = new Texture2D(2, 2);
//                     texture.LoadImage(imageBytes);

//                     song.jacketImage = Sprite.Create(texture,
//                         new Rect(0, 0, texture.width, texture.height),
//                         new Vector2(0.5f, 0.5f));
//                 }
//                 songList.Add(song);

//                 CreateSongButtonA(song);
//             }
//             else
//             {
//                 Debug.LogWarning($"data.json が存在しません: {dir}");
//             }
//         }

//         Debug.Log($"読み込んだ曲数: {songList.Count}");
//     }

//     private void CreateSongButtonA(SongData song)
//     {
//         var temp = Instantiate(prefabButtonTemplate, imageScroll.transform);
//         temp.transform.Find("SongName").GetComponent<Text>().text = song.name;
//         temp.transform.Find("SongLevel").GetComponent<Text>().text = song.difficulty.hard.ToString("0.0");

//         Button button = temp.GetComponent<Button>();
//         if (button != null)
//         {
//             buttonClickedFlags[button] = false;

//             button.onClick.AddListener(() =>
//             {
//                 foreach (var key in buttonClickedFlags.Keys)
//                     buttonClickedFlags[key] = false;

//                 buttonClickedFlags[button] = true;

//                 if (song.jacketImage != null)
//                     songInterface.transform.Find("Jacket").GetComponent<Image>().sprite = song.jacketImage;

//                 songInterface.transform.Find("SongName").GetComponent<Text>().text = song.name;
//                 songInterface.transform.Find("ComposerName").GetComponent<Text>().text = song.composer;
//                 songInterface.transform.Find("BPM").GetComponent<Text>().text = "BPM : " + song.bpm;

//                 // ここでAudioClip再生したければ追加可能（あとで説明）
//             });
//         }
//     }



//     private bool isAddressablesInitialized = false;
//     private Task loadTask;

//     private async void Awake()
//     {
//         if (!isAddressablesInitialized)
//         {
//             Debug.Log("Addressables 初期化中...");
//             await Addressables.InitializeAsync().Task;
//             isAddressablesInitialized = true;
//             Debug.Log("Addressables 初期化完了");
//         }

//         // 事前に楽曲データをロード開始（非同期）
//         loadTask = LoadSongsAsync();
//     }

//     private async void Start()
//     {
//         // 楽曲データ読み込みが完了するまで待機
//         if (loadTask != null)
//             await loadTask;

//         Debug.Log("楽曲データのロード完了。UI初期化開始。");
//     }

//     private async Task LoadSongsAsync()
//     {
//         // 指定ラベルでJSONテキストアセットを一括読み込み
//         var jsonHandle = Addressables.LoadAssetsAsync<UnityEngine.TextAsset>(AddressableLabel, null);
//         await jsonHandle.Task;

//         if (jsonHandle.Status != AsyncOperationStatus.Succeeded)
//         {
//             Debug.LogError("JSONファイルの読み込みに失敗しました");
//             return;
//         }

//         foreach (var jsonAsset in jsonHandle.Result)
//         {
//             SongData song = JsonUtility.FromJson<SongData>(jsonAsset.text);
//             if (song == null)
//             {
//                 Debug.LogWarning("JSONのパースに失敗したデータがあります");
//                 continue;
//             }

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

//             songList.Add(song);

//             // UIにボタン作成
//             CreateSongButton(song);
//         }

//         Debug.Log($"合計で {songList.Count} 曲の読み込みが完了しました");
//     }

//     private void CreateSongButton(SongData song)
//     {
//         var btnObj = Instantiate(prefabButtonTemplate, imageScroll.transform);
//         btnObj.transform.Find("SongName").GetComponent<Text>().text = song.name;
//         btnObj.transform.Find("SongLevel").GetComponent<Text>().text = song.difficulty.hard.ToString("0.0");

//         var button = btnObj.GetComponent<UnityEngine.UI.Button>();
//         button.onClick.AddListener(() =>
//         {
//             DisplaySongInfo(song);
//             PlaySongPreview(song);
//         });
//     }

//     private void DisplaySongInfo(SongData song)
//     {
//         songInterface.transform.Find("SongName").GetComponent<Text>().text = song.name;
//         songInterface.transform.Find("ComposerName").GetComponent<Text>().text = song.composer;
//         songInterface.transform.Find("BPM").GetComponent<Text>().text = $"BPM : {song.bpm}";

//         var jacketImage = songInterface.transform.Find("Jacket").GetComponent<Image>();
//         jacketImage.sprite = song.jacketImage;
//     }
//     private void PlaySongPreview(SongData song)
//     {
//         if (song.audioClip == null)
//         {
//             Debug.LogWarning("AudioClipがロードされていません");
//             return;
//         }

//         audioSource.Stop();
//         audioSource.clip = song.audioClip;
//         audioSource.volume = 0.3f;
//         audioSource.mute = false;
//         audioSource.Play();

//         Debug.Log($"音声再生開始: {song.name}");
//     }
// }