using UnityEngine;

public class TestTap : MonoBehaviour
{
    #region Variables
    NotesManager notesManager;

    [SerializeField] private Light tapGlow0;
    [SerializeField] private Light tapGlow1;
    [SerializeField] private Light tapGlow2;
    [SerializeField] private Light tapGlow3;
    #endregion

    #region Unity Methods
    // void OnGUI()
    // {
    //     GUI.Label(new Rect(10, 10, 200, 30),
    //         $"FPS: {(1f / Time.deltaTime):F1}");
    // }

    void Start()
    {
        notesManager = FindFirstObjectByType<NotesManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            notesManager.JudgeLane(0);
            tapGlow0.TriggerGlow();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            notesManager.JudgeLane(1);
            tapGlow1.TriggerGlow();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            notesManager.JudgeLane(2);
            tapGlow2.TriggerGlow();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            notesManager.JudgeLane(3);
            tapGlow3.TriggerGlow();
        }
    }
    #endregion
}
