using UnityEngine;
using common;

public class Handover : MonoBehaviour
{

    public static Handover Instance { get; private set; }

    #region パブリック変数
    [SerializeField] public SongData handoverSongData;
    public Difficulty nowDifficulty;
    public DataSheet dataSheet;
    public GameSetting gameSetting;
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // void OnGUI()
    // {
    //     GUI.Label(new Rect(10, 40, 300, 30),
    //         $"NotesSpeed: {gameSetting.NotesSpeed}" + $"  MusicOffset: {gameSetting.MusicOffset}" + $"  NotesOffset: {gameSetting.NotesOffset}"
    //         );
    // }
}
