using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
#region Variables
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
#endregion

#region Unity Methods
    private void Start()
    {
        SoundSettings settings = SoundManager.Instance.GetSettings();

        masterSlider.value = settings.masterVolume;
        bgmSlider.value = settings.bgmVolume;
        seSlider.value = settings.seVolume;

        masterSlider.onValueChanged.AddListener(_ => Apply());
        bgmSlider.onValueChanged.AddListener(_ => Apply());
        seSlider.onValueChanged.AddListener(_ => Apply());
    }
#endregion

#region Private Methods
    private void Apply()
    {
        SoundManager.Instance.SetVolume(
            masterSlider.value,
            bgmSlider.value,
            seSlider.value
        );
    }
#endregion
}
