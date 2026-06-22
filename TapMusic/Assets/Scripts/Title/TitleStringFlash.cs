using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TitleStringFlash : MonoBehaviour
{
    TMP_Text textMeshPro;
    float flashSpeed = 4.0f;

    void Start()
    {
        textMeshPro = GetComponent<TMP_Text>();
    }


    void FixedUpdate()
    {
        float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1) / 2; // 0から1の範囲で変化
        Color color = textMeshPro.color;
        color.a = alpha;
        textMeshPro.color = color;
    }





}
