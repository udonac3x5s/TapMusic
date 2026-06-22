using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[DisallowMultipleComponent]
public class TouchCol : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField] public UnityEvent onTap;

    public void OnPointerClick(PointerEventData eventData)
    {
        // onTap?.Invoke();
    }

    // 押下開始で反応させたい場合はこちらも使える
    public void OnPointerDown(PointerEventData eventData)
    {
        onTap?.Invoke();
    }

    public void DebugLog()
    {
        Debug.Log("message");
    }
}
