using UnityEngine;

public class DontDestroyObj : MonoBehaviour
{
    public static DontDestroyObj instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
