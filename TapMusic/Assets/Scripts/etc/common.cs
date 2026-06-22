using System;
using System.Diagnostics;
using UnityEngine;

namespace common
{
    #region 基本設定
    // 定数
    public class Const
    {
        // ノーツスピードの最大値
        public const float MAX_NOTES_SPEED = 80.0f;

        // ノーツスピードの最小値
        public const float MIN_NOTES_SPEED = 5.0f;

        // ノーツオフセットの最大値
        public const float MAX_NOTES_OFFSET = 500f;

        // ノーツオフセットの最小値
        public const float MIN_NOTES_OFFSET = -500f;

        // 音楽オフセットの最大値
        public const float MAX_MUSIC_OFFSET = 500f;

        // 音楽オフセットの最小値
        public const float MIN_MUSIC_OFFSET = -500f;



        // リセット
        public const float RESET_VALUE = 0.0f;
        public const float RESET_NOTES_SPEED = 15.0f;
        public const float RESET_NOTES_OFFSET = 0f;
        public const float RESET_MUSIC_OFFSET = 200f;

        // 設定ファイル名
        public const string SETTING_FILE_NAME = "Setting.json";
        public const string SOUND_SETTINGS_FILE_NAME = "SoundSettings.json";
        public const string SCORE_DATA_FILE_NAME = "Score.json";
        public const string SONG_SELECT_INFO_FILE_NAME = "SongSelectInfo.json";



    }
    #endregion








    #region 曲の情報関連
    // 曲情報
    [Serializable]
    public class SongData
    {
        public string name;             // 曲名
        public string composer;         // 作曲者
        public string bpm;
        public string audioName;
        public DifficultyLevel difficulty;   // 難易度
        public string jacketName;   // ジャケット名
        public Sprite jacketImage; // ジャケット
        public AudioClip audioClip;
        public bool isLoaded;
        public Action onLoaded;

    }

    // 難易度情報
    [Serializable]
    public class DifficultyLevel
    {
        public float easy;
        public float normal;
        public float hard;

        // Difficultyに応じて値を返す
        public float GetValue(Difficulty diff)
        {
            return diff switch
            {
                Difficulty.Easy => easy,
                Difficulty.Normal => normal,
                Difficulty.Hard => hard,
                _ => hard
            };
        }
    }
    #endregion

    #region 譜面データ関連
    [Serializable]
    public class DataSheet
    {
        public string name;
        public int maxBlock;
        public int BPM;
        public int offset;
        public Note[] notes;
        // public bpmEvents[] bpmEvents;

    }
    [Serializable]
    public class Note
    {
        public int type;
        public int num;
        public int block;
        public int LPB;
        // public float hs;

    }
    [Serializable]
    public class bpmEvents
    {
        public int measure;
        public int bpm;
    }
    #endregion

    // 難易度enum
    [Serializable]
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard

    }

    // ゲーム設定
    [Serializable]
    public class GameSetting
    {
        public float NotesSpeed;
        public float NotesOffset;
        public float MusicOffset;

        public GameSetting()
        {
            NotesSpeed = Const.RESET_NOTES_SPEED;
            NotesOffset = Const.RESET_NOTES_OFFSET;
            MusicOffset = Const.RESET_MUSIC_OFFSET;
        }
    }

    // 設定の種類enum
    [Serializable]
    public enum SettingType
    {
        [InspectorName("表示ボタン")] SettingSwitch,
        [InspectorName("ノーツスピード")] NotesSpeed,
        [InspectorName("ノーツオフセット")] NotesOffset,
        [InspectorName("音楽オフセット")] MusicOffset,
        [InspectorName("リセット")] Reset,
    }

    // 設定ボタン
    [Serializable]
    public class SettingButton
    {
        public SettingType settingType;
        public GameObject settingTmpObj;


        // SettingSwitchが選択されているときに設定できるように

        public bool settingOn = false;
        public bool settingOff = false;
    }




    #region スコア関連
    // 全曲のスコアデータ
    [Serializable]
    public class AllScoreData
    {
        public SongScoreData[] songScoreData;
    }

    // 各曲のスコアデータ
    [Serializable]
    public class SongScoreData
    {
        public string songName;
        public DifficultyScoreData[] difficultyScoreData;
    }

    // 各難易度のスコアデータ
    [Serializable]
    public class DifficultyScoreData
    {
        public Difficulty difficulty;   // 難易度
        public int score;               // スコア
        public int maxCombo;            // 最大コンボ数
        public int goodCount;           // 良の数
        public int okCount;             // 可の数
        public int missCount;           // ミス数
    }
    #endregion

    #region 曲選択画面関連
    // 曲選択画面で使用する曲情報
    public class SongSelectInfo
    {
        public SongData lastSelectedSong;
        public Difficulty lastSelectedDifficulty;
        public SongSortType lastSortType;
        public SortOrder lastSortOrder;
    }

    public enum SongSortType
    {
        ByName,             // 曲名順
        ByDifficulty,       // 難易度順（Easy→Normal→Hard）
    }

    public enum SortOrder
    {
        Ascending,          // 昇順
        Descending,         // 降順
    }



    #endregion



}