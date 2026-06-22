using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Button))]
public class Pause : MonoBehaviour
{
    #region Variables
    [SerializeField] private NotesManager notesManager;
    [SerializeField] private GameObject pauseUI;
    private bool isPaused = false;
    private bool isTap = false;
    #endregion

    #region Unity Methods
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PauseButton);
        pauseUI.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(ResumeGame);
        pauseUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(RetryGame);
        pauseUI.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(QuitGame);
    }
    #endregion

    #region Pause Methods
    void PauseButton()
    {
        if(isPaused && notesManager.Ended) return;

        if(!notesManager.isSongPlaying) return;

        isPaused = true;
        notesManager.PauseGame();
        pauseUI.SetActive(true);
    }
    #endregion

    #region Pause UI Methods
    void ResumeGame()
    {
        isPaused = false;
        notesManager.ResumeGame();
        pauseUI.SetActive(false);
    }

    void RetryGame()
    {
        if(isTap) return;
        isTap = true;
        // シーン移動
        SlideUIGenerate.Instance.SceneMove("Game");
    }

    void QuitGame()
    {
        if(isTap) return;
        isTap = true;
        // シーン移動
        SlideUIGenerate.Instance.SceneMove("Menu");
    }

    public void RetryResult()
    {
        isTap = false;
        // シーン移動
        SlideUIGenerate.Instance.SceneMove("Game");
    }

    #endregion


}
