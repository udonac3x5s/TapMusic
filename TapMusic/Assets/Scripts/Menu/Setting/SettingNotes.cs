using UnityEngine;

public class SettingNotes : MonoBehaviour
{
    #region Variables
    private float noteTime;                 // 判定時刻（秒）
    private float noteSpeed;                // 速度
    private SettingNotesManager manager;    // 参照
    public float NoteTime => noteTime;
    #endregion

    #region Init
    /// <summary>
    /// ノーツ初期化（プレビュー用）
    /// </summary>
    public void Init(float time, float speed, SettingNotesManager notesManager)
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

        float songTime = manager.VisualSongTime;
        float diff = noteTime - songTime;

        // 判定位置は spawnBase（Zも含む）を基準にする
        Vector3 basePos = manager.SpawnBasePos;

        // forwardZ は Manager 内の計算に合わせたいので、同じ方向判定をここでは再利用せず
        // Manager側が forwardZ を持っている場合は公開して使ってもOK
        // ここでは「Managerと同じ forwardZ を使う」ために、Managerで計算済みのSpawnBasePosと
        // diff*speed の向きだけ合わせる（下の1行を Manager の forwardZ に合わせて切替）

        // forwardZ = true の場合:
        float z = basePos.z + diff * noteSpeed;

        // forwardZ = false にしたいなら↑を↓に置き換え
        // float z = basePos.z - diff * noteSpeed;

        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }
}
