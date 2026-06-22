using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class NotesPool : MonoBehaviour
{
    #region Singleton
    public static NotesPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Initialize();
    }
    #endregion



    #region Variables
    [Header("変数")]
    public ObjectPool<GameObject> pool;
    [SerializeField] public GameObject notesPrefab;
    int notesInitNum = 3;
    int notesMax = 15;
    float posY;
    #endregion

    #region Initialize
    private void Initialize()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: OnCreateObject,                 //第1関数：プールにオブジェクトがない場合オブジェクト生成(Instantiate)する
            actionOnGet: OnGetObject,                   //第2関数：プールに使用していないオブジェクトがある場合はプールから出す。SetActive(true)する
            actionOnRelease: OnReturnedObject,          //第3関数：プールに返却する
            actionOnDestroy: OnDestroyObject,           //第4関数：プールの許容量を超えた時にオブジェクトを削除する
            collectionCheck: false,                     //既にプールにあるオブジェトを追加した場合に例外とするか。エディタでのみ実行される
            defaultCapacity: notesInitNum,               //初期のプールサイズ
            maxSize: notesMax                            //最大プールサイズ
            );

        posY = notesPrefab.transform.localPosition.y;
    }
    #endregion

    #region Create Methods
    //第1関数
    //プールにオブジェクトがない場合オブジェクト生成(Instantiate)する
    //Get()の時に呼ばれる
    GameObject OnCreateObject()
    {
        GameObject obj = Instantiate(notesPrefab);
        return obj;
    }

    //第2関数
    //プールに使用していないオブジェクトがある場合はプールから出す。SetActive(true)する
    // Get()の時に呼ばれる
    void OnGetObject(GameObject obj)
    {
        obj.SetActive(true);//ショットを再利用

        // 位置リセット
        Vector3 pos = obj.transform.localPosition;
        pos.y = posY;
        obj.transform.localPosition = pos;
    }

    //第3関数
    //プールに返却する
    void OnReturnedObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    //第4関数
    //プールの最大許容量を超えた時にオブジェクトを自動で削除する
    void OnDestroyObject(GameObject obj)
    {
        Destroy(obj);
    }

    public GameObject Spawn()
    {
        return pool.Get();
    }

    public void Despawn(GameObject obj)
    {
        pool.Release(obj);
    }
    #endregion
}
