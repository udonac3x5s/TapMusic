using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelUI : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text artistText;
    [SerializeField] Image jacketImage;
    [SerializeField] TMP_Text difficultyText;
    [SerializeField] TMP_Text levelText;

    void Start()
    {
        Initialized();
    }


    void Initialized()
    {
        titleText.text = Handover.Instance.handoverSongData.name;
        artistText.text = Handover.Instance.handoverSongData.composer;
        jacketImage.sprite = Handover.Instance.handoverSongData.jacketImage;
        difficultyText.text = Handover.Instance.nowDifficulty.ToString();
        levelText.text = Handover.Instance.handoverSongData.difficulty.GetValue(Handover.Instance.nowDifficulty).ToString("F1");
    }
}
