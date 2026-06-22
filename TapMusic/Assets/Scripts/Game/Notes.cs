using UnityEngine;

public class Notes : MonoBehaviour
{
    #region Variables
    private float noteTime;            // 判定時刻（秒）
    private float noteSpeed;           // 移動速度
    private NotesManager manager;
    #endregion

    #region Init
    /// <summary>
    /// ノーツ初期化（DSP基準）
    /// </summary>
    public void Init(float time, float speed, NotesManager notesManager)
    {
        noteTime = time;
        noteSpeed = speed;
        manager = notesManager;
    }

    /// <summary>
    /// 速度を後から変更（Setting反映用）
    /// </summary>
    public void SetSpeed(float speed)
    {
        noteSpeed = speed;
    }
    #endregion

    void Update()
    {
        if (manager == null) return;

        // ★ isPlaying で止めない
        float songTime = manager.VisualSongTime;

        float diff = noteTime - songTime;
        float z = diff * noteSpeed;

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            z
        );
    }
}
