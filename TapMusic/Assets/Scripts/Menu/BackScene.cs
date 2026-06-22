using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Button))]
public class BackScene : MonoBehaviour
{
    #region Variables
    bool isTap = false;
    #endregion

    #region Unity Methods
    private void Start()
    {
        SoundManager.Instance.ResetSettings();
        BackToTitle();
    }

    #endregion

    #region Methods
    void BackToTitle()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (isTap) return;
            isTap = true;
            SlideUIGenerate.Instance.SceneMove("Title", true).Forget();
        });
    }
    #endregion
}
