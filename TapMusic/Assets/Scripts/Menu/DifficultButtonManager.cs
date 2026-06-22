using UnityEngine;
using common;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class DifficultButtonManager : MonoBehaviour
{
    #region 変数
    [SerializeField] public Difficulty difficulty;
    #endregion

    #region Unity Methods
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(()=>
        {
            var selectSongManager = FindAnyObjectByType<SelectSongManager>();
            selectSongManager.nowDifficulty = difficulty;
            selectSongManager.LevelTextUpdate();
            Debug.Log(selectSongManager.nowDifficulty + "が選択されました");

            selectSongManager.OnDifficultyButtonClicked((int)difficulty);

            // Buttonの色を変える
            foreach (Transform child in selectSongManager.DifficultyButtons.transform)
            {
                var button = child.GetComponent<Image>();
                var colors = button.color;
                if (button.GetComponent<Button>() == GetComponent<Button>())
                {
                    colors = new Color32(164, 220, 246, 255); // 選択されたボタンを水色に
                }
                else
                {
                    colors = Color.white; // 他のボタンを白に
                }
                button.color = colors;
            }

            // 現在のソートに基づいて曲リストを再ソート
            selectSongManager.ApplySort(false);

        });
    }
    #endregion
}
