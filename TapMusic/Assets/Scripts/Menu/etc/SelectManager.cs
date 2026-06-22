// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using System.Linq;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using System.Threading.Tasks;

// [Serializable]
// class SongData
// {
//     public string name;             // 曲名
//     public string composer;         // 作曲者
//     public string bpm;
//     public string audioName;        // ファイル名（mp3, wav など）
//     public Difficulty difficulty;   // 難易度
//     public string jacketName;       // ファイル名（png, jpg など）

//     [NonSerialized] public Sprite jacketImage;   // 実際にロードされたジャケット
//     [NonSerialized] public AudioClip audioClip;  // 実際にロードされた音源
// }

// [Serializable]
// class Difficulty
// {
//     public float easy;
//     public float normal;
//     public float hard;
// }
// public class SelectManager : MonoBehaviour
// {
//     #region プライベート変数
//     [SerializeField] GameObject prefabButtonTemplate;  // ボタン君
//     [SerializeField] GameObject imageScroll;           // スクロールするところ
//     [SerializeField] GameObject songInterface;         // 曲情報パネル

//     private Dictionary<Button, bool> buttonClickedFlags = new();

//     [Header("Audio")]
//     [SerializeField] AudioSource audioSource;

//     private List<SongData> songList = new();           // 曲データ君
//     #endregion


//     /// <summary>
//     /// 曲データのロード
//     /// </summary>
//     private async Task LoadSongsAsync()
//     {
//         // JSON をラベル "SongData" で全部ロード
//         var handle = Addressables.LoadAssetsAsync<TextAsset>("SongData", null);
//         await handle.Task;

//         foreach (var json in handle.Result)
//         {
//             SongData song = JsonUtility.FromJson<SongData>(json.text);

//             // ジャケットロード (ファイル名そのまま)
//             if (!string.IsNullOrEmpty(song.jacketName))
//             {
//                 var jacketHandle = Addressables.LoadAssetAsync<Sprite>(song.jacketName);
//                 await jacketHandle.Task;
//                 song.jacketImage = jacketHandle.Status == AsyncOperationStatus.Succeeded
//                     ? jacketHandle.Result
//                     : null;
//             }

//             // AudioClipロード (ファイル名そのまま)
//             if (!string.IsNullOrEmpty(song.audioName))
//             {
//                 var audioHandle = Addressables.LoadAssetAsync<AudioClip>(song.audioName);
//                 await audioHandle.Task;
//                 song.audioClip = audioHandle.Status == AsyncOperationStatus.Succeeded
//                     ? audioHandle.Result
//                     : null;
//             }

//             songList.Add(song);
//             CreateSongButton(song);
//         }

//         Debug.Log($"読み込んだ曲数: {songList.Count}");
//     }

//     /// <summary>
//     /// ボタン生成
//     /// </summary>
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

//                     // プレビュー再生
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
//         await LoadSongsAsync();
//     }
// }