using System.Collections.Generic;
using UnityEngine;

public class SettingNotesManager : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private NotesPool notesPool;
    [SerializeField] private Transform spawnBase;          // 判定バー(基準点)
    [SerializeField] private float intervalSec = 1.0f;     // 1秒に1回
    [SerializeField] private int previewCount = 5;         // 常に保持するノーツ数
    [SerializeField] private bool forwardZ = true;         // true: +Z方向に遠ざかる(奥) / false: -Z方向(手前)
    #endregion

    #region Variables
    public float VisualSongTime => visualTime;
    public Vector3 SpawnBasePos => spawnBase != null ? spawnBase.position : Vector3.zero;

    private float visualTime = 0f;
    private float currentSpeed = 6.0f;

    private readonly Queue<SettingNotes> notesQueue = new();
    #endregion

    #region Unity Methods
    void Start()
    {
        float initSpeed = currentSpeed;

        if (Handover.Instance != null && Handover.Instance.gameSetting != null)
            initSpeed = Handover.Instance.gameSetting.NotesSpeed;

        InitPreview(initSpeed);
    }

    void Update()
    {
        visualTime += Time.deltaTime;

        // 先頭が判定バー(=spawnBase)を通り過ぎたら末尾へ回す
        while (notesQueue.Count > 0)
        {
            var head = notesQueue.Peek();
            if (head == null)
            {
                notesQueue.Dequeue();
                continue;
            }

            if (head.NoteTime < visualTime)
            {
                notesQueue.Dequeue();
                ReuseToTail(head);
                notesQueue.Enqueue(head);
            }
            else
            {
                break;
            }
        }
    }
    #endregion

    #region Init / Reuse
    public void InitPreview(float speed)
    {
        currentSpeed = Mathf.Max(0.0001f, speed);
        visualTime = 0f;

        while (notesQueue.Count > 0)
        {
            var n = notesQueue.Dequeue();
            if (n != null) notesPool.Despawn(n.gameObject);
        }

        for (int i = 0; i < previewCount; i++)
        {
            float t = i * intervalSec;
            var note = SpawnAtTime(t);
            notesQueue.Enqueue(note);
        }
    }

    private SettingNotes SpawnAtTime(float noteTime)
    {
        GameObject obj = notesPool.Spawn();

        var note = obj.GetComponent<SettingNotes>();
        note.Init(noteTime, currentSpeed, this);

        obj.transform.position = CalcWorldPos(noteTime);
        return note;
    }

    private void ReuseToTail(SettingNotes note)
    {
        float lastTime = visualTime;

        foreach (var n in notesQueue)
        {
            if (n == null) continue;
            lastTime = Mathf.Max(lastTime, n.NoteTime);
        }

        float newTime = lastTime + intervalSec;

        note.Init(newTime, currentSpeed, this);
        note.transform.position = CalcWorldPos(newTime);
    }
    #endregion

    #region Position Calc
    /// <summary>
    /// spawnBase を基準に、(noteTime - visualTime) * speed でZ位置を計算する
    /// noteTime == visualTime のとき、spawnBase位置(判定バー)に来る
    /// </summary>
    private Vector3 CalcWorldPos(float noteTime)
    {
        Vector3 basePos = SpawnBasePos;

        float diff = noteTime - visualTime; // 未来ほどプラス
        float dir = forwardZ ? 1f : -1f;

        float z = basePos.z + (diff * currentSpeed * dir);

        return new Vector3(basePos.x, basePos.y, z);
    }
    #endregion

    #region Setting
    /// <summary>
    /// Settingから速度を反映（既存ノーツにも適用）
    /// </summary>
    public void SetNotesSpeed(float speed)
    {
        currentSpeed = Mathf.Max(0.0001f, speed);

        foreach (var n in notesQueue)
        {
            if (n == null) continue;

            n.SetSpeed(currentSpeed);
            n.transform.position = CalcWorldPos(n.NoteTime);
        }
    }
    #endregion
}
