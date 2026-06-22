#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

/// <summary>
/// SongData.json だけを登録、他のJSONは無視
/// </summary>
public class AutoAddressableImporter
{
    [MenuItem("Tools/Addressables/ソングデータ一括Addressables登録")]
    public static void AutoRegisterSongDataAssets()
    {
        string targetFolder = "Assets/AddressableAssets/SongData";
        string labelName = "SongData";

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables未設定です。Window > Asset Management > Addressables > Groups でセットアップしてください。");
            return;
        }

        // SongData.json だけ登録
        RegisterSpecificJsonsOnly(targetFolder, "SongData.json", labelName, settings);

        // 難易度譜面データ（Easy.json, Normal.json, Hard.json など）も登録
        RegisterDifficultyJsons(targetFolder, labelName, settings);

        // 画像
        RegisterFilesWithExtension(targetFolder, "*.png", labelName, settings);
        RegisterFilesWithExtension(targetFolder, "*.jpg", labelName, settings);

        // 音声
        RegisterFilesWithExtension(targetFolder, "*.wav", labelName, settings);
        RegisterFilesWithExtension(targetFolder, "*.mp3", labelName, settings);
        RegisterFilesWithExtension(targetFolder, "*.ogg", labelName, settings);

        AssetDatabase.SaveAssets();
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log("登録完了: SongData.json / 難易度譜面 / 音声 / 画像");
    }

    // 難易度譜面データ（Easy.json, Normal.json, Hard.json）を登録
    static void RegisterDifficultyJsons(string folder, string label, AddressableAssetSettings settings)
    {
        string[] allJsonPaths = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);
        foreach (string jsonPath in allJsonPaths)
        {
            string assetPath = jsonPath.Replace("\\", "/");
            // SongData.json以外の難易度譜面データのみ
            if (assetPath.EndsWith("SongData.json")) continue;
            if (assetPath.EndsWith("Easy.json") || assetPath.EndsWith("Normal.json") || assetPath.EndsWith("Hard.json"))
            {
                AddToAddressables(assetPath, label, settings);
            }
        }
    }
    static void RegisterFilesWithExtension(string folder, string searchPattern, string label, AddressableAssetSettings settings)
    {
        string ext = searchPattern.Replace("*", "").ToLower();
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { folder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetExtension(path).ToLower() != ext) continue;

            AddToAddressables(path, label, settings);
        }
    }

    static void RegisterSpecificJsonsOnly(string folder, string targetJsonFileName, string label, AddressableAssetSettings settings)
    {
        string[] allJsonPaths = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);
        foreach (string jsonPath in allJsonPaths)
        {
            string assetPath = jsonPath.Replace("\\", "/");
            if (!assetPath.EndsWith(targetJsonFileName)) continue;

            AddToAddressables(assetPath, label, settings);
        }
    }

    static void AddToAddressables(string assetPath, string label, AddressableAssetSettings settings)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid)) return;

        var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
        string address = GetRelativeAddress(assetPath);
        entry.address = address;

        if (!entry.labels.Contains(label))
            entry.SetLabel(label, true);
    }

    static string GetRelativeAddress(string assetPath)
    {
        string baseFolder = Path.GetFullPath("Assets/AddressableAssets");
        string fullPath = Path.GetFullPath(assetPath);
        string relative = fullPath.StartsWith(baseFolder)
            ? fullPath.Substring(baseFolder.Length)
            : assetPath;

        return relative.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/');
    }
}
#endif
